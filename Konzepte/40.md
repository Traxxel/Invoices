# Aufgabe 40: SettingsForm (Pfade, DB-Connection)

## Ziel

SettingsForm für die Konfiguration von Pfaden, Datenbankverbindung und anderen Anwendungseinstellungen.

## 1. SettingsForm Interface

**Datei:** `src/InvoiceReader.WinForms/Forms/SettingsForm.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvoiceReader.WinForms.Forms;

public partial class SettingsForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SettingsForm> _logger;
    private readonly IConfigurationService _configurationService;

    // UI Controls
    private TabControl _mainTabControl;
    private TabPage _generalTabPage;
    private TabPage _databaseTabPage;
    private TabPage _pathsTabPage;
    private TabPage _mlTabPage;
    private TabPage _uiTabPage;

    // General settings
    private TextBox _applicationNameTextBox;
    private TextBox _versionTextBox;
    private CheckBox _autoSaveCheckBox;
    private CheckBox _createBackupCheckBox;
    private NumericUpDown _autoSaveIntervalNumericUpDown;
    private ComboBox _languageComboBox;
    private ComboBox _themeComboBox;

    // Database settings
    private TextBox _connectionStringTextBox;
    private TextBox _databaseNameTextBox;
    private TextBox _serverTextBox;
    private TextBox _usernameTextBox;
    private TextBox _passwordTextBox;
    private CheckBox _integratedSecurityCheckBox;
    private Button _testConnectionButton;
    private Label _connectionStatusLabel;

    // Paths settings
    private TextBox _dataPathTextBox;
    private TextBox _modelsPathTextBox;
    private TextBox _logsPathTextBox;
    private TextBox _tempPathTextBox;
    private Button _browseDataPathButton;
    private Button _browseModelsPathButton;
    private Button _browseLogsPathButton;
    private Button _browseTempPathButton;

    // ML settings
    private ComboBox _defaultAlgorithmComboBox;
    private NumericUpDown _defaultConfidenceThresholdNumericUpDown;
    private CheckBox _useDataAugmentationCheckBox;
    private CheckBox _useCrossValidationCheckBox;
    private NumericUpDown _crossValidationFoldsNumericUpDown;
    private TextBox _modelVersionTextBox;

    // UI settings
    private CheckBox _showTooltipsCheckBox;
    private CheckBox _showStatusBarCheckBox;
    private CheckBox _showProgressBarCheckBox;
    private NumericUpDown _gridRowHeightNumericUpDown;
    private ComboBox _dateFormatComboBox;
    private ComboBox _numberFormatComboBox;

    // Buttons
    private Button _saveButton;
    private Button _cancelButton;
    private Button _resetButton;
    private Button _importButton;
    private Button _exportButton;

    // Data
    private ApplicationSettings _settings;
    private bool _hasChanges;

    public SettingsForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<SettingsForm>>();
        _configurationService = serviceProvider.GetRequiredService<IConfigurationService>();

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Text = "Settings";
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
        // Main tab control
        _mainTabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // General tab
        _generalTabPage = new TabPage("General");
        CreateGeneralTabControls();
        _mainTabControl.TabPages.Add(_generalTabPage);

        // Database tab
        _databaseTabPage = new TabPage("Database");
        CreateDatabaseTabControls();
        _mainTabControl.TabPages.Add(_databaseTabPage);

        // Paths tab
        _pathsTabPage = new TabPage("Paths");
        CreatePathsTabControls();
        _mainTabControl.TabPages.Add(_pathsTabPage);

        // ML tab
        _mlTabPage = new TabPage("Machine Learning");
        CreateMLTabControls();
        _mainTabControl.TabPages.Add(_mlTabPage);

        // UI tab
        _uiTabPage = new TabPage("User Interface");
        CreateUITabControls();
        _mainTabControl.TabPages.Add(_uiTabPage);
    }

    private void CreateGeneralTabControls()
    {
        _applicationNameTextBox = new TextBox
        {
            Width = 200
        };

        _versionTextBox = new TextBox
        {
            Width = 200,
            ReadOnly = true
        };

        _autoSaveCheckBox = new CheckBox
        {
            Text = "Enable auto-save"
        };

        _createBackupCheckBox = new CheckBox
        {
            Text = "Create backup before saving"
        };

        _autoSaveIntervalNumericUpDown = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 60,
            Value = 5,
            Width = 100
        };

        _languageComboBox = new ComboBox
        {
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _languageComboBox.Items.AddRange(new[] { "English", "German", "French", "Spanish" });
        _languageComboBox.SelectedIndex = 0;

        _themeComboBox = new ComboBox
        {
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _themeComboBox.Items.AddRange(new[] { "Light", "Dark", "System" });
        _themeComboBox.SelectedIndex = 0;
    }

    private void CreateDatabaseTabControls()
    {
        _connectionStringTextBox = new TextBox
        {
            Width = 400,
            Height = 60,
            Multiline = true
        };

        _databaseNameTextBox = new TextBox
        {
            Width = 200
        };

        _serverTextBox = new TextBox
        {
            Width = 200
        };

        _usernameTextBox = new TextBox
        {
            Width = 200
        };

        _passwordTextBox = new TextBox
        {
            Width = 200,
            UseSystemPasswordChar = true
        };

        _integratedSecurityCheckBox = new CheckBox
        {
            Text = "Use Integrated Security"
        };

        _testConnectionButton = new Button
        {
            Text = "Test Connection",
            Width = 120
        };
        _testConnectionButton.Click += OnTestConnection;

        _connectionStatusLabel = new Label
        {
            Text = "Not tested",
            AutoSize = true
        };
    }

    private void CreatePathsTabControls()
    {
        _dataPathTextBox = new TextBox
        {
            Width = 300
        };

        _modelsPathTextBox = new TextBox
        {
            Width = 300
        };

        _logsPathTextBox = new TextBox
        {
            Width = 300
        };

        _tempPathTextBox = new TextBox
        {
            Width = 300
        };

        _browseDataPathButton = new Button
        {
            Text = "Browse...",
            Width = 80
        };
        _browseDataPathButton.Click += OnBrowseDataPath;

        _browseModelsPathButton = new Button
        {
            Text = "Browse...",
            Width = 80
        };
        _browseModelsPathButton.Click += OnBrowseModelsPath;

        _browseLogsPathButton = new Button
        {
            Text = "Browse...",
            Width = 80
        };
        _browseLogsPathButton.Click += OnBrowseLogsPath;

        _browseTempPathButton = new Button
        {
            Text = "Browse...",
            Width = 80
        };
        _browseTempPathButton.Click += OnBrowseTempPath;
    }

    private void CreateMLTabControls()
    {
        _defaultAlgorithmComboBox = new ComboBox
        {
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _defaultAlgorithmComboBox.Items.AddRange(new[] { "SdcaMaximumEntropy", "LbfgsMaximumEntropy", "FastTree", "FastForest" });
        _defaultAlgorithmComboBox.SelectedIndex = 0;

        _defaultConfidenceThresholdNumericUpDown = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1,
            Increment = 0.1m,
            DecimalPlaces = 1,
            Value = 0.7m,
            Width = 100
        };

        _useDataAugmentationCheckBox = new CheckBox
        {
            Text = "Use data augmentation"
        };

        _useCrossValidationCheckBox = new CheckBox
        {
            Text = "Use cross validation"
        };

        _crossValidationFoldsNumericUpDown = new NumericUpDown
        {
            Minimum = 2,
            Maximum = 10,
            Value = 5,
            Width = 100
        };

        _modelVersionTextBox = new TextBox
        {
            Width = 200
        };
    }

    private void CreateUITabControls()
    {
        _showTooltipsCheckBox = new CheckBox
        {
            Text = "Show tooltips"
        };

        _showStatusBarCheckBox = new CheckBox
        {
            Text = "Show status bar"
        };

        _showProgressBarCheckBox = new CheckBox
        {
            Text = "Show progress bar"
        };

        _gridRowHeightNumericUpDown = new NumericUpDown
        {
            Minimum = 20,
            Maximum = 50,
            Value = 25,
            Width = 100
        };

        _dateFormatComboBox = new ComboBox
        {
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _dateFormatComboBox.Items.AddRange(new[] { "dd.MM.yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" });
        _dateFormatComboBox.SelectedIndex = 0;

        _numberFormatComboBox = new ComboBox
        {
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _numberFormatComboBox.Items.AddRange(new[] { "1,234.56", "1.234,56", "1 234,56" });
        _numberFormatComboBox.SelectedIndex = 0;
    }

    private void LayoutControls()
    {
        // General tab layout
        var generalLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6
        };

        generalLayout.Controls.Add(new Label { Text = "Application Name:" }, 0, 0);
        generalLayout.Controls.Add(_applicationNameTextBox, 1, 0);
        generalLayout.Controls.Add(new Label { Text = "Version:" }, 0, 1);
        generalLayout.Controls.Add(_versionTextBox, 1, 1);
        generalLayout.Controls.Add(_autoSaveCheckBox, 0, 2);
        generalLayout.Controls.Add(_autoSaveIntervalNumericUpDown, 1, 2);
        generalLayout.Controls.Add(_createBackupCheckBox, 0, 3);
        generalLayout.Controls.Add(new Label { Text = "" }, 1, 3);
        generalLayout.Controls.Add(new Label { Text = "Language:" }, 0, 4);
        generalLayout.Controls.Add(_languageComboBox, 1, 4);
        generalLayout.Controls.Add(new Label { Text = "Theme:" }, 0, 5);
        generalLayout.Controls.Add(_themeComboBox, 1, 5);

        _generalTabPage.Controls.Add(generalLayout);

        // Database tab layout
        var databaseLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6
        };

        databaseLayout.Controls.Add(new Label { Text = "Connection String:" }, 0, 0);
        databaseLayout.Controls.Add(_connectionStringTextBox, 1, 0);
        databaseLayout.Controls.Add(new Label { Text = "Database Name:" }, 0, 1);
        databaseLayout.Controls.Add(_databaseNameTextBox, 1, 1);
        databaseLayout.Controls.Add(new Label { Text = "Server:" }, 0, 2);
        databaseLayout.Controls.Add(_serverTextBox, 1, 2);
        databaseLayout.Controls.Add(new Label { Text = "Username:" }, 0, 3);
        databaseLayout.Controls.Add(_usernameTextBox, 1, 3);
        databaseLayout.Controls.Add(new Label { Text = "Password:" }, 0, 4);
        databaseLayout.Controls.Add(_passwordTextBox, 1, 4);
        databaseLayout.Controls.Add(_integratedSecurityCheckBox, 0, 5);
        databaseLayout.Controls.Add(_testConnectionButton, 1, 5);

        var databaseStatusPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        databaseStatusPanel.Controls.Add(_connectionStatusLabel);

        _databaseTabPage.Controls.Add(databaseLayout);
        _databaseTabPage.Controls.Add(databaseStatusPanel);

        // Paths tab layout
        var pathsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4
        };

        pathsLayout.Controls.Add(new Label { Text = "Data Path:" }, 0, 0);
        pathsLayout.Controls.Add(_dataPathTextBox, 1, 0);
        pathsLayout.Controls.Add(_browseDataPathButton, 2, 0);
        pathsLayout.Controls.Add(new Label { Text = "Models Path:" }, 0, 1);
        pathsLayout.Controls.Add(_modelsPathTextBox, 1, 1);
        pathsLayout.Controls.Add(_browseModelsPathButton, 2, 1);
        pathsLayout.Controls.Add(new Label { Text = "Logs Path:" }, 0, 2);
        pathsLayout.Controls.Add(_logsPathTextBox, 1, 2);
        pathsLayout.Controls.Add(_browseLogsPathButton, 2, 2);
        pathsLayout.Controls.Add(new Label { Text = "Temp Path:" }, 0, 3);
        pathsLayout.Controls.Add(_tempPathTextBox, 1, 3);
        pathsLayout.Controls.Add(_browseTempPathButton, 2, 3);

        _pathsTabPage.Controls.Add(pathsLayout);

        // ML tab layout
        var mlLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6
        };

        mlLayout.Controls.Add(new Label { Text = "Default Algorithm:" }, 0, 0);
        mlLayout.Controls.Add(_defaultAlgorithmComboBox, 1, 0);
        mlLayout.Controls.Add(new Label { Text = "Confidence Threshold:" }, 0, 1);
        mlLayout.Controls.Add(_defaultConfidenceThresholdNumericUpDown, 1, 1);
        mlLayout.Controls.Add(_useDataAugmentationCheckBox, 0, 2);
        mlLayout.Controls.Add(new Label { Text = "" }, 1, 2);
        mlLayout.Controls.Add(_useCrossValidationCheckBox, 0, 3);
        mlLayout.Controls.Add(_crossValidationFoldsNumericUpDown, 1, 3);
        mlLayout.Controls.Add(new Label { Text = "Model Version:" }, 0, 4);
        mlLayout.Controls.Add(_modelVersionTextBox, 1, 4);

        _mlTabPage.Controls.Add(mlLayout);

        // UI tab layout
        var uiLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6
        };

        uiLayout.Controls.Add(_showTooltipsCheckBox, 0, 0);
        uiLayout.Controls.Add(new Label { Text = "" }, 1, 0);
        uiLayout.Controls.Add(_showStatusBarCheckBox, 0, 1);
        uiLayout.Controls.Add(new Label { Text = "" }, 1, 1);
        uiLayout.Controls.Add(_showProgressBarCheckBox, 0, 2);
        uiLayout.Controls.Add(new Label { Text = "" }, 1, 2);
        uiLayout.Controls.Add(new Label { Text = "Grid Row Height:" }, 0, 3);
        uiLayout.Controls.Add(_gridRowHeightNumericUpDown, 1, 3);
        uiLayout.Controls.Add(new Label { Text = "Date Format:" }, 0, 4);
        uiLayout.Controls.Add(_dateFormatComboBox, 1, 4);
        uiLayout.Controls.Add(new Label { Text = "Number Format:" }, 0, 5);
        uiLayout.Controls.Add(_numberFormatComboBox, 1, 5);

        _uiTabPage.Controls.Add(uiLayout);

        // Buttons
        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            AutoSize = true
        };

        _saveButton = new Button
        {
            Text = "Save",
            Width = 80
        };
        _saveButton.Click += OnSave;

        _cancelButton = new Button
        {
            Text = "Cancel",
            Width = 80
        };
        _cancelButton.Click += OnCancel;

        _resetButton = new Button
        {
            Text = "Reset",
            Width = 80
        };
        _resetButton.Click += OnReset;

        _importButton = new Button
        {
            Text = "Import",
            Width = 80
        };
        _importButton.Click += OnImport;

        _exportButton = new Button
        {
            Text = "Export",
            Width = 80
        };
        _exportButton.Click += OnExport;

        buttonPanel.Controls.AddRange(new Control[] {
            _cancelButton, _saveButton, _resetButton, _importButton, _exportButton
        });

        this.Controls.Add(_mainTabControl);
        this.Controls.Add(buttonPanel);
    }

    private void InitializeData()
    {
        _settings = new ApplicationSettings();
        _hasChanges = false;
        LoadSettings();
    }

    private async void LoadSettings()
    {
        try
        {
            _settings = await _configurationService.GetSettingsAsync();
            PopulateControls();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            MessageBox.Show($"Failed to load settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PopulateControls()
    {
        // General settings
        _applicationNameTextBox.Text = _settings.ApplicationName;
        _versionTextBox.Text = _settings.Version;
        _autoSaveCheckBox.Checked = _settings.AutoSave;
        _createBackupCheckBox.Checked = _settings.CreateBackup;
        _autoSaveIntervalNumericUpDown.Value = _settings.AutoSaveInterval;
        _languageComboBox.SelectedItem = _settings.Language;
        _themeComboBox.SelectedItem = _settings.Theme;

        // Database settings
        _connectionStringTextBox.Text = _settings.DatabaseConnectionString;
        _databaseNameTextBox.Text = _settings.DatabaseName;
        _serverTextBox.Text = _settings.DatabaseServer;
        _usernameTextBox.Text = _settings.DatabaseUsername;
        _passwordTextBox.Text = _settings.DatabasePassword;
        _integratedSecurityCheckBox.Checked = _settings.DatabaseIntegratedSecurity;

        // Paths settings
        _dataPathTextBox.Text = _settings.DataPath;
        _modelsPathTextBox.Text = _settings.ModelsPath;
        _logsPathTextBox.Text = _settings.LogsPath;
        _tempPathTextBox.Text = _settings.TempPath;

        // ML settings
        _defaultAlgorithmComboBox.SelectedItem = _settings.DefaultAlgorithm;
        _defaultConfidenceThresholdNumericUpDown.Value = _settings.DefaultConfidenceThreshold;
        _useDataAugmentationCheckBox.Checked = _settings.UseDataAugmentation;
        _useCrossValidationCheckBox.Checked = _settings.UseCrossValidation;
        _crossValidationFoldsNumericUpDown.Value = _settings.CrossValidationFolds;
        _modelVersionTextBox.Text = _settings.ModelVersion;

        // UI settings
        _showTooltipsCheckBox.Checked = _settings.ShowTooltips;
        _showStatusBarCheckBox.Checked = _settings.ShowStatusBar;
        _showProgressBarCheckBox.Checked = _settings.ShowProgressBar;
        _gridRowHeightNumericUpDown.Value = _settings.GridRowHeight;
        _dateFormatComboBox.SelectedItem = _settings.DateFormat;
        _numberFormatComboBox.SelectedItem = _settings.NumberFormat;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        try
        {
            UpdateSettingsFromControls();
            _configurationService.SaveSettingsAsync(_settings);
            _hasChanges = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    private void OnReset(object? sender, EventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to reset all settings to default values?", "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            _settings = new ApplicationSettings();
            PopulateControls();
            _hasChanges = true;
        }
    }

    private void OnImport(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Import Settings"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImportSettings(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import settings");
            MessageBox.Show($"Failed to import settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnExport(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Export Settings"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ExportSettings(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export settings");
            MessageBox.Show($"Failed to export settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnTestConnection(object? sender, EventArgs e)
    {
        try
        {
            _testConnectionButton.Enabled = false;
            _connectionStatusLabel.Text = "Testing connection...";

            var connectionString = _connectionStringTextBox.Text;
            var success = await _configurationService.TestDatabaseConnectionAsync(connectionString);

            if (success)
            {
                _connectionStatusLabel.Text = "Connection successful";
                _connectionStatusLabel.ForeColor = Color.Green;
            }
            else
            {
                _connectionStatusLabel.Text = "Connection failed";
                _connectionStatusLabel.ForeColor = Color.Red;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test database connection");
            _connectionStatusLabel.Text = $"Connection error: {ex.Message}";
            _connectionStatusLabel.ForeColor = Color.Red;
        }
        finally
        {
            _testConnectionButton.Enabled = true;
        }
    }

    private void OnBrowseDataPath(object? sender, EventArgs e)
    {
        BrowseFolder(_dataPathTextBox);
    }

    private void OnBrowseModelsPath(object? sender, EventArgs e)
    {
        BrowseFolder(_modelsPathTextBox);
    }

    private void OnBrowseLogsPath(object? sender, EventArgs e)
    {
        BrowseFolder(_logsPathTextBox);
    }

    private void OnBrowseTempPath(object? sender, EventArgs e)
    {
        BrowseFolder(_tempPathTextBox);
    }

    private void BrowseFolder(TextBox textBox)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select folder",
            SelectedPath = textBox.Text
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            textBox.Text = dialog.SelectedPath;
            _hasChanges = true;
        }
    }

    private void UpdateSettingsFromControls()
    {
        // General settings
        _settings.ApplicationName = _applicationNameTextBox.Text;
        _settings.AutoSave = _autoSaveCheckBox.Checked;
        _settings.CreateBackup = _createBackupCheckBox.Checked;
        _settings.AutoSaveInterval = (int)_autoSaveIntervalNumericUpDown.Value;
        _settings.Language = _languageComboBox.SelectedItem?.ToString() ?? "English";
        _settings.Theme = _themeComboBox.SelectedItem?.ToString() ?? "Light";

        // Database settings
        _settings.DatabaseConnectionString = _connectionStringTextBox.Text;
        _settings.DatabaseName = _databaseNameTextBox.Text;
        _settings.DatabaseServer = _serverTextBox.Text;
        _settings.DatabaseUsername = _usernameTextBox.Text;
        _settings.DatabasePassword = _passwordTextBox.Text;
        _settings.DatabaseIntegratedSecurity = _integratedSecurityCheckBox.Checked;

        // Paths settings
        _settings.DataPath = _dataPathTextBox.Text;
        _settings.ModelsPath = _modelsPathTextBox.Text;
        _settings.LogsPath = _logsPathTextBox.Text;
        _settings.TempPath = _tempPathTextBox.Text;

        // ML settings
        _settings.DefaultAlgorithm = _defaultAlgorithmComboBox.SelectedItem?.ToString() ?? "SdcaMaximumEntropy";
        _settings.DefaultConfidenceThreshold = (float)_defaultConfidenceThresholdNumericUpDown.Value;
        _settings.UseDataAugmentation = _useDataAugmentationCheckBox.Checked;
        _settings.UseCrossValidation = _useCrossValidationCheckBox.Checked;
        _settings.CrossValidationFolds = (int)_crossValidationFoldsNumericUpDown.Value;
        _settings.ModelVersion = _modelVersionTextBox.Text;

        // UI settings
        _settings.ShowTooltips = _showTooltipsCheckBox.Checked;
        _settings.ShowStatusBar = _showStatusBarCheckBox.Checked;
        _settings.ShowProgressBar = _showProgressBarCheckBox.Checked;
        _settings.GridRowHeight = (int)_gridRowHeightNumericUpDown.Value;
        _settings.DateFormat = _dateFormatComboBox.SelectedItem?.ToString() ?? "dd.MM.yyyy";
        _settings.NumberFormat = _numberFormatComboBox.SelectedItem?.ToString() ?? "1,234.56";
    }

    private async void ImportSettings(string filePath)
    {
        try
        {
            var settings = await _configurationService.ImportSettingsAsync(filePath);
            _settings = settings;
            PopulateControls();
            _hasChanges = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import settings: {FilePath}", filePath);
            MessageBox.Show($"Failed to import settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void ExportSettings(string filePath)
    {
        try
        {
            UpdateSettingsFromControls();
            await _configurationService.ExportSettingsAsync(_settings, filePath);
            MessageBox.Show("Settings exported successfully", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export settings: {FilePath}", filePath);
            MessageBox.Show($"Failed to export settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

public class ApplicationSettings
{
    public string ApplicationName { get; set; } = "Invoice Reader";
    public string Version { get; set; } = "1.0.0";
    public bool AutoSave { get; set; } = true;
    public bool CreateBackup { get; set; } = true;
    public int AutoSaveInterval { get; set; } = 5;
    public string Language { get; set; } = "English";
    public string Theme { get; set; } = "Light";

    public string DatabaseConnectionString { get; set; } = "";
    public string DatabaseName { get; set; } = "InvoiceReader";
    public string DatabaseServer { get; set; } = "localhost";
    public string DatabaseUsername { get; set; } = "";
    public string DatabasePassword { get; set; } = "";
    public bool DatabaseIntegratedSecurity { get; set; } = true;

    public string DataPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InvoiceReader", "Data");
    public string ModelsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InvoiceReader", "Models");
    public string LogsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InvoiceReader", "Logs");
    public string TempPath { get; set; } = Path.GetTempPath();

    public string DefaultAlgorithm { get; set; } = "SdcaMaximumEntropy";
    public float DefaultConfidenceThreshold { get; set; } = 0.7f;
    public bool UseDataAugmentation { get; set; } = false;
    public bool UseCrossValidation { get; set; } = true;
    public int CrossValidationFolds { get; set; } = 5;
    public string ModelVersion { get; set; } = "1.0";

    public bool ShowTooltips { get; set; } = true;
    public bool ShowStatusBar { get; set; } = true;
    public bool ShowProgressBar { get; set; } = true;
    public int GridRowHeight { get; set; } = 25;
    public string DateFormat { get; set; } = "dd.MM.yyyy";
    public string NumberFormat { get; set; } = "1,234.56";
}
```

## Wichtige Hinweise

- SettingsForm mit Tab-basierter UI
- General, Database, Paths, ML, UI Tabs
- Database Connection Testing
- Path Browsing für alle Pfade
- Import/Export von Settings
- Change Tracking für unsaved changes
- Validation für alle Einstellungen
- Error Handling für alle Operationen
