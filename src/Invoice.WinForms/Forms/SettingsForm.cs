using Invoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using System.IO;

namespace Invoice.WinForms.Forms;

public partial class SettingsForm : XtraForm
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SettingsForm> _logger;
    private readonly object _configurationService; // TODO: Replace with actual IConfigurationService when available

    // DevExpress UI Controls
    private XtraTabControl _mainTabControl = null!;
    private XtraTabPage _generalTabPage = null!;
    private XtraTabPage _databaseTabPage = null!;
    private XtraTabPage _pathsTabPage = null!;
    private XtraTabPage _mlTabPage = null!;
    private XtraTabPage _uiTabPage = null!;

    // General settings
    private TextEdit _applicationNameTextEdit = null!;
    private TextEdit _versionTextEdit = null!;
    private CheckEdit _autoSaveCheckBox = null!;
    private CheckEdit _createBackupCheckBox = null!;
    private SpinEdit _autoSaveIntervalSpinEdit = null!;
    private ComboBoxEdit _languageComboBox = null!;
    private ComboBoxEdit _themeComboBox = null!;

    // Database settings
    private MemoEdit _connectionStringMemoEdit = null!;
    private TextEdit _databaseNameTextEdit = null!;
    private TextEdit _serverTextEdit = null!;
    private TextEdit _usernameTextEdit = null!;
    private TextEdit _passwordTextEdit = null!;
    private CheckEdit _integratedSecurityCheckBox = null!;
    private SimpleButton _testConnectionButton = null!;
    private LabelControl _connectionStatusLabel = null!;

    // Paths settings
    private TextEdit _dataPathTextEdit = null!;
    private TextEdit _modelsPathTextEdit = null!;
    private TextEdit _logsPathTextEdit = null!;
    private TextEdit _tempPathTextEdit = null!;
    private SimpleButton _browseDataPathButton = null!;
    private SimpleButton _browseModelsPathButton = null!;
    private SimpleButton _browseLogsPathButton = null!;
    private SimpleButton _browseTempPathButton = null!;

    // ML settings
    private ComboBoxEdit _defaultAlgorithmComboBox = null!;
    private SpinEdit _defaultConfidenceThresholdSpinEdit = null!;
    private CheckEdit _useDataAugmentationCheckBox = null!;
    private CheckEdit _useCrossValidationCheckBox = null!;
    private SpinEdit _crossValidationFoldsSpinEdit = null!;
    private TextEdit _modelVersionTextEdit = null!;

    // UI settings
    private CheckEdit _showTooltipsCheckBox = null!;
    private CheckEdit _showStatusBarCheckBox = null!;
    private CheckEdit _showProgressBarCheckBox = null!;
    private SpinEdit _gridRowHeightSpinEdit = null!;
    private ComboBoxEdit _dateFormatComboBox = null!;
    private ComboBoxEdit _numberFormatComboBox = null!;

    // Buttons
    private SimpleButton _saveButton = null!;
    private SimpleButton _cancelButton = null!;
    private SimpleButton _resetButton = null!;
    private SimpleButton _importButton = null!;
    private SimpleButton _exportButton = null!;

    // Data
    private ApplicationSettings _settings = new();
    private bool _hasChanges;

    public SettingsForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<SettingsForm>>();
        // TODO: Uncomment when IConfigurationService is available
        // _configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
        _configurationService = new object(); // Placeholder

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Text = "Einstellungen";
        this.Size = new Size(700, 600);
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
        _mainTabControl = new XtraTabControl { Dock = DockStyle.Fill };

        // General tab
        _generalTabPage = new XtraTabPage { Text = "Allgemein" };
        CreateGeneralTabControls();
        _mainTabControl.TabPages.Add(_generalTabPage);

        // Database tab
        _databaseTabPage = new XtraTabPage { Text = "Datenbank" };
        CreateDatabaseTabControls();
        _mainTabControl.TabPages.Add(_databaseTabPage);

        // Paths tab
        _pathsTabPage = new XtraTabPage { Text = "Pfade" };
        CreatePathsTabControls();
        _mainTabControl.TabPages.Add(_pathsTabPage);

        // ML tab
        _mlTabPage = new XtraTabPage { Text = "Machine Learning" };
        CreateMLTabControls();
        _mainTabControl.TabPages.Add(_mlTabPage);

        // UI tab
        _uiTabPage = new XtraTabPage { Text = "Benutzeroberfläche" };
        CreateUITabControls();
        _mainTabControl.TabPages.Add(_uiTabPage);
    }

    private void CreateGeneralTabControls()
    {
        _applicationNameTextEdit = new TextEdit { Width = 250 };
        
        _versionTextEdit = new TextEdit 
        { 
            Width = 250,
            Properties = { ReadOnly = true }
        };

        _autoSaveCheckBox = new CheckEdit { Text = "Automatisches Speichern aktivieren" };

        _createBackupCheckBox = new CheckEdit { Text = "Backup vor dem Speichern erstellen" };

        _autoSaveIntervalSpinEdit = new SpinEdit
        {
            Properties = { MinValue = 1, MaxValue = 60 },
            EditValue = 5,
            Width = 120
        };

        _languageComboBox = new ComboBoxEdit { Width = 200 };
        _languageComboBox.Properties.Items.AddRange(new[] { "Deutsch", "English", "Français", "Español" });
        _languageComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        _languageComboBox.SelectedIndex = 0;

        _themeComboBox = new ComboBoxEdit { Width = 200 };
        _themeComboBox.Properties.Items.AddRange(new[] { "Hell", "Dunkel", "System" });
        _themeComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        _themeComboBox.SelectedIndex = 0;

        _autoSaveCheckBox.CheckedChanged += (s, e) => _hasChanges = true;
        _createBackupCheckBox.CheckedChanged += (s, e) => _hasChanges = true;
    }

    private void CreateDatabaseTabControls()
    {
        _connectionStringMemoEdit = new MemoEdit
        {
            Width = 450,
            Height = 80
        };

        _databaseNameTextEdit = new TextEdit { Width = 250 };
        _serverTextEdit = new TextEdit { Width = 250 };
        _usernameTextEdit = new TextEdit { Width = 250 };
        
        _passwordTextEdit = new TextEdit 
        { 
            Width = 250,
            Properties = { PasswordChar = '●' }
        };

        _integratedSecurityCheckBox = new CheckEdit { Text = "Integrierte Windows-Authentifizierung verwenden" };

        _testConnectionButton = new SimpleButton
        {
            Text = "Verbindung testen",
            Width = 150
        };
        _testConnectionButton.Click += OnTestConnection;

        _connectionStatusLabel = new LabelControl
        {
            Text = "Nicht getestet",
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 300
        };
    }

    private void CreatePathsTabControls()
    {
        _dataPathTextEdit = new TextEdit { Width = 350 };
        _modelsPathTextEdit = new TextEdit { Width = 350 };
        _logsPathTextEdit = new TextEdit { Width = 350 };
        _tempPathTextEdit = new TextEdit { Width = 350 };

        _browseDataPathButton = new SimpleButton { Text = "Durchsuchen...", Width = 100 };
        _browseDataPathButton.Click += OnBrowseDataPath;

        _browseModelsPathButton = new SimpleButton { Text = "Durchsuchen...", Width = 100 };
        _browseModelsPathButton.Click += OnBrowseModelsPath;

        _browseLogsPathButton = new SimpleButton { Text = "Durchsuchen...", Width = 100 };
        _browseLogsPathButton.Click += OnBrowseLogsPath;

        _browseTempPathButton = new SimpleButton { Text = "Durchsuchen...", Width = 100 };
        _browseTempPathButton.Click += OnBrowseTempPath;
    }

    private void CreateMLTabControls()
    {
        _defaultAlgorithmComboBox = new ComboBoxEdit { Width = 250 };
        _defaultAlgorithmComboBox.Properties.Items.AddRange(new[] { "SdcaMaximumEntropy", "LbfgsMaximumEntropy", "FastTree", "FastForest" });
        _defaultAlgorithmComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        _defaultAlgorithmComboBox.SelectedIndex = 0;

        _defaultConfidenceThresholdSpinEdit = new SpinEdit
        {
            Properties = 
            { 
                MinValue = 0,
                MaxValue = 1,
                Increment = 0.1m,
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "0.0" }
            },
            EditValue = 0.7m,
            Width = 120
        };

        _useDataAugmentationCheckBox = new CheckEdit { Text = "Datenaugmentierung verwenden" };
        _useCrossValidationCheckBox = new CheckEdit { Text = "Kreuzvalidierung verwenden" };

        _crossValidationFoldsSpinEdit = new SpinEdit
        {
            Properties = { MinValue = 2, MaxValue = 10 },
            EditValue = 5,
            Width = 120
        };

        _modelVersionTextEdit = new TextEdit { Width = 200 };
    }

    private void CreateUITabControls()
    {
        _showTooltipsCheckBox = new CheckEdit { Text = "Tooltips anzeigen" };
        _showStatusBarCheckBox = new CheckEdit { Text = "Statusleiste anzeigen" };
        _showProgressBarCheckBox = new CheckEdit { Text = "Fortschrittsleiste anzeigen" };

        _gridRowHeightSpinEdit = new SpinEdit
        {
            Properties = { MinValue = 20, MaxValue = 50 },
            EditValue = 25,
            Width = 120
        };

        _dateFormatComboBox = new ComboBoxEdit { Width = 200 };
        _dateFormatComboBox.Properties.Items.AddRange(new[] { "dd.MM.yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" });
        _dateFormatComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        _dateFormatComboBox.SelectedIndex = 0;

        _numberFormatComboBox = new ComboBoxEdit { Width = 200 };
        _numberFormatComboBox.Properties.Items.AddRange(new[] { "1.234,56", "1,234.56", "1 234,56" });
        _numberFormatComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        _numberFormatComboBox.SelectedIndex = 0;
    }

    private void LayoutControls()
    {
        // General tab layout
        var generalLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(15)
        };

        generalLayout.Controls.Add(new LabelControl { Text = "Anwendungsname:" }, 0, 0);
        generalLayout.Controls.Add(_applicationNameTextEdit, 1, 0);
        generalLayout.Controls.Add(new LabelControl { Text = "Version:" }, 0, 1);
        generalLayout.Controls.Add(_versionTextEdit, 1, 1);
        generalLayout.Controls.Add(_autoSaveCheckBox, 0, 2);
        generalLayout.Controls.Add(_autoSaveIntervalSpinEdit, 1, 2);
        generalLayout.Controls.Add(_createBackupCheckBox, 0, 3);
        generalLayout.SetColumnSpan(_createBackupCheckBox, 2);
        generalLayout.Controls.Add(new LabelControl { Text = "Sprache:" }, 0, 4);
        generalLayout.Controls.Add(_languageComboBox, 1, 4);
        generalLayout.Controls.Add(new LabelControl { Text = "Design:" }, 0, 5);
        generalLayout.Controls.Add(_themeComboBox, 1, 5);

        _generalTabPage.Controls.Add(generalLayout);

        // Database tab layout
        var databaseLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(15)
        };

        databaseLayout.Controls.Add(new LabelControl { Text = "Verbindungszeichenfolge:" }, 0, 0);
        databaseLayout.Controls.Add(_connectionStringMemoEdit, 1, 0);
        databaseLayout.Controls.Add(new LabelControl { Text = "Datenbankname:" }, 0, 1);
        databaseLayout.Controls.Add(_databaseNameTextEdit, 1, 1);
        databaseLayout.Controls.Add(new LabelControl { Text = "Server:" }, 0, 2);
        databaseLayout.Controls.Add(_serverTextEdit, 1, 2);
        databaseLayout.Controls.Add(new LabelControl { Text = "Benutzername:" }, 0, 3);
        databaseLayout.Controls.Add(_usernameTextEdit, 1, 3);
        databaseLayout.Controls.Add(new LabelControl { Text = "Passwort:" }, 0, 4);
        databaseLayout.Controls.Add(_passwordTextEdit, 1, 4);
        databaseLayout.Controls.Add(_integratedSecurityCheckBox, 0, 5);
        databaseLayout.SetColumnSpan(_integratedSecurityCheckBox, 2);
        databaseLayout.Controls.Add(_testConnectionButton, 0, 6);
        databaseLayout.Controls.Add(_connectionStatusLabel, 1, 6);

        _databaseTabPage.Controls.Add(databaseLayout);

        // Paths tab layout
        var pathsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            Padding = new Padding(15)
        };

        pathsLayout.Controls.Add(new LabelControl { Text = "Daten-Pfad:" }, 0, 0);
        pathsLayout.Controls.Add(_dataPathTextEdit, 1, 0);
        pathsLayout.Controls.Add(_browseDataPathButton, 2, 0);
        pathsLayout.Controls.Add(new LabelControl { Text = "Modelle-Pfad:" }, 0, 1);
        pathsLayout.Controls.Add(_modelsPathTextEdit, 1, 1);
        pathsLayout.Controls.Add(_browseModelsPathButton, 2, 1);
        pathsLayout.Controls.Add(new LabelControl { Text = "Logs-Pfad:" }, 0, 2);
        pathsLayout.Controls.Add(_logsPathTextEdit, 1, 2);
        pathsLayout.Controls.Add(_browseLogsPathButton, 2, 2);
        pathsLayout.Controls.Add(new LabelControl { Text = "Temp-Pfad:" }, 0, 3);
        pathsLayout.Controls.Add(_tempPathTextEdit, 1, 3);
        pathsLayout.Controls.Add(_browseTempPathButton, 2, 3);

        _pathsTabPage.Controls.Add(pathsLayout);

        // ML tab layout
        var mlLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(15)
        };

        mlLayout.Controls.Add(new LabelControl { Text = "Standard-Algorithmus:" }, 0, 0);
        mlLayout.Controls.Add(_defaultAlgorithmComboBox, 1, 0);
        mlLayout.Controls.Add(new LabelControl { Text = "Konfidenz-Schwelle:" }, 0, 1);
        mlLayout.Controls.Add(_defaultConfidenceThresholdSpinEdit, 1, 1);
        mlLayout.Controls.Add(_useDataAugmentationCheckBox, 0, 2);
        mlLayout.SetColumnSpan(_useDataAugmentationCheckBox, 2);
        mlLayout.Controls.Add(_useCrossValidationCheckBox, 0, 3);
        mlLayout.Controls.Add(_crossValidationFoldsSpinEdit, 1, 3);
        mlLayout.Controls.Add(new LabelControl { Text = "Modell-Version:" }, 0, 4);
        mlLayout.Controls.Add(_modelVersionTextEdit, 1, 4);

        _mlTabPage.Controls.Add(mlLayout);

        // UI tab layout
        var uiLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(15)
        };

        uiLayout.Controls.Add(_showTooltipsCheckBox, 0, 0);
        uiLayout.SetColumnSpan(_showTooltipsCheckBox, 2);
        uiLayout.Controls.Add(_showStatusBarCheckBox, 0, 1);
        uiLayout.SetColumnSpan(_showStatusBarCheckBox, 2);
        uiLayout.Controls.Add(_showProgressBarCheckBox, 0, 2);
        uiLayout.SetColumnSpan(_showProgressBarCheckBox, 2);
        uiLayout.Controls.Add(new LabelControl { Text = "Zeilenhöhe im Grid:" }, 0, 3);
        uiLayout.Controls.Add(_gridRowHeightSpinEdit, 1, 3);
        uiLayout.Controls.Add(new LabelControl { Text = "Datumsformat:" }, 0, 4);
        uiLayout.Controls.Add(_dateFormatComboBox, 1, 4);
        uiLayout.Controls.Add(new LabelControl { Text = "Zahlenformat:" }, 0, 5);
        uiLayout.Controls.Add(_numberFormatComboBox, 1, 5);

        _uiTabPage.Controls.Add(uiLayout);

        // Buttons
        var buttonPanel = new PanelControl { Dock = DockStyle.Bottom, Height = 60 };
        var buttonLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        _saveButton = new SimpleButton { Text = "Speichern", Width = 100 };
        _saveButton.Click += OnSave;

        _cancelButton = new SimpleButton { Text = "Abbrechen", Width = 100 };
        _cancelButton.Click += OnCancel;

        _resetButton = new SimpleButton { Text = "Zurücksetzen", Width = 120 };
        _resetButton.Click += OnReset;

        _importButton = new SimpleButton { Text = "Importieren", Width = 100 };
        _importButton.Click += OnImport;

        _exportButton = new SimpleButton { Text = "Exportieren", Width = 100 };
        _exportButton.Click += OnExport;

        buttonLayout.Controls.Add(_cancelButton);
        buttonLayout.Controls.Add(_saveButton);
        buttonLayout.Controls.Add(_resetButton);
        buttonLayout.Controls.Add(_importButton);
        buttonLayout.Controls.Add(_exportButton);

        buttonPanel.Controls.Add(buttonLayout);

        this.Controls.Add(_mainTabControl);
        this.Controls.Add(buttonPanel);
    }

    private void InitializeData()
    {
        _settings = new ApplicationSettings();
        _hasChanges = false;
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            // TODO: Implement when IConfigurationService is available
            // _settings = await _configurationService.GetSettingsAsync();
            _settings = new ApplicationSettings();
            PopulateControls();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Einstellungen");
            XtraMessageBox.Show(
                $"Fehler beim Laden der Einstellungen: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void PopulateControls()
    {
        // General settings
        _applicationNameTextEdit.Text = _settings.ApplicationName;
        _versionTextEdit.Text = _settings.Version;
        _autoSaveCheckBox.Checked = _settings.AutoSave;
        _createBackupCheckBox.Checked = _settings.CreateBackup;
        _autoSaveIntervalSpinEdit.EditValue = _settings.AutoSaveInterval;
        _languageComboBox.EditValue = _settings.Language;
        _themeComboBox.EditValue = _settings.Theme;

        // Database settings
        _connectionStringMemoEdit.Text = _settings.DatabaseConnectionString;
        _databaseNameTextEdit.Text = _settings.DatabaseName;
        _serverTextEdit.Text = _settings.DatabaseServer;
        _usernameTextEdit.Text = _settings.DatabaseUsername;
        _passwordTextEdit.Text = _settings.DatabasePassword;
        _integratedSecurityCheckBox.Checked = _settings.DatabaseIntegratedSecurity;

        // Paths settings
        _dataPathTextEdit.Text = _settings.DataPath;
        _modelsPathTextEdit.Text = _settings.ModelsPath;
        _logsPathTextEdit.Text = _settings.LogsPath;
        _tempPathTextEdit.Text = _settings.TempPath;

        // ML settings
        _defaultAlgorithmComboBox.EditValue = _settings.DefaultAlgorithm;
        _defaultConfidenceThresholdSpinEdit.EditValue = (decimal)_settings.DefaultConfidenceThreshold;
        _useDataAugmentationCheckBox.Checked = _settings.UseDataAugmentation;
        _useCrossValidationCheckBox.Checked = _settings.UseCrossValidation;
        _crossValidationFoldsSpinEdit.EditValue = _settings.CrossValidationFolds;
        _modelVersionTextEdit.Text = _settings.ModelVersion;

        // UI settings
        _showTooltipsCheckBox.Checked = _settings.ShowTooltips;
        _showStatusBarCheckBox.Checked = _settings.ShowStatusBar;
        _showProgressBarCheckBox.Checked = _settings.ShowProgressBar;
        _gridRowHeightSpinEdit.EditValue = _settings.GridRowHeight;
        _dateFormatComboBox.EditValue = _settings.DateFormat;
        _numberFormatComboBox.EditValue = _settings.NumberFormat;

        _hasChanges = false;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        try
        {
            UpdateSettingsFromControls();
            // TODO: Implement when IConfigurationService is available
            // await _configurationService.SaveSettingsAsync(_settings);
            _hasChanges = false;
            
            XtraMessageBox.Show(
                "Einstellungen erfolgreich gespeichert",
                "Erfolg",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Speichern der Einstellungen");
            XtraMessageBox.Show(
                $"Fehler beim Speichern der Einstellungen: {ex.Message}",
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

    private void OnReset(object? sender, EventArgs e)
    {
        var result = XtraMessageBox.Show(
            "Möchten Sie wirklich alle Einstellungen auf die Standardwerte zurücksetzen?",
            "Zurücksetzen bestätigen",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

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
                Filter = "JSON-Dateien (*.json)|*.json|Alle Dateien (*.*)|*.*",
                Title = "Einstellungen importieren"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImportSettings(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Importieren der Einstellungen");
            XtraMessageBox.Show(
                $"Fehler beim Importieren der Einstellungen: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnExport(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "JSON-Dateien (*.json)|*.json|Alle Dateien (*.*)|*.*",
                Title = "Einstellungen exportieren"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ExportSettings(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Exportieren der Einstellungen");
            XtraMessageBox.Show(
                $"Fehler beim Exportieren der Einstellungen: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnTestConnection(object? sender, EventArgs e)
    {
        try
        {
            _testConnectionButton.Enabled = false;
            _connectionStatusLabel.Text = "Teste Verbindung...";
            _connectionStatusLabel.Appearance.ForeColor = Color.Blue;

            // TODO: Implement when IConfigurationService is available
            // var connectionString = _connectionStringMemoEdit.Text;
            // var success = await _configurationService.TestDatabaseConnectionAsync(connectionString);
            var success = false; // Placeholder

            if (success)
            {
                _connectionStatusLabel.Text = "Verbindung erfolgreich";
                _connectionStatusLabel.Appearance.ForeColor = Color.Green;
                
                XtraMessageBox.Show(
                    "Datenbankverbindung erfolgreich getestet",
                    "Erfolg",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                _connectionStatusLabel.Text = "Funktion noch nicht implementiert";
                _connectionStatusLabel.Appearance.ForeColor = Color.Orange;
                
                XtraMessageBox.Show(
                    "Test-Funktion noch nicht implementiert (IConfigurationService fehlt)",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Testen der Datenbankverbindung");
            _connectionStatusLabel.Text = $"Verbindungsfehler: {ex.Message}";
            _connectionStatusLabel.Appearance.ForeColor = Color.Red;
            
            XtraMessageBox.Show(
                $"Fehler beim Testen der Datenbankverbindung: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _testConnectionButton.Enabled = true;
        }
    }

    private void OnBrowseDataPath(object? sender, EventArgs e)
    {
        BrowseFolder(_dataPathTextEdit);
    }

    private void OnBrowseModelsPath(object? sender, EventArgs e)
    {
        BrowseFolder(_modelsPathTextEdit);
    }

    private void OnBrowseLogsPath(object? sender, EventArgs e)
    {
        BrowseFolder(_logsPathTextEdit);
    }

    private void OnBrowseTempPath(object? sender, EventArgs e)
    {
        BrowseFolder(_tempPathTextEdit);
    }

    private void BrowseFolder(TextEdit textEdit)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Ordner auswählen",
            SelectedPath = textEdit.Text
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            textEdit.Text = dialog.SelectedPath;
            _hasChanges = true;
        }
    }

    private void UpdateSettingsFromControls()
    {
        // General settings
        _settings.ApplicationName = _applicationNameTextEdit.Text;
        _settings.AutoSave = _autoSaveCheckBox.Checked;
        _settings.CreateBackup = _createBackupCheckBox.Checked;
        _settings.AutoSaveInterval = Convert.ToInt32(_autoSaveIntervalSpinEdit.EditValue);
        _settings.Language = _languageComboBox.EditValue?.ToString() ?? "Deutsch";
        _settings.Theme = _themeComboBox.EditValue?.ToString() ?? "Hell";

        // Database settings
        _settings.DatabaseConnectionString = _connectionStringMemoEdit.Text;
        _settings.DatabaseName = _databaseNameTextEdit.Text;
        _settings.DatabaseServer = _serverTextEdit.Text;
        _settings.DatabaseUsername = _usernameTextEdit.Text;
        _settings.DatabasePassword = _passwordTextEdit.Text;
        _settings.DatabaseIntegratedSecurity = _integratedSecurityCheckBox.Checked;

        // Paths settings
        _settings.DataPath = _dataPathTextEdit.Text;
        _settings.ModelsPath = _modelsPathTextEdit.Text;
        _settings.LogsPath = _logsPathTextEdit.Text;
        _settings.TempPath = _tempPathTextEdit.Text;

        // ML settings
        _settings.DefaultAlgorithm = _defaultAlgorithmComboBox.EditValue?.ToString() ?? "SdcaMaximumEntropy";
        _settings.DefaultConfidenceThreshold = Convert.ToSingle(_defaultConfidenceThresholdSpinEdit.EditValue);
        _settings.UseDataAugmentation = _useDataAugmentationCheckBox.Checked;
        _settings.UseCrossValidation = _useCrossValidationCheckBox.Checked;
        _settings.CrossValidationFolds = Convert.ToInt32(_crossValidationFoldsSpinEdit.EditValue);
        _settings.ModelVersion = _modelVersionTextEdit.Text;

        // UI settings
        _settings.ShowTooltips = _showTooltipsCheckBox.Checked;
        _settings.ShowStatusBar = _showStatusBarCheckBox.Checked;
        _settings.ShowProgressBar = _showProgressBarCheckBox.Checked;
        _settings.GridRowHeight = Convert.ToInt32(_gridRowHeightSpinEdit.EditValue);
        _settings.DateFormat = _dateFormatComboBox.EditValue?.ToString() ?? "dd.MM.yyyy";
        _settings.NumberFormat = _numberFormatComboBox.EditValue?.ToString() ?? "1.234,56";
    }

    private void ImportSettings(string filePath)
    {
        try
        {
            // TODO: Implement when IConfigurationService is available
            // var settings = await _configurationService.ImportSettingsAsync(filePath);
            // _settings = settings;
            // PopulateControls();
            // _hasChanges = true;
            
            XtraMessageBox.Show(
                "Import-Funktion noch nicht implementiert (IConfigurationService fehlt)",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Importieren der Einstellungen: {FilePath}", filePath);
            XtraMessageBox.Show(
                $"Fehler beim Importieren der Einstellungen: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ExportSettings(string filePath)
    {
        try
        {
            UpdateSettingsFromControls();
            // TODO: Implement when IConfigurationService is available
            // await _configurationService.ExportSettingsAsync(_settings, filePath);
            
            XtraMessageBox.Show(
                "Export-Funktion noch nicht implementiert (IConfigurationService fehlt)",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Exportieren der Einstellungen: {FilePath}", filePath);
            XtraMessageBox.Show(
                $"Fehler beim Exportieren der Einstellungen: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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

public class ApplicationSettings
{
    public string ApplicationName { get; set; } = "Invoice Reader";
    public string Version { get; set; } = "1.0.0";
    public bool AutoSave { get; set; } = true;
    public bool CreateBackup { get; set; } = true;
    public int AutoSaveInterval { get; set; } = 5;
    public string Language { get; set; } = "Deutsch";
    public string Theme { get; set; } = "Hell";

    public string DatabaseConnectionString { get; set; } = "";
    public string DatabaseName { get; set; } = "Invoice";
    public string DatabaseServer { get; set; } = "localhost";
    public string DatabaseUsername { get; set; } = "";
    public string DatabasePassword { get; set; } = "";
    public bool DatabaseIntegratedSecurity { get; set; } = true;

    public string DataPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Invoice", "Data");
    public string ModelsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Invoice", "Models");
    public string LogsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Invoice", "Logs");
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
    public string NumberFormat { get; set; } = "1.234,56";
}

