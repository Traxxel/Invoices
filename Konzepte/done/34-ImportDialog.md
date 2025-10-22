# Aufgabe 34: ImportDialog (File Picker, Progress)

## ⚠️ WICHTIG: DevExpress-Komponenten verwenden!
**Alle Standard-WinForms-Komponenten müssen durch DevExpress-Äquivalente ersetzt werden!**
Siehe: `Konzepte/DEVEXPRESS_MAPPING.md` für Details.

## Ziel

ImportDialog für die Auswahl von PDF-Dateien und Anzeige des Import-Fortschritts mit DevExpress-Komponenten.

## 1. ImportDialog Interface

**Datei:** `src/Invoice.WinForms/Forms/ImportDialog.cs`

```csharp
using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;

namespace Invoice.WinForms.Forms;

public partial class ImportDialog : XtraForm
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImportDialog> _logger;
    private readonly IImportInvoiceUseCase _importInvoiceUseCase;

    // DevExpress UI Controls
    private OpenFileDialog _fileDialog;
    private DevExpress.XtraEditors.ProgressBarControl _progressBar;
    private LabelControl _statusLabel;
    private ListBoxControl _fileListBox;
    private SimpleButton _addFilesButton;
    private SimpleButton _removeFileButton;
    private SimpleButton _importButton;
    private SimpleButton _cancelButton;
    private CheckEdit _checkDuplicatesCheckBox;
    private CheckEdit _requireManualReviewCheckBox;
    private SpinEdit _confidenceThresholdNumericUpDown;

    // Data
    private List<string> _selectedFiles;
    private CancellationTokenSource _cancellationTokenSource;

    public ImportDialog(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<ImportDialog>>();
        _importInvoiceUseCase = serviceProvider.GetRequiredService<IImportInvoiceUseCase>();

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Text = "Import Invoices";
        this.Size = new Size(600, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // File selection
        _fileListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            SelectionMode = SelectionMode.MultiExtended
        };

        _addFilesButton = new Button
        {
            Text = "Add Files",
            Width = 100
        };
        _addFilesButton.Click += OnAddFiles;

        _removeFileButton = new Button
        {
            Text = "Remove Selected",
            Width = 100
        };
        _removeFileButton.Click += OnRemoveFile;

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

        // Progress
        _progressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Visible = false
        };

        _statusLabel = new Label
        {
            Text = "Ready",
            AutoSize = true
        };

        // Buttons
        _importButton = new Button
        {
            Text = "Import",
            DialogResult = DialogResult.OK,
            Enabled = false
        };
        _importButton.Click += OnImport;

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
            RowCount = 6
        };

        // File selection
        mainPanel.Controls.Add(new Label { Text = "Files to import:" }, 0, 0);
        mainPanel.Controls.Add(_fileListBox, 0, 1);
        mainPanel.SetRowSpan(_fileListBox, 2);

        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };
        buttonPanel.Controls.Add(_addFilesButton);
        buttonPanel.Controls.Add(_removeFileButton);
        mainPanel.Controls.Add(buttonPanel, 1, 1);

        // Options
        mainPanel.Controls.Add(new Label { Text = "Options:" }, 0, 3);
        mainPanel.Controls.Add(_checkDuplicatesCheckBox, 0, 4);
        mainPanel.Controls.Add(_requireManualReviewCheckBox, 0, 5);
        mainPanel.Controls.Add(new Label { Text = "Confidence threshold:" }, 1, 4);
        mainPanel.Controls.Add(_confidenceThresholdNumericUpDown, 1, 5);

        // Progress
        mainPanel.Controls.Add(_progressBar, 0, 6);
        mainPanel.Controls.Add(_statusLabel, 1, 6);

        // Buttons
        var buttonPanel2 = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            AutoSize = true
        };
        buttonPanel2.Controls.Add(_cancelButton);
        buttonPanel2.Controls.Add(_importButton);

        this.Controls.Add(mainPanel);
        this.Controls.Add(buttonPanel2);
    }

    private void InitializeData()
    {
        _selectedFiles = new List<string>();
        _cancellationTokenSource = new CancellationTokenSource();
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
                if (!_selectedFiles.Contains(file))
                {
                    _selectedFiles.Add(file);
                }
            }
            RefreshFileList();
        }
    }

    private void OnRemoveFile(object? sender, EventArgs e)
    {
        if (_fileListBox.SelectedItems.Count > 0)
        {
            var selectedFiles = _fileListBox.SelectedItems.Cast<string>().ToList();
            foreach (var file in selectedFiles)
            {
                _selectedFiles.Remove(file);
            }
            RefreshFileList();
        }
    }

    private void RefreshFileList()
    {
        _fileListBox.DataSource = null;
        _fileListBox.DataSource = _selectedFiles;
        _importButton.Enabled = _selectedFiles.Count > 0;
    }

    private async void OnImport(object? sender, EventArgs e)
    {
        try
        {
            _importButton.Enabled = false;
            _progressBar.Visible = true;
            _progressBar.Maximum = _selectedFiles.Count;
            _progressBar.Value = 0;

            var successCount = 0;
            var errorCount = 0;

            foreach (var file in _selectedFiles)
            {
                try
                {
                    SetStatus($"Importing {Path.GetFileName(file)}...");

                    var result = await _importInvoiceUseCase.ExecuteAsync(file);

                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import file: {File}", file);
                    errorCount++;
                }

                _progressBar.Value++;
                Application.DoEvents();
            }

            SetStatus($"Import completed: {successCount} successful, {errorCount} failed");

            if (successCount > 0)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import files");
            MessageBox.Show($"Failed to import files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _importButton.Enabled = true;
            _progressBar.Visible = false;
        }
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
        Application.DoEvents();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        base.OnFormClosing(e);
    }
}
```

## Wichtige Hinweise

- ImportDialog für PDF-Datei-Auswahl
- Progress Bar für Import-Fortschritt
- Optionen für Duplikatprüfung und Confidence Threshold
- Multi-File Selection
- Error Handling für Import-Operationen
- Async Import mit Progress Updates
