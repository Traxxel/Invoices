using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using System.ComponentModel;
using System.IO;

namespace Invoice.WinForms.Forms;

public partial class ReviewForm : XtraForm
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReviewForm> _logger;
    private readonly ISaveInvoiceUseCase _saveInvoiceUseCase;
    private readonly IExtractFieldsUseCase _extractFieldsUseCase;

    // DevExpress UI Controls
    private SplitContainerControl _mainSplitContainer = null!;
    private PanelControl _pdfViewerPanel = null!;
    private PanelControl _fieldsPanel = null!;
    private GridControl _fieldsGrid = null!;
    private GridView _fieldsGridView = null!;
    private SimpleButton _saveButton = null!;
    private SimpleButton _cancelButton = null!;
    private SimpleButton _reExtractButton = null!;
    private SimpleButton _validateButton = null!;
    private LabelControl _confidenceLabel = null!;
    private ProgressBarControl _confidenceProgressBar = null!;

    // Data
    private InvoiceDto? _invoice;
    private ExtractionResult? _extraction;
    private bool _isEditMode;
    private bool _hasChanges;
    private BindingList<FieldDisplayItem> _fields = null!;

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
        this.Text = "Rechnung prüfen";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = true;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // Main split container
        _mainSplitContainer = new SplitContainerControl
        {
            Dock = DockStyle.Fill,
            SplitterPosition = 700
        };

        // PDF viewer panel
        _pdfViewerPanel = new PanelControl
        {
            Dock = DockStyle.Fill
        };
        _pdfViewerPanel.Text = "PDF-Ansicht";

        // Fields panel
        _fieldsPanel = new PanelControl
        {
            Dock = DockStyle.Fill
        };

        // Fields grid
        _fieldsGrid = new GridControl { Dock = DockStyle.Fill };
        _fieldsGridView = new GridView(_fieldsGrid);
        
        _fieldsGrid.MainView = _fieldsGridView;
        _fieldsGridView.GridControl = _fieldsGrid;
        _fieldsGridView.OptionsBehavior.Editable = true;
        _fieldsGridView.OptionsView.ShowGroupPanel = false;
        _fieldsGridView.OptionsView.ShowAutoFilterRow = false;
        _fieldsGridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _fieldsGridView.CellValueChanged += (s, e) => _hasChanges = true;

        // Confidence display
        _confidenceLabel = new LabelControl
        {
            Text = "Konfidenz: 0%",
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 150
        };

        _confidenceProgressBar = new ProgressBarControl
        {
            Properties = 
            { 
                ShowTitle = true,
                Minimum = 0,
                Maximum = 100
            },
            Width = 250,
            Height = 25
        };

        // Buttons
        _saveButton = new SimpleButton
        {
            Text = "Speichern",
            Width = 120
        };
        _saveButton.Click += OnSave;

        _cancelButton = new SimpleButton
        {
            Text = "Abbrechen",
            Width = 120
        };
        _cancelButton.Click += OnCancel;

        _reExtractButton = new SimpleButton
        {
            Text = "Neu extrahieren",
            Width = 120
        };
        _reExtractButton.Click += OnReExtract;

        _validateButton = new SimpleButton
        {
            Text = "Validieren",
            Width = 120
        };
        _validateButton.Click += OnValidate;
    }

    private void LayoutControls()
    {
        // PDF viewer in left panel
        _mainSplitContainer.Panel1.Controls.Add(_pdfViewerPanel);

        // Fields layout in right panel
        var fieldsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(10)
        };

        // Title
        var titleLabel = new LabelControl 
        { 
            Text = "Extrahierte Felder:",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) },
            AutoSizeMode = LabelAutoSizeMode.None,
            Height = 25
        };

        // Confidence panel
        var confidencePanel = new PanelControl 
        { 
            Height = 60,
            BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
        };
        var confidenceLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 10, 0, 0)
        };
        confidenceLayout.Controls.Add(_confidenceLabel);
        confidenceLayout.Controls.Add(_confidenceProgressBar);
        confidencePanel.Controls.Add(confidenceLayout);

        // Buttons panel
        var buttonsPanel = new PanelControl 
        { 
            Height = 50,
            Dock = DockStyle.Bottom,
            BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
        };
        var buttonsLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 10, 0, 0)
        };
        buttonsLayout.Controls.Add(_cancelButton);
        buttonsLayout.Controls.Add(_saveButton);
        buttonsLayout.Controls.Add(_validateButton);
        buttonsLayout.Controls.Add(_reExtractButton);
        buttonsPanel.Controls.Add(buttonsLayout);

        fieldsLayout.Controls.Add(titleLabel, 0, 0);
        fieldsLayout.SetRow(titleLabel, 0);
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        fieldsLayout.Controls.Add(_fieldsGrid, 0, 1);
        fieldsLayout.SetRow(_fieldsGrid, 1);
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        fieldsLayout.Controls.Add(confidencePanel, 0, 2);
        fieldsLayout.SetRow(confidencePanel, 2);
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

        fieldsLayout.Controls.Add(buttonsPanel, 0, 3);
        fieldsLayout.SetRow(buttonsPanel, 3);
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        _fieldsPanel.Controls.Add(fieldsLayout);
        _mainSplitContainer.Panel2.Controls.Add(_fieldsPanel);

        this.Controls.Add(_mainSplitContainer);
    }

    private void InitializeData()
    {
        _invoice = null;
        _extraction = null;
        _isEditMode = false;
        _hasChanges = false;
        _fields = new BindingList<FieldDisplayItem>();

        _fieldsGrid.DataSource = _fields;

        // Configure grid columns
        var fieldTypeColumn = _fieldsGridView.Columns["FieldType"];
        if (fieldTypeColumn != null)
        {
            fieldTypeColumn.Caption = "Feldtyp";
            fieldTypeColumn.Width = 150;
            fieldTypeColumn.OptionsColumn.ReadOnly = true;
        }

        var valueColumn = _fieldsGridView.Columns["Value"];
        if (valueColumn != null)
        {
            valueColumn.Caption = "Wert";
            valueColumn.Width = 250;
            valueColumn.OptionsColumn.AllowEdit = true;
        }

        var confidenceColumn = _fieldsGridView.Columns["Confidence"];
        if (confidenceColumn != null)
        {
            confidenceColumn.Caption = "Konfidenz";
            confidenceColumn.Width = 100;
            confidenceColumn.OptionsColumn.ReadOnly = true;
            confidenceColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            confidenceColumn.DisplayFormat.FormatString = "P0";
        }
    }

    public void SetInvoice(InvoiceDto invoice)
    {
        _invoice = invoice;
        LoadInvoiceData();
    }

    public void SetEditMode(bool editMode)
    {
        _isEditMode = editMode;
        _fieldsGridView.OptionsBehavior.Editable = editMode;
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
            _logger.LogError(ex, "Fehler beim Laden der Rechnungsdaten");
            XtraMessageBox.Show(
                $"Fehler beim Laden der Rechnungsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadPdfViewer()
    {
        if (_invoice?.SourceFilePath == null) return;

        try
        {
            // Try to create PDF viewer control
            // Note: PdfViewerControl needs to be implemented separately (Konzept 37)
            var pdfLabel = new LabelControl
            {
                Text = $"PDF-Vorschau:\n{Path.GetFileName(_invoice.SourceFilePath)}\n\n(PDF-Viewer wird in Konzept 37 implementiert)",
                Dock = DockStyle.Fill,
                Appearance = { TextOptions = { WordWrap = DevExpress.Utils.WordWrap.Wrap, VAlignment = DevExpress.Utils.VertAlignment.Center, HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            };

            _pdfViewerPanel.Controls.Clear();
            _pdfViewerPanel.Controls.Add(pdfLabel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des PDF-Viewers");
            _pdfViewerPanel.Controls.Clear();
            _pdfViewerPanel.Controls.Add(new LabelControl 
            { 
                Text = "PDF-Viewer nicht verfügbar", 
                Dock = DockStyle.Fill,
                Appearance = { TextOptions = { VAlignment = DevExpress.Utils.VertAlignment.Center, HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            });
        }
    }

    private void LoadExtractionData()
    {
        if (_invoice == null) return;

        try
        {
            _fields.Clear();

            // Add invoice fields
            _fields.Add(new FieldDisplayItem { FieldType = "Rechnungsnummer", Value = _invoice.InvoiceNumber, Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "Rechnungsdatum", Value = _invoice.InvoiceDate.ToString("dd.MM.yyyy"), Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "Aussteller", Value = _invoice.IssuerName, Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "Straße", Value = _invoice.IssuerStreet, Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "PLZ", Value = _invoice.IssuerPostalCode, Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "Stadt", Value = _invoice.IssuerCity, Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "Land", Value = _invoice.IssuerCountry ?? "", Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "Nettobetrag", Value = _invoice.NetTotal.ToString("C2"), Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "MwSt", Value = _invoice.VatTotal.ToString("C2"), Confidence = 1.0f });
            _fields.Add(new FieldDisplayItem { FieldType = "Bruttobetrag", Value = _invoice.GrossTotal.ToString("C2"), Confidence = 1.0f });

            _fieldsGridView.BestFitColumns();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Extraktionsdaten");
            XtraMessageBox.Show(
                $"Fehler beim Laden der Extraktionsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateConfidenceDisplay()
    {
        if (_invoice == null) return;

        var confidence = _invoice.ExtractionConfidence;
        _confidenceLabel.Text = $"Konfidenz: {confidence:P0}";
        _confidenceProgressBar.Position = (int)(confidence * 100);

        // Color code confidence
        if (confidence < 0.5f)
        {
            _confidenceProgressBar.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            _confidenceProgressBar.Properties.Appearance.ForeColor = Color.Red;
        }
        else if (confidence < 0.8f)
        {
            _confidenceProgressBar.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            _confidenceProgressBar.Properties.Appearance.ForeColor = Color.Orange;
        }
        else
        {
            _confidenceProgressBar.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            _confidenceProgressBar.Properties.Appearance.ForeColor = Color.Green;
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
                XtraMessageBox.Show(
                    "Rechnung erfolgreich gespeichert",
                    "Erfolg",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                
                _hasChanges = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                XtraMessageBox.Show(
                    $"Fehler beim Speichern der Rechnung: {result.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Speichern der Rechnung");
            XtraMessageBox.Show(
                $"Fehler beim Speichern der Rechnung: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        if (_hasChanges)
        {
            var result = XtraMessageBox.Show(
                "Sie haben ungespeicherte Änderungen. Wirklich abbrechen?",
                "Abbrechen bestätigen",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

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
                // TODO: Update invoice from extraction result
                LoadExtractionData();
                UpdateConfidenceDisplay();
                _hasChanges = true;
                
                XtraMessageBox.Show(
                    "Felder erfolgreich neu extrahiert",
                    "Erfolg",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                XtraMessageBox.Show(
                    $"Fehler beim Neu-Extrahieren der Felder: {result.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Neu-Extrahieren der Felder");
            XtraMessageBox.Show(
                $"Fehler beim Neu-Extrahieren der Felder: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
                XtraMessageBox.Show(
                    "Rechnungsvalidierung erfolgreich",
                    "Validierung",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                var errors = string.Join("\n", result.Errors.Select(e => $"• {e.Message}"));
                XtraMessageBox.Show(
                    $"Rechnungsvalidierung fehlgeschlagen:\n\n{errors}",
                    "Validierung",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Validieren der Rechnung");
            XtraMessageBox.Show(
                $"Fehler beim Validieren der Rechnung: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateInvoiceFromGrid()
    {
        if (_invoice == null) return;

        foreach (var field in _fields)
        {
            switch (field.FieldType)
            {
                case "Rechnungsnummer":
                    _invoice = _invoice with { InvoiceNumber = field.Value };
                    break;
                case "Rechnungsdatum":
                    if (DateOnly.TryParse(field.Value, out var date))
                    {
                        _invoice = _invoice with { InvoiceDate = date };
                    }
                    break;
                case "Aussteller":
                    _invoice = _invoice with { IssuerName = field.Value };
                    break;
                case "Straße":
                    _invoice = _invoice with { IssuerStreet = field.Value };
                    break;
                case "PLZ":
                    _invoice = _invoice with { IssuerPostalCode = field.Value };
                    break;
                case "Stadt":
                    _invoice = _invoice with { IssuerCity = field.Value };
                    break;
                case "Land":
                    _invoice = _invoice with { IssuerCountry = string.IsNullOrWhiteSpace(field.Value) ? null : field.Value };
                    break;
                case "Bruttobetrag":
                    var cleanValue = field.Value.Replace("€", "").Replace(".", "").Replace(",", ".").Trim();
                    if (decimal.TryParse(cleanValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var total))
                    {
                        _invoice = _invoice with { GrossTotal = total };
                    }
                    break;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_hasChanges && e.CloseReason == CloseReason.UserClosing)
        {
            var result = XtraMessageBox.Show(
                "Sie haben ungespeicherte Änderungen. Wirklich schließen?",
                "Schließen bestätigen",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        base.OnFormClosing(e);
    }
}

public class FieldDisplayItem : INotifyPropertyChanged
{
    private string _fieldType = string.Empty;
    private string _value = string.Empty;
    private float _confidence;

    public string FieldType
    {
        get => _fieldType;
        set
        {
            _fieldType = value;
            OnPropertyChanged(nameof(FieldType));
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    public float Confidence
    {
        get => _confidence;
        set
        {
            _confidence = value;
            OnPropertyChanged(nameof(Confidence));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

