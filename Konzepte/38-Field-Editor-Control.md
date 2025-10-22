# Aufgabe 38: Field Editor Control (Kandidaten-Dropdown)

## Ziel

Field Editor Control für die Bearbeitung von extrahierten Feldern mit Kandidaten-Dropdown und Alternativen.

## 1. Field Editor Control Interface

**Datei:** `src/Invoice.WinForms/Controls/FieldEditorControl.cs`

```csharp
using Invoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Invoice.WinForms.Controls;

public partial class FieldEditorControl : XtraUserControl
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FieldEditorControl> _logger;
    private readonly IRegexPatternService _regexPatternService;
    private readonly IParserHelperService _parserHelperService;

    // UI Controls
    private Label _fieldTypeLabel;
    private TextBox _valueTextBox;
    private ComboBox _candidatesComboBox;
    private Button _validateButton;
    private Button _suggestButton;
    private Label _confidenceLabel;
    private ProgressBar _confidenceProgressBar;
    private Label _alternativesLabel;
    private ListBox _alternativesListBox;
    private Button _useAlternativeButton;
    private Label _validationLabel;
    private Panel _validationPanel;

    // Data
    private ExtractedField? _field;
    private List<AlternativeValue> _alternatives;
    private List<string> _candidates;
    private bool _isValidating;
    private bool _hasChanges;

    public event EventHandler<FieldValueChangedEventArgs>? FieldValueChanged;
    public event EventHandler<FieldValidatedEventArgs>? FieldValidated;

    public FieldEditorControl(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<FieldEditorControl>>();
        _regexPatternService = serviceProvider.GetRequiredService<IRegexPatternService>();
        _parserHelperService = serviceProvider.GetRequiredService<IParserHelperService>();

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BorderStyle = BorderStyle.FixedSingle;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // Field type label
        _fieldTypeLabel = new Label
        {
            Text = "Field Type",
            Font = new Font("Arial", 10, FontStyle.Bold),
            AutoSize = true
        };

        // Value text box
        _valueTextBox = new TextBox
        {
            Width = 300,
            Height = 25
        };
        _valueTextBox.TextChanged += OnValueTextChanged;
        _valueTextBox.KeyPress += OnValueTextKeyPress;

        // Candidates combo box
        _candidatesComboBox = new ComboBox
        {
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems
        };
        _candidatesComboBox.SelectedIndexChanged += OnCandidateSelected;

        // Validate button
        _validateButton = new Button
        {
            Text = "Validate",
            Width = 80,
            Height = 25
        };
        _validateButton.Click += OnValidate;

        // Suggest button
        _suggestButton = new Button
        {
            Text = "Suggest",
            Width = 80,
            Height = 25
        };
        _suggestButton.Click += OnSuggest;

        // Confidence display
        _confidenceLabel = new Label
        {
            Text = "Confidence: 0%",
            AutoSize = true
        };

        _confidenceProgressBar = new ProgressBar
        {
            Width = 200,
            Height = 20,
            Style = ProgressBarStyle.Continuous
        };

        // Alternatives
        _alternativesLabel = new Label
        {
            Text = "Alternatives:",
            AutoSize = true
        };

        _alternativesListBox = new ListBox
        {
            Width = 300,
            Height = 100
        };
        _alternativesListBox.SelectedIndexChanged += OnAlternativeSelected;

        _useAlternativeButton = new Button
        {
            Text = "Use Alternative",
            Width = 120,
            Height = 25,
            Enabled = false
        };
        _useAlternativeButton.Click += OnUseAlternative;

        // Validation panel
        _validationPanel = new Panel
        {
            Height = 50,
            BorderStyle = BorderStyle.FixedSingle
        };

        _validationLabel = new Label
        {
            Text = "Validation: Ready",
            AutoSize = true,
            Dock = DockStyle.Fill
        };
        _validationPanel.Controls.Add(_validationLabel);
    }

    private void LayoutControls()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6
        };

        // Field type
        mainLayout.Controls.Add(_fieldTypeLabel, 0, 0);
        mainLayout.Controls.Add(new Label { Text = "" }, 1, 0);

        // Value input
        mainLayout.Controls.Add(new Label { Text = "Value:" }, 0, 1);
        mainLayout.Controls.Add(_valueTextBox, 1, 1);

        // Candidates
        mainLayout.Controls.Add(new Label { Text = "Candidates:" }, 0, 2);
        mainLayout.Controls.Add(_candidatesComboBox, 1, 2);

        // Buttons
        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        buttonPanel.Controls.Add(_validateButton);
        buttonPanel.Controls.Add(_suggestButton);
        mainLayout.Controls.Add(buttonPanel, 1, 3);

        // Confidence
        mainLayout.Controls.Add(new Label { Text = "Confidence:" }, 0, 4);
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

        this.Controls.Add(mainLayout);
        this.Controls.Add(_validationPanel);
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
            _valueTextBox.Text = _field.Value;

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
            _logger.LogError(ex, "Failed to load field data");
            MessageBox.Show($"Failed to load field data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            _candidatesComboBox.Items.Clear();
            _candidatesComboBox.Items.AddRange(candidates.ToArray());

            // Set current value as selected
            var currentIndex = _candidatesComboBox.Items.IndexOf(_field.Value);
            if (currentIndex >= 0)
            {
                _candidatesComboBox.SelectedIndex = currentIndex;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load candidates");
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
                    // Get invoice numbers from database
                    var invoiceNumbers = await GetInvoiceNumbersFromDatabase();
                    candidates.AddRange(invoiceNumbers);
                    break;

                case "IssuerName":
                    // Get issuer names from database
                    var issuerNames = await GetIssuerNamesFromDatabase();
                    candidates.AddRange(issuerNames);
                    break;

                case "InvoiceDate":
                    // Get recent dates
                    var recentDates = GetRecentDates();
                    candidates.AddRange(recentDates);
                    break;

                case "GrossTotal":
                    // Get recent amounts
                    var recentAmounts = await GetRecentAmountsFromDatabase();
                    candidates.AddRange(recentAmounts);
                    break;
            }

            return candidates.Distinct().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get candidates for field type: {FieldType}", fieldType);
            return new List<string>();
        }
    }

    private async Task<List<string>> GetInvoiceNumbersFromDatabase()
    {
        // Implementation would depend on the database service
        return new List<string>();
    }

    private async Task<List<string>> GetIssuerNamesFromDatabase()
    {
        // Implementation would depend on the database service
        return new List<string>();
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

    private async Task<List<string>> GetRecentAmountsFromDatabase()
    {
        // Implementation would depend on the database service
        return new List<string>();
    }

    private void UpdateConfidenceDisplay(float confidence)
    {
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

    private void OnValueTextChanged(object? sender, EventArgs e)
    {
        _hasChanges = true;
        FieldValueChanged?.Invoke(this, new FieldValueChangedEventArgs(_field, _valueTextBox.Text));
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
        if (_candidatesComboBox.SelectedItem != null)
        {
            _valueTextBox.Text = _candidatesComboBox.SelectedItem.ToString();
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
            _validationLabel.Text = "Validating...";

            // Validate field value
            var validationResult = await ValidateFieldValue(_field.FieldType, _valueTextBox.Text);

            if (validationResult.IsValid)
            {
                _validationLabel.Text = "Validation: Valid";
                _validationLabel.ForeColor = Color.Green;
            }
            else
            {
                _validationLabel.Text = $"Validation: {validationResult.Message}";
                _validationLabel.ForeColor = Color.Red;
            }

            FieldValidated?.Invoke(this, new FieldValidatedEventArgs(_field, validationResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate field");
            _validationLabel.Text = $"Validation: Error - {ex.Message}";
            _validationLabel.ForeColor = Color.Red;
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
                    return new ValidationResult(true, "Validation not implemented", new List<ValidationError>(), new List<ValidationWarning>(), new Dictionary<string, object>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate field value: {FieldType}, {Value}", fieldType, value);
            return new ValidationResult(false, "Validation failed", new List<ValidationError> { new ValidationError(fieldType, "VALIDATION_ERROR", ex.Message, value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }
    }

    private async Task<ValidationResult> ValidateInvoiceNumber(string value)
    {
        // Validate invoice number format
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult(false, "Invoice number is required", new List<ValidationError> { new ValidationError("InvoiceNumber", "REQUIRED", "Invoice number is required", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        // Check for duplicates
        var duplicates = await GetInvoiceNumbersFromDatabase();
        if (duplicates.Contains(value))
        {
            return new ValidationResult(false, "Invoice number already exists", new List<ValidationError> { new ValidationError("InvoiceNumber", "DUPLICATE", "Invoice number already exists", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        return new ValidationResult(true, "Invoice number is valid", new List<ValidationError>(), new List<ValidationWarning>(), new Dictionary<string, object>());
    }

    private async Task<ValidationResult> ValidateInvoiceDate(string value)
    {
        // Validate date format
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult(false, "Invoice date is required", new List<ValidationError> { new ValidationError("InvoiceDate", "REQUIRED", "Invoice date is required", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        if (!DateOnly.TryParse(value, out var date))
        {
            return new ValidationResult(false, "Invalid date format", new List<ValidationError> { new ValidationError("InvoiceDate", "INVALID_FORMAT", "Invalid date format", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        // Check if date is in the future
        if (date > DateOnly.FromDateTime(DateTime.Today))
        {
            return new ValidationResult(false, "Invoice date cannot be in the future", new List<ValidationError> { new ValidationError("InvoiceDate", "FUTURE_DATE", "Invoice date cannot be in the future", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        return new ValidationResult(true, "Invoice date is valid", new List<ValidationError>(), new List<ValidationWarning>(), new Dictionary<string, object>());
    }

    private async Task<ValidationResult> ValidateIssuerName(string value)
    {
        // Validate issuer name
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult(false, "Issuer name is required", new List<ValidationError> { new ValidationError("IssuerName", "REQUIRED", "Issuer name is required", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        if (value.Length < 2)
        {
            return new ValidationResult(false, "Issuer name is too short", new List<ValidationError> { new ValidationError("IssuerName", "TOO_SHORT", "Issuer name is too short", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        return new ValidationResult(true, "Issuer name is valid", new List<ValidationError>(), new List<ValidationWarning>(), new Dictionary<string, object>());
    }

    private async Task<ValidationResult> ValidateGrossTotal(string value)
    {
        // Validate amount
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult(false, "Gross total is required", new List<ValidationError> { new ValidationError("GrossTotal", "REQUIRED", "Gross total is required", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        if (!decimal.TryParse(value, out var amount))
        {
            return new ValidationResult(false, "Invalid amount format", new List<ValidationError> { new ValidationError("GrossTotal", "INVALID_FORMAT", "Invalid amount format", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        if (amount <= 0)
        {
            return new ValidationResult(false, "Amount must be greater than zero", new List<ValidationError> { new ValidationError("GrossTotal", "INVALID_AMOUNT", "Amount must be greater than zero", value, null) }, new List<ValidationWarning>(), new Dictionary<string, object>());
        }

        return new ValidationResult(true, "Gross total is valid", new List<ValidationError>(), new List<ValidationWarning>(), new Dictionary<string, object>());
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
                _candidatesComboBox.Items.Clear();
                _candidatesComboBox.Items.AddRange(suggestions.ToArray());
                _candidatesComboBox.DroppedDown = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get suggestions");
            MessageBox.Show($"Failed to get suggestions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task<List<string>> GetSuggestionsForFieldType(string fieldType)
    {
        // Implementation would depend on the specific field type
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
                _valueTextBox.Text = alternative.Value;
                _hasChanges = true;
            }
        }
    }

    public string GetCurrentValue()
    {
        return _valueTextBox.Text;
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
```

## Wichtige Hinweise

- Field Editor Control für Feldbearbeitung
- Kandidaten-Dropdown mit AutoComplete
- Alternativen-Liste mit Auswahl
- Validation für verschiedene Feldtypen
- Confidence-Anzeige mit Progress Bar
- Suggest-Funktionalität
- Change Tracking
- Event Handling für Value Changes
- Error Handling für alle Operationen
