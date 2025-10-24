using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using System.ComponentModel;
using System.IO;
using Application = System.Windows.Forms.Application;

namespace Invoice.WinForms.Forms;

public partial class BatchImportDialog : XtraForm
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BatchImportDialog> _logger;
    private readonly IImportInvoiceUseCase _importInvoiceUseCase;

    // DevExpress UI Controls
    private GridControl _fileGridControl = null!;
    private GridView _fileGridView = null!;
    private ProgressBarControl _overallProgressBar = null!;
    private LabelControl _overallStatusLabel = null!;
    private SimpleButton _addFilesButton = null!;
    private SimpleButton _addFolderButton = null!;
    private SimpleButton _removeSelectedButton = null!;
    private SimpleButton _clearAllButton = null!;
    private SimpleButton _startImportButton = null!;
    private SimpleButton _cancelButton = null!;
    private CheckEdit _checkDuplicatesCheckBox = null!;
    private CheckEdit _requireManualReviewCheckBox = null!;
    private SpinEdit _confidenceThresholdSpinEdit = null!;
    private CheckEdit _stopOnErrorCheckBox = null!;

    // Data
    private BindingList<ImportFileInfo> _importFiles = null!;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isImporting;

    public BatchImportDialog(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<BatchImportDialog>>();
        _importInvoiceUseCase = serviceProvider.GetRequiredService<IImportInvoiceUseCase>();

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Text = "Batch Import Rechnungen";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // File grid
        _fileGridControl = new GridControl();
        _fileGridView = new GridView(_fileGridControl);
        
        _fileGridControl.MainView = _fileGridView;
        _fileGridView.GridControl = _fileGridControl;
        _fileGridView.OptionsBehavior.Editable = false;
        _fileGridView.OptionsSelection.MultiSelect = true;
        _fileGridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
        _fileGridView.OptionsView.ShowGroupPanel = false;
        _fileGridView.OptionsView.ShowAutoFilterRow = true;

        // File operations buttons
        _addFilesButton = new SimpleButton
        {
            Text = "Dateien hinzufügen",
            Width = 150
        };
        _addFilesButton.Click += OnAddFiles;

        _addFolderButton = new SimpleButton
        {
            Text = "Ordner hinzufügen",
            Width = 150
        };
        _addFolderButton.Click += OnAddFolder;

        _removeSelectedButton = new SimpleButton
        {
            Text = "Ausgewählte entfernen",
            Width = 150
        };
        _removeSelectedButton.Click += OnRemoveSelected;

        _clearAllButton = new SimpleButton
        {
            Text = "Alle löschen",
            Width = 150
        };
        _clearAllButton.Click += OnClearAll;

        // Options
        _checkDuplicatesCheckBox = new CheckEdit
        {
            Text = "Duplikate prüfen",
            Checked = true
        };

        _requireManualReviewCheckBox = new CheckEdit
        {
            Text = "Manuelle Prüfung bei niedriger Konfidenz",
            Checked = false
        };

        _confidenceThresholdSpinEdit = new SpinEdit
        {
            Properties =
            {
                MinValue = 0,
                MaxValue = 1,
                Increment = 0.1m,
                DisplayFormat = {FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "0.0"}
            },
            EditValue = 0.7m,
            Width = 100
        };

        _stopOnErrorCheckBox = new CheckEdit
        {
            Text = "Bei erstem Fehler stoppen",
            Checked = false
        };

        // Progress
        _overallProgressBar = new ProgressBarControl
        {
            Properties = { ShowTitle = true },
            Visible = false,
            Height = 25
        };

        _overallStatusLabel = new LabelControl
        {
            Text = "Bereit",
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 300
        };

        // Action buttons
        _startImportButton = new SimpleButton
        {
            Text = "Import starten",
            Width = 120,
            Enabled = false
        };
        _startImportButton.Click += OnStartImport;

        _cancelButton = new SimpleButton
        {
            Text = "Abbrechen",
            DialogResult = DialogResult.Cancel,
            Width = 120
        };
    }

    private void LayoutControls()
    {
        var mainLayout = new LayoutControl
        {
            Dock = DockStyle.Fill
        };

        var layoutControlGroup = new LayoutControlGroup
        {
            EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True,
            GroupBordersVisible = false
        };
        mainLayout.Root = layoutControlGroup;

        // File list section
        var fileListItem = layoutControlGroup.AddItem();
        fileListItem.Control = _fileGridControl;
        fileListItem.Text = "Zu importierende Dateien:";
        fileListItem.TextVisible = true;
        fileListItem.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
        fileListItem.MaxSize = new Size(0, 400);
        fileListItem.MinSize = new Size(0, 300);
        fileListItem.Size = new Size(0, 350);
        fileListItem.TextLocation = DevExpress.Utils.Locations.Top;

        // File operations panel
        var fileOpsPanel = new PanelControl { Dock = DockStyle.Right, Width = 180 };
        var fileOpsLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };
        fileOpsLayout.Controls.Add(_addFilesButton);
        fileOpsLayout.Controls.Add(_addFolderButton);
        fileOpsLayout.Controls.Add(_removeSelectedButton);
        fileOpsLayout.Controls.Add(_clearAllButton);
        fileOpsPanel.Controls.Add(fileOpsLayout);

        // Options section
        var optionsPanel = new PanelControl { Height = 150 };
        var optionsLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        
        var confidencePanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        confidencePanel.Controls.Add(new LabelControl { Text = "Konfidenz-Schwelle:", Padding = new Padding(0, 5, 10, 0) });
        confidencePanel.Controls.Add(_confidenceThresholdSpinEdit);

        optionsLayout.Controls.Add(_checkDuplicatesCheckBox);
        optionsLayout.Controls.Add(_requireManualReviewCheckBox);
        optionsLayout.Controls.Add(_stopOnErrorCheckBox);
        optionsLayout.Controls.Add(confidencePanel);
        optionsPanel.Controls.Add(optionsLayout);

        var optionsItem = layoutControlGroup.AddItem();
        optionsItem.Control = optionsPanel;
        optionsItem.Text = "Import-Optionen:";
        optionsItem.TextVisible = true;
        optionsItem.TextLocation = DevExpress.Utils.Locations.Top;

        // Progress section
        var progressPanel = new PanelControl { Height = 60 };
        var progressLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        progressLayout.Controls.Add(_overallStatusLabel);
        progressLayout.Controls.Add(_overallProgressBar);
        progressPanel.Controls.Add(progressLayout);

        var progressItem = layoutControlGroup.AddItem();
        progressItem.Control = progressPanel;
        progressItem.TextVisible = false;

        // Buttons
        var buttonPanel = new PanelControl { Dock = DockStyle.Bottom, Height = 50 };
        var buttonLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        buttonLayout.Controls.Add(_cancelButton);
        buttonLayout.Controls.Add(_startImportButton);
        buttonPanel.Controls.Add(buttonLayout);

        // Add file ops panel to main layout
        var containerPanel = new PanelControl { Dock = DockStyle.Fill };
        containerPanel.Controls.Add(mainLayout);
        containerPanel.Controls.Add(fileOpsPanel);

        this.Controls.Add(containerPanel);
        this.Controls.Add(buttonPanel);
    }

    private void InitializeData()
    {
        _importFiles = new BindingList<ImportFileInfo>();
        _fileGridControl.DataSource = _importFiles;
        
        _cancellationTokenSource = new CancellationTokenSource();
        _isImporting = false;

        // Configure grid columns
        _fileGridView.Columns["FilePath"].Visible = false;
        _fileGridView.Columns["FileName"].Caption = "Dateiname";
        _fileGridView.Columns["FileName"].Width = 350;
        _fileGridView.Columns["Status"].Caption = "Status";
        _fileGridView.Columns["Status"].Width = 100;
        _fileGridView.Columns["Progress"].Caption = "Fortschritt (%)";
        _fileGridView.Columns["Progress"].Width = 120;
        _fileGridView.Columns["Message"].Caption = "Nachricht";
        _fileGridView.Columns["Message"].Width = 250;
    }

    private void OnAddFiles(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "PDF-Dateien (*.pdf)|*.pdf|Alle Dateien (*.*)|*.*",
            Title = "Rechnungs-PDFs auswählen",
            Multiselect = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            foreach (var file in dialog.FileNames)
            {
                AddFile(file);
            }
        }
    }

    private void OnAddFolder(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Ordner mit PDF-Dateien auswählen"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var pdfFiles = Directory.GetFiles(dialog.SelectedPath, "*.pdf", SearchOption.AllDirectories);
            foreach (var file in pdfFiles)
            {
                AddFile(file);
            }
        }
    }

    private void AddFile(string filePath)
    {
        if (!_importFiles.Any(f => f.FilePath == filePath))
        {
            var fileInfo = new ImportFileInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = ImportStatus.Pending,
                Progress = 0,
                Message = "Bereit"
            };

            _importFiles.Add(fileInfo);
            UpdateStartButtonState();
        }
    }

    private void OnRemoveSelected(object? sender, EventArgs e)
    {
        var selectedRows = _fileGridView.GetSelectedRows();
        for (int i = selectedRows.Length - 1; i >= 0; i--)
        {
            var rowHandle = selectedRows[i];
            if (rowHandle >= 0)
            {
                _importFiles.RemoveAt(rowHandle);
            }
        }
        UpdateStartButtonState();
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        _importFiles.Clear();
        UpdateStartButtonState();
    }

    private void UpdateStartButtonState()
    {
        _startImportButton.Enabled = _importFiles.Count > 0 && !_isImporting;
    }

    private async void OnStartImport(object? sender, EventArgs e)
    {
        try
        {
            _isImporting = true;
            _startImportButton.Enabled = false;
            _overallProgressBar.Visible = true;
            _overallProgressBar.Properties.Maximum = _importFiles.Count;
            _overallProgressBar.Position = 0;

            var successCount = 0;
            var errorCount = 0;

            foreach (var fileInfo in _importFiles.ToList())
            {
                try
                {
                    SetOverallStatus($"Importiere {fileInfo.FileName}...");
                    fileInfo.Status = ImportStatus.Importing;
                    _fileGridView.RefreshData();

                    var result = await _importInvoiceUseCase.ExecuteAsync(fileInfo.FilePath);

                    if (result.Success)
                    {
                        fileInfo.Status = ImportStatus.Success;
                        fileInfo.Progress = 100;
                        fileInfo.Message = "Import erfolgreich";
                        successCount++;
                    }
                    else
                    {
                        fileInfo.Status = ImportStatus.Error;
                        fileInfo.Progress = 100;
                        fileInfo.Message = result.Message;
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Importieren der Datei: {File}", fileInfo.FilePath);
                    fileInfo.Status = ImportStatus.Error;
                    fileInfo.Progress = 100;
                    fileInfo.Message = ex.Message;
                    errorCount++;
                }

                _overallProgressBar.Position++;
                _fileGridView.RefreshData();
                System.Windows.Forms.Application.DoEvents();

                if (_stopOnErrorCheckBox.Checked && errorCount > 0)
                {
                    break;
                }
            }

            SetOverallStatus($"Import abgeschlossen: {successCount} erfolgreich, {errorCount} fehlgeschlagen");

            if (successCount > 0)
            {
                XtraMessageBox.Show(
                    $"Batch-Import abgeschlossen:\n\n{successCount} erfolgreich\n{errorCount} fehlgeschlagen",
                    "Import abgeschlossen",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            else
            {
                XtraMessageBox.Show(
                    "Alle Imports sind fehlgeschlagen. Bitte prüfen Sie die Fehlermeldungen.",
                    "Import fehlgeschlagen",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Starten des Batch-Imports");
            XtraMessageBox.Show(
                $"Fehler beim Starten des Batch-Imports: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _isImporting = false;
            _startImportButton.Enabled = true;
            _overallProgressBar.Visible = false;
        }
    }

    private void SetOverallStatus(string message)
    {
        _overallStatusLabel.Text = message;
        System.Windows.Forms.Application.DoEvents();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_isImporting)
        {
            var result = XtraMessageBox.Show(
                "Ein Import läuft gerade. Wirklich abbrechen?",
                "Import läuft",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        _cancellationTokenSource?.Cancel();
        base.OnFormClosing(e);
    }
}

public class ImportFileInfo : INotifyPropertyChanged
{
    private string _filePath = string.Empty;
    private string _fileName = string.Empty;
    private ImportStatus _status;
    private int _progress;
    private string _message = string.Empty;

    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged(nameof(FilePath));
        }
    }

    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            OnPropertyChanged(nameof(FileName));
        }
    }

    public ImportStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public int Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            OnPropertyChanged(nameof(Progress));
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged(nameof(Message));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum ImportStatus
{
    Pending,
    Importing,
    Success,
    Error,
    Cancelled
}

