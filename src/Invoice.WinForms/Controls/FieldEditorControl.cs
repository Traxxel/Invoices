using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace Invoice.WinForms.Controls;

public partial class FieldEditorControl : XtraUserControl
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FieldEditorControl> _logger;

    // DevExpress UI Controls
    private LabelControl _fieldTypeLabel = null!;
    private TextEdit _valueTextEdit = null!;
    private ComboBoxEdit _candidatesComboBox = null!;
    private SimpleButton _validateButton = null!;
    private SimpleButton _suggestButton = null!;
    private LabelControl _confidenceLabel = null!;
    private ProgressBarControl _confidenceProgressBar = null!;
    private LabelControl _alternativesLabel = null!;
    private ListBoxControl _alternativesListBox = null!;
    private SimpleButton _useAlternativeButton = null!;
    private LabelControl _validationLabel = null!;
    private PanelControl _validationPanel = null!;

    // Data
    private ExtractedField? _field;
    private List<AlternativeValue> _alternatives = new();
    private List<string> _candidates = new();
    private bool _isValidating;
    private bool _hasChanges;

    public event EventHandler<FieldValueChangedEventArgs>? FieldValueChanged;
    public event EventHandler<FieldValidatedEventArgs>? FieldValidated;

    public FieldEditorControl(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<FieldEditorControl>>();

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // Field type label
        _fieldTypeLabel = new LabelControl
        {
            Text = "Feldtyp",
            Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) },
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 200
        };

        // Value text edit
        _valueTextEdit = new TextEdit
        {
            Width = 300
        };
        _valueTextEdit.EditValueChanged += OnValueTextChanged;
        _valueTextEdit.KeyPress += OnValueTextKeyPress;

        // Candidates combo box
        _candidatesComboBox = new ComboBoxEdit
        {
            Width = 300
        };
        _candidatesComboBox.Properties.TextEditStyle = TextEditStyles.Standard;
        _candidatesComboBox.SelectedIndexChanged += OnCandidateSelected;

        // Validate button
        _validateButton = new SimpleButton
        {
            Text = "Validieren",
            Width = 100
        };
        _validateButton.Click += OnValidate;

        // Suggest button
        _suggestButton = new SimpleButton
        {
            Text = "Vorschlagen",
            Width = 100
        };
        _suggestButton.Click += OnSuggest;

        // Confidence display
        _confidenceLabel = new LabelControl
        {
            Text = "Konfidenz: 0%",
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 150
        };

        _confidenceProgressBar = new ProgressBarControl
        {
            Width = 250,
            Height = 25,
            Properties = { ShowTitle = true, Minimum = 0, Maximum = 100 }
        };

        // Alternatives
        _alternativesLabel = new LabelControl
        {
            Text = "Alternativen:",
            AutoSizeMode = LabelAutoSizeMode.None
        };

        _alternativesListBox = new ListBoxControl
        {
            Width = 300,
            Height = 120
        };
        _alternativesListBox.SelectedIndexChanged += OnAlternativeSelected;

        _useAlternativeButton = new SimpleButton
        {
            Text = "Alternative verwenden",
            Width = 180,
            Enabled = false
        };
        _useAlternativeButton.Click += OnUseAlternative;

        // Validation panel
        _validationPanel = new PanelControl
        {
            Height = 50,
            BorderStyle = BorderStyles.Simple
        };

        _validationLabel = new LabelControl
        {
            Text = "Validierung: Bereit",
            Dock = DockStyle.Fill,
            Appearance = { TextOptions = { VAlignment = DevExpress.Utils.VertAlignment.Center } }
        };
        _validationPanel.Controls.Add(_validationLabel);
    }

    private void LayoutControls()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(10)
        };

        // Field type
        mainLayout.Controls.Add(new LabelControl { Text = "Feldtyp:", AutoSizeMode = LabelAutoSizeMode.None }, 0, 0);
        mainLayout.Controls.Add(_fieldTypeLabel, 1, 0);

        // Value input
        mainLayout.Controls.Add(new LabelControl { Text = "Wert:", AutoSizeMode = LabelAutoSizeMode.None }, 0, 1);
        mainLayout.Controls.Add(_valueTextEdit, 1, 1);

        // Candidates
        mainLayout.Controls.Add(new LabelControl { Text = "Kandidaten:", AutoSizeMode = LabelAutoSizeMode.None }, 0, 2);
        mainLayout.Controls.Add(_candidatesComboBox, 1, 2);

        // Buttons
        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        buttonPanel.Controls.Add(_validateButton);
        buttonPanel.Controls.Add(_suggestButton);
        mainLayout.Controls.Add(new LabelControl(), 0, 3);
        mainLayout.Controls.Add(buttonPanel, 1, 3);

        // Confidence
        mainLayout.Controls.Add(new LabelControl { Text = "Konfidenz:", AutoSizeMode = LabelAutoSizeMode.None }, 0, 4);
        var confidencePanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        confidencePanel.Controls.Add(_confidenceLabel);
        confidencePanel.Controls.Add(_confidenceProgressBar);
        mainLayout.Controls.Add(confidencePanel, 1, 4);

        // Alternatives
        mainLayout.Controls.Add(_alternativesLabel, 0, 5);
        var alternativesPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };
        alternativesPanel.Controls.Add(_alternativesListBox);
        alternativesPanel.Controls.Add(_useAlternativeButton);
        mainLayout.Controls.Add(alternativesPanel, 1, 5);

        // Validation
        mainLayout.Controls.Add(new LabelControl { Text = "Status:", AutoSizeMode = LabelAutoSizeMode.None }, 0, 6);
        mainLayout.Controls.Add(_validationPanel, 1, 6);

        this.Controls.Add(mainLayout);
    }

    private void InitializeData()
    {
        _field = null;
        _alternatives = new List<AlternativeValue>();
        _candidates = new List<string>();
        _isValidating = false;
        _hasChanges = false;
    }

    public void SetField(ExtractedField field)
    {
        _field = field;
        LoadFieldData();
    }

    private void LoadFieldData()
    {
        if (_field == null) return;

        try
        {
            // Set field type
            _fieldTypeLabel.Text = _field.FieldType;

            // Set value
            _valueTextEdit.Text = _field.Value;

            // Set confidence
            UpdateConfidenceDisplay(_field.Confidence);

            // Load alternatives
            _alternatives = _field.Alternatives.ToList();
            LoadAlternatives();

            // Load candidates
            LoadCandidates();

            _hasChanges = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Felddaten");
            XtraMessageBox.Show(
                $"Fehler beim Laden der Felddaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadAlternatives()
    {
        _alternativesListBox.Items.Clear();

        foreach (var alternative in _alternatives)
        {
            var item = $"{alternative.Value} ({alternative.Confidence:P0})";
            _alternativesListBox.Items.Add(item);
        }
    }

    private async void LoadCandidates()
    {
        try
        {
            if (_field == null) return;

            // Get candidates based on field type
            var candidates = await GetCandidatesForFieldType(_field.FieldType);

            _candidatesComboBox.Properties.Items.Clear();
            _candidatesComboBox.Properties.Items.AddRange(candidates.ToArray());

            // Set current value
            if (!string.IsNullOrWhiteSpace(_field.Value))
            {
                _candidatesComboBox.EditValue = _field.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kandidaten");
        }
    }

    private async Task<List<string>> GetCandidatesForFieldType(string fieldType)
    {
        try
        {
            var candidates = new List<string>();

            switch (fieldType)
            {
                case "InvoiceNumber":
                    // Placeholder: Would get from database
                    break;

                case "IssuerName":
                    // Placeholder: Would get from database
                    break;

                case "InvoiceDate":
                    candidates.AddRange(GetRecentDates());
                    break;

                case "GrossTotal":
                    // Placeholder: Would get from database
                    break;
            }

            return candidates.Distinct().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kandidaten für Feldtyp: {FieldType}", fieldType);
            return new List<string>();
        }
    }

    private List<string> GetRecentDates()
    {
        var dates = new List<string>();
        var today = DateTime.Today;

        for (int i = 0; i < 30; i++)
        {
            var date = today.AddDays(-i);
            dates.Add(date.ToString("dd.MM.yyyy"));
        }

        return dates;
    }

    private void UpdateConfidenceDisplay(float confidence)
    {
        _confidenceLabel.Text = $"Konfidenz: {confidence:P0}";
        _confidenceProgressBar.Position = (int)(confidence * 100);

        // Color code confidence
        if (confidence < 0.5f)
        {
            _confidenceProgressBar.Properties.Appearance.ForeColor = Color.Red;
        }
        else if (confidence < 0.8f)
        {
            _confidenceProgressBar.Properties.Appearance.ForeColor = Color.Orange;
        }
        else
        {
            _confidenceProgressBar.Properties.Appearance.ForeColor = Color.Green;
        }
    }

    private void OnValueTextChanged(object? sender, EventArgs e)
    {
        _hasChanges = true;
        FieldValueChanged?.Invoke(this, new FieldValueChangedEventArgs(_field, _valueTextEdit.Text));
    }

    private void OnValueTextKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            OnValidate(sender, e);
        }
    }

    private void OnCandidateSelected(object? sender, EventArgs e)
    {
        if (_candidatesComboBox.EditValue != null)
        {
            _valueTextEdit.Text = _candidatesComboBox.EditValue.ToString();
            _hasChanges = true;
        }
    }

    private async void OnValidate(object? sender, EventArgs e)
    {
        try
        {
            if (_field == null) return;

            _isValidating = true;
            _validateButton.Enabled = false;
            _validationLabel.Text = "Validierung läuft...";
            _validationLabel.Appearance.ForeColor = Color.Blue;

            // Validate field value
            var validationResult = await ValidateFieldValue(_field.FieldType, _valueTextEdit.Text);

            if (validationResult.IsValid)
            {
                _validationLabel.Text = "Validierung: Gültig";
                _validationLabel.Appearance.ForeColor = Color.Green;
            }
            else
            {
                var errorMessage = validationResult.Errors.FirstOrDefault()?.Message ?? "Validierung fehlgeschlagen";
                _validationLabel.Text = $"Validierung: {errorMessage}";
                _validationLabel.Appearance.ForeColor = Color.Red;
            }

            FieldValidated?.Invoke(this, new FieldValidatedEventArgs(_field, validationResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Feldvalidierung");
            _validationLabel.Text = $"Validierung: Fehler - {ex.Message}";
            _validationLabel.Appearance.ForeColor = Color.Red;
        }
        finally
        {
            _isValidating = false;
            _validateButton.Enabled = true;
        }
    }

    private async Task<ValidationResult> ValidateFieldValue(string fieldType, string value)
    {
        try
        {
            switch (fieldType)
            {
                case "InvoiceNumber":
                    return await ValidateInvoiceNumber(value);
                case "InvoiceDate":
                    return await ValidateInvoiceDate(value);
                case "IssuerName":
                    return await ValidateIssuerName(value);
                case "GrossTotal":
                    return await ValidateGrossTotal(value);
                default:
                    return new ValidationResult(
                        true,
                        new List<ValidationError>(),
                        new List<ValidationWarning>(),
                        new Dictionary<string, object>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Feldwert-Validierung: {FieldType}, {Value}", fieldType, value);
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError(fieldType, "VALIDATION_ERROR", ex.Message, value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }
    }

    private async Task<ValidationResult> ValidateInvoiceNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("InvoiceNumber", "REQUIRED", "Rechnungsnummer erforderlich", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        return new ValidationResult(
            true,
            new List<ValidationError>(),
            new List<ValidationWarning>(),
            new Dictionary<string, object>());
    }

    private async Task<ValidationResult> ValidateInvoiceDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("InvoiceDate", "REQUIRED", "Rechnungsdatum erforderlich", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        if (!DateOnly.TryParse(value, out var date))
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("InvoiceDate", "INVALID_FORMAT", "Ungültiges Datumsformat", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        if (date > DateOnly.FromDateTime(DateTime.Today))
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("InvoiceDate", "FUTURE_DATE", "Rechnungsdatum darf nicht in der Zukunft liegen", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        return new ValidationResult(
            true,
            new List<ValidationError>(),
            new List<ValidationWarning>(),
            new Dictionary<string, object>());
    }

    private async Task<ValidationResult> ValidateIssuerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("IssuerName", "REQUIRED", "Ausstellername erforderlich", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        if (value.Length < 2)
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("IssuerName", "TOO_SHORT", "Ausstellername zu kurz", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        return new ValidationResult(
            true,
            new List<ValidationError>(),
            new List<ValidationWarning>(),
            new Dictionary<string, object>());
    }

    private async Task<ValidationResult> ValidateGrossTotal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("GrossTotal", "REQUIRED", "Bruttobetrag erforderlich", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        var cleanValue = value.Replace("€", "").Replace(".", "").Replace(",", ".").Trim();
        if (!decimal.TryParse(cleanValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("GrossTotal", "INVALID_FORMAT", "Ungültiges Betragsformat", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        if (amount <= 0)
        {
            return new ValidationResult(
                false,
                new List<ValidationError> { new ValidationError("GrossTotal", "INVALID_AMOUNT", "Betrag muss größer als Null sein", value, null) },
                new List<ValidationWarning>(),
                new Dictionary<string, object>());
        }

        return new ValidationResult(
            true,
            new List<ValidationError>(),
            new List<ValidationWarning>(),
            new Dictionary<string, object>());
    }

    private async void OnSuggest(object? sender, EventArgs e)
    {
        try
        {
            if (_field == null) return;

            // Get suggestions based on field type
            var suggestions = await GetSuggestionsForFieldType(_field.FieldType);

            if (suggestions.Any())
            {
                _candidatesComboBox.Properties.Items.Clear();
                _candidatesComboBox.Properties.Items.AddRange(suggestions.ToArray());
                _candidatesComboBox.ShowPopup();
            }
            else
            {
                XtraMessageBox.Show(
                    "Keine Vorschläge verfügbar",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen von Vorschlägen");
            XtraMessageBox.Show(
                $"Fehler beim Abrufen von Vorschlägen: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task<List<string>> GetSuggestionsForFieldType(string fieldType)
    {
        // Placeholder: Implementation would depend on specific field type
        return new List<string>();
    }

    private void OnAlternativeSelected(object? sender, EventArgs e)
    {
        _useAlternativeButton.Enabled = _alternativesListBox.SelectedIndex >= 0;
    }

    private void OnUseAlternative(object? sender, EventArgs e)
    {
        if (_alternativesListBox.SelectedIndex >= 0)
        {
            var selectedIndex = _alternativesListBox.SelectedIndex;
            if (selectedIndex < _alternatives.Count)
            {
                var alternative = _alternatives[selectedIndex];
                _valueTextEdit.Text = alternative.Value;
                _hasChanges = true;
            }
        }
    }

    public string GetCurrentValue()
    {
        return _valueTextEdit.Text;
    }

    public bool HasChanges()
    {
        return _hasChanges;
    }

    public void ClearChanges()
    {
        _hasChanges = false;
    }
}

public class FieldValueChangedEventArgs : EventArgs
{
    public ExtractedField? Field { get; }
    public string NewValue { get; }

    public FieldValueChangedEventArgs(ExtractedField? field, string newValue)
    {
        Field = field;
        NewValue = newValue;
    }
}

public class FieldValidatedEventArgs : EventArgs
{
    public ExtractedField? Field { get; }
    public ValidationResult ValidationResult { get; }

    public FieldValidatedEventArgs(ExtractedField? field, ValidationResult validationResult)
    {
        Field = field;
        ValidationResult = validationResult;
    }
}

