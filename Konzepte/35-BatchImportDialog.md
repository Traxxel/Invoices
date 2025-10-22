# Aufgabe 35: BatchImportDialog (Multi-File, Status List)

## Ziel

BatchImportDialog für den Import mehrerer PDF-Dateien mit detaillierter Status-Liste.

## 1. BatchImportDialog Interface

**Datei:** `src/Invoice.WinForms/Forms/BatchImportDialog.cs`

```csharp
using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Invoice.WinForms.Forms;

public partial class BatchImportDialog : XtraForm
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BatchImportDialog> _logger;
    private readonly IImportInvoiceUseCase _importInvoiceUseCase;

    // UI Controls
    private ListView _fileListView;
    private ProgressBar _overallProgressBar;
    private Label _overallStatusLabel;
    private Button _addFilesButton;
    private Button _addFolderButton;
    private Button _removeSelectedButton;
    private Button _clearAllButton;
    private Button _startImportButton;
    private Button _cancelButton;
    private CheckBox _checkDuplicatesCheckBox;
    private CheckBox _requireManualReviewCheckBox;
    private NumericUpDown _confidenceThresholdNumericUpDown;
    private CheckBox _stopOnErrorCheckBox;

    // Data
    private List<ImportFileInfo> _importFiles;
    private CancellationTokenSource _cancellationTokenSource;
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
        this.Text = "Batch Import Invoices";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // File list view
        _fileListView = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            CheckBoxes = true,
            Dock = DockStyle.Fill
        };

        _fileListView.Columns.Add("File", 300);
        _fileListView.Columns.Add("Status", 100);
        _fileListView.Columns.Add("Progress", 80);
        _fileListView.Columns.Add("Message", 200);

        // File operations
        _addFilesButton = new Button
        {
            Text = "Add Files",
            Width = 100
        };
        _addFilesButton.Click += OnAddFiles;

        _addFolderButton = new Button
        {
            Text = "Add Folder",
            Width = 100
        };
        _addFolderButton.Click += OnAddFolder;

        _removeSelectedButton = new Button
        {
            Text = "Remove Selected",
            Width = 100
        };
        _removeSelectedButton.Click += OnRemoveSelected;

        _clearAllButton = new Button
        {
            Text = "Clear All",
            Width = 100
        };
        _clearAllButton.Click += OnClearAll;

        // Options
        _checkDuplicatesCheckBox = new CheckBox
        {
            Text = "Check for duplicates",
            Checked = true
        };

        _requireManualReviewCheckBox = new CheckBox
        {
            Text = "Require manual review for low confidence",
            Checked = false
        };

        _confidenceThresholdNumericUpDown = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1,
            Increment = 0.1m,
            DecimalPlaces = 1,
            Value = 0.7m
        };

        _stopOnErrorCheckBox = new CheckBox
        {
            Text = "Stop on first error",
            Checked = false
        };

        // Progress
        _overallProgressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Visible = false
        };

        _overallStatusLabel = new Label
        {
            Text = "Ready",
            AutoSize = true
        };

        // Buttons
        _startImportButton = new Button
        {
            Text = "Start Import",
            DialogResult = DialogResult.OK,
            Enabled = false
        };
        _startImportButton.Click += OnStartImport;

        _cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel
        };
    }

    private void LayoutControls()
    {
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5
        };

        // File list
        mainPanel.Controls.Add(new Label { Text = "Files to import:" }, 0, 0);
        mainPanel.Controls.Add(_fileListView, 0, 1);
        mainPanel.SetRowSpan(_fileListView, 2);

        // File operations
        var fileOpsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };
        fileOpsPanel.Controls.Add(_addFilesButton);
        fileOpsPanel.Controls.Add(_addFolderButton);
        fileOpsPanel.Controls.Add(_removeSelectedButton);
        fileOpsPanel.Controls.Add(_clearAllButton);
        mainPanel.Controls.Add(fileOpsPanel, 1, 1);

        // Options
        mainPanel.Controls.Add(new Label { Text = "Import Options:" }, 0, 3);
        var optionsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };
        optionsPanel.Controls.Add(_checkDuplicatesCheckBox);
        optionsPanel.Controls.Add(_requireManualReviewCheckBox);
        optionsPanel.Controls.Add(_stopOnErrorCheckBox);
        optionsPanel.Controls.Add(new Label { Text = "Confidence threshold:" });
        optionsPanel.Controls.Add(_confidenceThresholdNumericUpDown);
        mainPanel.Controls.Add(optionsPanel, 1, 3);

        // Progress
        mainPanel.Controls.Add(_overallProgressBar, 0, 4);
        mainPanel.Controls.Add(_overallStatusLabel, 1, 4);

        // Buttons
        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            AutoSize = true
        };
        buttonPanel.Controls.Add(_cancelButton);
        buttonPanel.Controls.Add(_startImportButton);

        this.Controls.Add(mainPanel);
        this.Controls.Add(buttonPanel);
    }

    private void InitializeData()
    {
        _importFiles = new List<ImportFileInfo>();
        _cancellationTokenSource = new CancellationTokenSource();
        _isImporting = false;
    }

    private void OnAddFiles(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
            Title = "Select Invoice PDFs",
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
            Description = "Select folder containing PDF files"
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
                Message = "Ready"
            };

            _importFiles.Add(fileInfo);
            RefreshFileList();
        }
    }

    private void OnRemoveSelected(object? sender, EventArgs e)
    {
        var selectedItems = _fileListView.CheckedItems.Cast<ListViewItem>().ToList();
        foreach (var item in selectedItems)
        {
            var fileInfo = item.Tag as ImportFileInfo;
            if (fileInfo != null)
            {
                _importFiles.Remove(fileInfo);
            }
        }
        RefreshFileList();
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        _importFiles.Clear();
        RefreshFileList();
    }

    private void RefreshFileList()
    {
        _fileListView.Items.Clear();

        foreach (var fileInfo in _importFiles)
        {
            var item = new ListViewItem(fileInfo.FileName);
            item.SubItems.Add(fileInfo.Status.ToString());
            item.SubItems.Add($"{fileInfo.Progress}%");
            item.SubItems.Add(fileInfo.Message);
            item.Tag = fileInfo;
            _fileListView.Items.Add(item);
        }

        _startImportButton.Enabled = _importFiles.Count > 0 && !_isImporting;
    }

    private async void OnStartImport(object? sender, EventArgs e)
    {
        try
        {
            _isImporting = true;
            _startImportButton.Enabled = false;
            _overallProgressBar.Visible = true;
            _overallProgressBar.Maximum = _importFiles.Count;
            _overallProgressBar.Value = 0;

            var successCount = 0;
            var errorCount = 0;

            foreach (var fileInfo in _importFiles)
            {
                try
                {
                    SetOverallStatus($"Importing {fileInfo.FileName}...");

                    var result = await _importInvoiceUseCase.ExecuteAsync(fileInfo.FilePath);

                    if (result.Success)
                    {
                        fileInfo.Status = ImportStatus.Success;
                        fileInfo.Progress = 100;
                        fileInfo.Message = "Import successful";
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
                    _logger.LogError(ex, "Failed to import file: {File}", fileInfo.FilePath);
                    fileInfo.Status = ImportStatus.Error;
                    fileInfo.Progress = 100;
                    fileInfo.Message = ex.Message;
                    errorCount++;
                }

                _overallProgressBar.Value++;
                RefreshFileList();
                Application.DoEvents();

                if (_stopOnErrorCheckBox.Checked && errorCount > 0)
                {
                    break;
                }
            }

            SetOverallStatus($"Import completed: {successCount} successful, {errorCount} failed");

            if (successCount > 0)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start batch import");
            MessageBox.Show($"Failed to start batch import: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        Application.DoEvents();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        base.OnFormClosing(e);
    }
}

public class ImportFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public ImportStatus Status { get; set; }
    public int Progress { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum ImportStatus
{
    Pending,
    Importing,
    Success,
    Error,
    Cancelled
}
```

## Wichtige Hinweise

- BatchImportDialog für Multi-File Import
- ListView mit detaillierter Status-Anzeige
- Folder Selection für Batch-Import
- Progress Tracking für jeden File
- Stop on Error Option
- Checkbox Selection für Files
- Overall Progress Bar
- Error Handling für jeden Import
