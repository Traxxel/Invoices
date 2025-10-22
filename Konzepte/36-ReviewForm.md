# Aufgabe 36: ReviewForm (Layout: PDF links, Felder rechts)

## Ziel

ReviewForm für die Überprüfung und Bearbeitung von extrahierten Rechnungsfeldern mit PDF-Viewer links und Feldeditor rechts.

## 1. ReviewForm Interface

**Datei:** `src/Invoice.WinForms/Forms/ReviewForm.cs`

```csharp
using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Invoice.WinForms.Forms;

public partial class ReviewForm : XtraForm
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReviewForm> _logger;
    private readonly ISaveInvoiceUseCase _saveInvoiceUseCase;
    private readonly IExtractFieldsUseCase _extractFieldsUseCase;

    // UI Controls
    private SplitContainer _mainSplitContainer;
    private Panel _pdfViewerPanel;
    private Panel _fieldsPanel;
    private DataGridView _fieldsGrid;
    private Button _saveButton;
    private Button _cancelButton;
    private Button _reExtractButton;
    private Button _validateButton;
    private Label _confidenceLabel;
    private ProgressBar _confidenceProgressBar;

    // Data
    private InvoiceDto? _invoice;
    private ExtractionResult? _extraction;
    private bool _isEditMode;
    private bool _hasChanges;

    public ReviewForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<ReviewForm>>();
        _saveInvoiceUseCase = serviceProvider.GetRequiredService<ISaveInvoiceUseCase>();
        _extractFieldsUseCase = serviceProvider.GetRequiredService<IExtractFieldsUseCase>();

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Text = "Review Invoice";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // Main split container
        _mainSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 600,
            SplitterWidth = 5
        };

        // PDF viewer panel
        _pdfViewerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Fields panel
        _fieldsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Fields grid
        _fieldsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Confidence display
        _confidenceLabel = new Label
        {
            Text = "Confidence: 0%",
            AutoSize = true
        };

        _confidenceProgressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Width = 200
        };

        // Buttons
        _saveButton = new Button
        {
            Text = "Save",
            Width = 100
        };
        _saveButton.Click += OnSave;

        _cancelButton = new Button
        {
            Text = "Cancel",
            Width = 100
        };
        _cancelButton.Click += OnCancel;

        _reExtractButton = new Button
        {
            Text = "Re-extract",
            Width = 100
        };
        _reExtractButton.Click += OnReExtract;

        _validateButton = new Button
        {
            Text = "Validate",
            Width = 100
        };
        _validateButton.Click += OnValidate;
    }

    private void LayoutControls()
    {
        // Main split container
        _mainSplitContainer.Panel1.Controls.Add(_pdfViewerPanel);
        _mainSplitContainer.Panel2.Controls.Add(_fieldsPanel);

        // Fields panel layout
        var fieldsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };

        // Confidence panel
        var confidencePanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        confidencePanel.Controls.Add(_confidenceLabel);
        confidencePanel.Controls.Add(_confidenceProgressBar);

        // Buttons panel
        var buttonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        buttonsPanel.Controls.Add(_cancelButton);
        buttonsPanel.Controls.Add(_saveButton);
        buttonsPanel.Controls.Add(_validateButton);
        buttonsPanel.Controls.Add(_reExtractButton);

        fieldsLayout.Controls.Add(new Label { Text = "Extracted Fields:" }, 0, 0);
        fieldsLayout.Controls.Add(_fieldsGrid, 0, 1);
        fieldsLayout.Controls.Add(confidencePanel, 0, 2);
        fieldsLayout.Controls.Add(buttonsPanel, 0, 3);

        _fieldsPanel.Controls.Add(fieldsLayout);

        this.Controls.Add(_mainSplitContainer);
    }

    private void InitializeData()
    {
        _invoice = null;
        _extraction = null;
        _isEditMode = false;
        _hasChanges = false;
    }

    public void SetInvoice(InvoiceDto invoice)
    {
        _invoice = invoice;
        LoadInvoiceData();
    }

    public void SetEditMode(bool editMode)
    {
        _isEditMode = editMode;
        _fieldsGrid.ReadOnly = !editMode;
        _saveButton.Enabled = editMode;
    }

    private void LoadInvoiceData()
    {
        if (_invoice == null) return;

        try
        {
            // Load PDF viewer
            LoadPdfViewer();

            // Load extraction data
            LoadExtractionData();

            // Update confidence display
            UpdateConfidenceDisplay();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load invoice data");
            MessageBox.Show($"Failed to load invoice data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadPdfViewer()
    {
        if (_invoice?.SourceFilePath == null) return;

        try
        {
            // Create PDF viewer control
            var pdfViewer = new PdfViewerControl(_serviceProvider);
            pdfViewer.LoadPdf(_invoice.SourceFilePath);
            pdfViewer.Dock = DockStyle.Fill;

            _pdfViewerPanel.Controls.Clear();
            _pdfViewerPanel.Controls.Add(pdfViewer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load PDF viewer");
            _pdfViewerPanel.Controls.Clear();
            _pdfViewerPanel.Controls.Add(new Label { Text = "PDF viewer not available", Dock = DockStyle.Fill });
        }
    }

    private void LoadExtractionData()
    {
        if (_invoice == null) return;

        try
        {
            // Create fields grid
            _fieldsGrid.Columns.Clear();

            // Add columns
            var fieldTypeColumn = new DataGridViewTextBoxColumn
            {
                Name = "FieldType",
                HeaderText = "Field Type",
                DataPropertyName = "FieldType",
                ReadOnly = true,
                Width = 150
            };
            _fieldsGrid.Columns.Add(fieldTypeColumn);

            var valueColumn = new DataGridViewTextBoxColumn
            {
                Name = "Value",
                HeaderText = "Value",
                DataPropertyName = "Value",
                ReadOnly = false,
                Width = 200
            };
            _fieldsGrid.Columns.Add(valueColumn);

            var confidenceColumn = new DataGridViewTextBoxColumn
            {
                Name = "Confidence",
                HeaderText = "Confidence",
                DataPropertyName = "Confidence",
                ReadOnly = true,
                Width = 100
            };
            _fieldsGrid.Columns.Add(confidenceColumn);

            var alternativesColumn = new DataGridViewComboBoxColumn
            {
                Name = "Alternatives",
                HeaderText = "Alternatives",
                DataPropertyName = "Alternatives",
                ReadOnly = false,
                Width = 150
            };
            _fieldsGrid.Columns.Add(alternativesColumn);

            // Load data
            var fields = new List<FieldDisplayItem>();

            // Add invoice fields
            fields.Add(new FieldDisplayItem("InvoiceNumber", _invoice.InvoiceNumber, 1.0f, new List<string>()));
            fields.Add(new FieldDisplayItem("InvoiceDate", _invoice.InvoiceDate.ToString(), 1.0f, new List<string>()));
            fields.Add(new FieldDisplayItem("IssuerName", _invoice.IssuerName, 1.0f, new List<string>()));
            fields.Add(new FieldDisplayItem("GrossTotal", _invoice.GrossTotal.ToString(), 1.0f, new List<string>()));

            _fieldsGrid.DataSource = fields;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load extraction data");
            MessageBox.Show($"Failed to load extraction data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateConfidenceDisplay()
    {
        if (_invoice == null) return;

        var confidence = _invoice.ExtractionConfidence;
        _confidenceLabel.Text = $"Confidence: {confidence:P0}";
        _confidenceProgressBar.Value = (int)(confidence * 100);

        // Color code confidence
        if (confidence < 0.5f)
        {
            _confidenceProgressBar.ForeColor = Color.Red;
        }
        else if (confidence < 0.8f)
        {
            _confidenceProgressBar.ForeColor = Color.Orange;
        }
        else
        {
            _confidenceProgressBar.ForeColor = Color.Green;
        }
    }

    private async void OnSave(object? sender, EventArgs e)
    {
        try
        {
            if (_invoice == null) return;

            // Update invoice with changes
            UpdateInvoiceFromGrid();

            // Save invoice
            var result = await _saveInvoiceUseCase.ExecuteAsync(_invoice);

            if (result.Success)
            {
                MessageBox.Show("Invoice saved successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _hasChanges = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show($"Failed to save invoice: {result.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save invoice");
            MessageBox.Show($"Failed to save invoice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        if (_hasChanges)
        {
            var result = MessageBox.Show("You have unsaved changes. Are you sure you want to cancel?", "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }
        }

        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    private async void OnReExtract(object? sender, EventArgs e)
    {
        try
        {
            if (_invoice?.SourceFilePath == null) return;

            // Re-extract fields
            var result = await _extractFieldsUseCase.ExecuteAsync(_invoice.SourceFilePath);

            if (result.Success)
            {
                _extraction = result;
                LoadExtractionData();
                UpdateConfidenceDisplay();
                _hasChanges = true;
            }
            else
            {
                MessageBox.Show($"Failed to re-extract fields: {result.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to re-extract fields");
            MessageBox.Show($"Failed to re-extract fields: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnValidate(object? sender, EventArgs e)
    {
        try
        {
            if (_invoice == null) return;

            // Validate invoice
            var result = await _saveInvoiceUseCase.ValidateInvoiceAsync(_invoice);

            if (result.IsValid)
            {
                MessageBox.Show("Invoice validation successful", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var errors = string.Join("\n", result.Errors.Select(e => e.Message));
                MessageBox.Show($"Invoice validation failed:\n{errors}", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate invoice");
            MessageBox.Show($"Failed to validate invoice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateInvoiceFromGrid()
    {
        if (_invoice == null) return;

        foreach (DataGridViewRow row in _fieldsGrid.Rows)
        {
            var fieldType = row.Cells["FieldType"].Value?.ToString();
            var value = row.Cells["Value"].Value?.ToString();

            if (string.IsNullOrWhiteSpace(fieldType) || string.IsNullOrWhiteSpace(value)) continue;

            switch (fieldType)
            {
                case "InvoiceNumber":
                    _invoice = _invoice with { InvoiceNumber = value };
                    break;
                case "InvoiceDate":
                    if (DateOnly.TryParse(value, out var date))
                    {
                        _invoice = _invoice with { InvoiceDate = date };
                    }
                    break;
                case "IssuerName":
                    _invoice = _invoice with { IssuerName = value };
                    break;
                case "GrossTotal":
                    if (decimal.TryParse(value, out var total))
                    {
                        _invoice = _invoice with { GrossTotal = total };
                    }
                    break;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_hasChanges)
        {
            var result = MessageBox.Show("You have unsaved changes. Are you sure you want to close?", "Confirm Close", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        base.OnFormClosing(e);
    }
}

public class FieldDisplayItem
{
    public string FieldType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public List<string> Alternatives { get; set; } = new List<string>();
}
```

## Wichtige Hinweise

- ReviewForm mit Split-Layout (PDF links, Felder rechts)
- PDF-Viewer für Rechnungsanzeige
- DataGridView für Feldbearbeitung
- Confidence-Anzeige mit Progress Bar
- Save/Cancel/Re-extract/Validate Buttons
- Change Tracking für unsaved changes
- Field Validation und Alternatives
- Error Handling für alle Operationen
