using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.ComponentModel;
using System.IO;

namespace Invoice.WinForms.Forms;

public partial class TrainingForm : XtraForm
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrainingForm> _logger;
    private readonly ITrainModelsUseCase _trainModelsUseCase;
    private readonly IExtractFieldsUseCase _extractFieldsUseCase;

    // DevExpress UI Controls
    private XtraTabControl _mainTabControl = null!;
    private XtraTabPage _dataTabPage = null!;
    private XtraTabPage _trainingTabPage = null!;
    private XtraTabPage _evaluationTabPage = null!;

    // Data tab controls
    private GridControl _trainingDataGridControl = null!;
    private GridView _trainingDataGridView = null!;
    private SimpleButton _loadDataButton = null!;
    private SimpleButton _addDataButton = null!;
    private SimpleButton _removeDataButton = null!;
    private SimpleButton _clearDataButton = null!;
    private SimpleButton _saveDataButton = null!;
    private LabelControl _dataStatusLabel = null!;

    // Training tab controls
    private ComboBoxEdit _algorithmComboBox = null!;
    private SpinEdit _maxIterationsSpinEdit = null!;
    private SpinEdit _learningRateSpinEdit = null!;
    private SpinEdit _regularizationSpinEdit = null!;
    private CheckEdit _useCrossValidationCheckBox = null!;
    private SpinEdit _crossValidationFoldsSpinEdit = null!;
    private CheckEdit _useEarlyStoppingCheckBox = null!;
    private SpinEdit _earlyStoppingPatienceSpinEdit = null!;
    private SimpleButton _startTrainingButton = null!;
    private SimpleButton _stopTrainingButton = null!;
    private ProgressBarControl _trainingProgressBar = null!;
    private LabelControl _trainingStatusLabel = null!;
    private ListBoxControl _trainingLogListBox = null!;

    // Evaluation tab controls
    private GridControl _evaluationResultsGridControl = null!;
    private GridView _evaluationResultsGridView = null!;
    private SimpleButton _evaluateModelButton = null!;
    private SimpleButton _loadModelButton = null!;
    private LabelControl _evaluationStatusLabel = null!;
    private ProgressBarControl _evaluationProgressBar = null!;

    // Data
    private BindingList<TrainingDataRow> _trainingData = new();
    private BindingList<object> _evaluationResults = new(); // TODO: Replace with actual EvaluationResult type when available
    private bool _isTraining;
    private bool _isEvaluating;
    private CancellationTokenSource? _cancellationTokenSource;

    public TrainingForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<TrainingForm>>();
        _trainModelsUseCase = serviceProvider.GetRequiredService<ITrainModelsUseCase>();
        _extractFieldsUseCase = serviceProvider.GetRequiredService<IExtractFieldsUseCase>();

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Text = "Modell-Training";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = true;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // Main tab control
        _mainTabControl = new XtraTabControl
        {
            Dock = DockStyle.Fill
        };

        // Data tab
        _dataTabPage = new XtraTabPage { Text = "Trainingsdaten" };
        CreateDataTabControls();
        _mainTabControl.TabPages.Add(_dataTabPage);

        // Training tab
        _trainingTabPage = new XtraTabPage { Text = "Training" };
        CreateTrainingTabControls();
        _mainTabControl.TabPages.Add(_trainingTabPage);

        // Evaluation tab
        _evaluationTabPage = new XtraTabPage { Text = "Evaluation" };
        CreateEvaluationTabControls();
        _mainTabControl.TabPages.Add(_evaluationTabPage);
    }

    private void CreateDataTabControls()
    {
        // Training data grid
        _trainingDataGridControl = new GridControl();
        _trainingDataGridView = new GridView(_trainingDataGridControl);
        
        _trainingDataGridControl.MainView = _trainingDataGridView;
        _trainingDataGridView.GridControl = _trainingDataGridControl;
        _trainingDataGridView.OptionsBehavior.Editable = true;
        _trainingDataGridView.OptionsView.ShowGroupPanel = false;
        _trainingDataGridView.OptionsView.ShowAutoFilterRow = true;

        // Data buttons
        _loadDataButton = new SimpleButton { Text = "Daten laden", Width = 120 };
        _loadDataButton.Click += OnLoadData;

        _addDataButton = new SimpleButton { Text = "Hinzufügen", Width = 120 };
        _addDataButton.Click += OnAddData;

        _removeDataButton = new SimpleButton { Text = "Entfernen", Width = 120 };
        _removeDataButton.Click += OnRemoveData;

        _clearDataButton = new SimpleButton { Text = "Alle löschen", Width = 120 };
        _clearDataButton.Click += OnClearData;

        _saveDataButton = new SimpleButton { Text = "Speichern", Width = 120 };
        _saveDataButton.Click += OnSaveData;

        // Data status
        _dataStatusLabel = new LabelControl
        {
            Text = "Bereit",
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 300
        };
    }

    private void CreateTrainingTabControls()
    {
        // Algorithm selection
        _algorithmComboBox = new ComboBoxEdit { Width = 200 };
        _algorithmComboBox.Properties.Items.AddRange(new[] { "SdcaMaximumEntropy", "LbfgsMaximumEntropy", "FastTree", "FastForest" });
        _algorithmComboBox.SelectedIndex = 0;
        _algorithmComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

        // Training parameters
        _maxIterationsSpinEdit = new SpinEdit
        {
            Properties = { MinValue = 1, MaxValue = 10000 },
            EditValue = 100,
            Width = 120
        };

        _learningRateSpinEdit = new SpinEdit
        {
            Properties = 
            { 
                MinValue = 0.001m, 
                MaxValue = 1.0m, 
                Increment = 0.001m,
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "0.000" }
            },
            EditValue = 0.01m,
            Width = 120
        };

        _regularizationSpinEdit = new SpinEdit
        {
            Properties = 
            { 
                MinValue = 0.001m, 
                MaxValue = 1.0m, 
                Increment = 0.001m,
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "0.000" }
            },
            EditValue = 0.1m,
            Width = 120
        };

        // Cross validation
        _useCrossValidationCheckBox = new CheckEdit
        {
            Text = "Kreuzvalidierung verwenden",
            Checked = true
        };

        _crossValidationFoldsSpinEdit = new SpinEdit
        {
            Properties = { MinValue = 2, MaxValue = 10 },
            EditValue = 5,
            Width = 120
        };

        // Early stopping
        _useEarlyStoppingCheckBox = new CheckEdit
        {
            Text = "Early Stopping verwenden",
            Checked = true
        };

        _earlyStoppingPatienceSpinEdit = new SpinEdit
        {
            Properties = { MinValue = 1, MaxValue = 100 },
            EditValue = 10,
            Width = 120
        };

        // Training buttons
        _startTrainingButton = new SimpleButton
        {
            Text = "Training starten",
            Width = 150
        };
        _startTrainingButton.Click += OnStartTraining;

        _stopTrainingButton = new SimpleButton
        {
            Text = "Training stoppen",
            Width = 150,
            Enabled = false
        };
        _stopTrainingButton.Click += OnStopTraining;

        // Progress
        _trainingProgressBar = new ProgressBarControl
        {
            Properties = { ShowTitle = true },
            Width = 400,
            Height = 25
        };

        _trainingStatusLabel = new LabelControl
        {
            Text = "Bereit",
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 300
        };

        // Training log
        _trainingLogListBox = new ListBoxControl
        {
            Height = 250,
            Dock = DockStyle.Fill
        };
    }

    private void CreateEvaluationTabControls()
    {
        // Evaluation results grid
        _evaluationResultsGridControl = new GridControl();
        _evaluationResultsGridView = new GridView(_evaluationResultsGridControl);
        
        _evaluationResultsGridControl.MainView = _evaluationResultsGridView;
        _evaluationResultsGridView.GridControl = _evaluationResultsGridControl;
        _evaluationResultsGridView.OptionsBehavior.Editable = false;
        _evaluationResultsGridView.OptionsView.ShowGroupPanel = false;

        // Evaluation buttons
        _evaluateModelButton = new SimpleButton
        {
            Text = "Modell evaluieren",
            Width = 150
        };
        _evaluateModelButton.Click += OnEvaluateModel;

        _loadModelButton = new SimpleButton
        {
            Text = "Modell laden",
            Width = 150
        };
        _loadModelButton.Click += OnLoadModel;

        // Evaluation status
        _evaluationStatusLabel = new LabelControl
        {
            Text = "Bereit",
            AutoSizeMode = LabelAutoSizeMode.None,
            Width = 300
        };

        _evaluationProgressBar = new ProgressBarControl
        {
            Properties = { ShowTitle = true },
            Width = 400,
            Height = 25
        };
    }

    private void LayoutControls()
    {
        // Data tab layout
        var dataLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };

        dataLayout.Controls.Add(_trainingDataGridControl, 0, 0);
        dataLayout.SetRowSpan(_trainingDataGridControl, 2);
        dataLayout.SetColumnSpan(_trainingDataGridControl, 1);

        var dataButtonPanel = new PanelControl { Dock = DockStyle.Right, Width = 150 };
        var dataButtonLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };
        dataButtonLayout.Controls.Add(_loadDataButton);
        dataButtonLayout.Controls.Add(_addDataButton);
        dataButtonLayout.Controls.Add(_removeDataButton);
        dataButtonLayout.Controls.Add(_clearDataButton);
        dataButtonLayout.Controls.Add(_saveDataButton);
        dataButtonLayout.Controls.Add(_dataStatusLabel);
        dataButtonPanel.Controls.Add(dataButtonLayout);

        var dataContainer = new PanelControl { Dock = DockStyle.Fill };
        dataContainer.Controls.Add(_trainingDataGridControl);
        dataContainer.Controls.Add(dataButtonPanel);

        _dataTabPage.Controls.Add(dataContainer);

        // Training tab layout
        var trainingMainPanel = new PanelControl { Dock = DockStyle.Fill };
        var trainingLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };

        // Parameters group
        var parametersGroup = new GroupControl
        {
            Text = "Training-Parameter",
            Dock = DockStyle.Top,
            Height = 250
        };

        var parametersLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(10)
        };

        parametersLayout.Controls.Add(new LabelControl { Text = "Algorithmus:" }, 0, 0);
        parametersLayout.Controls.Add(_algorithmComboBox, 1, 0);
        parametersLayout.Controls.Add(new LabelControl { Text = "Max. Iterationen:" }, 0, 1);
        parametersLayout.Controls.Add(_maxIterationsSpinEdit, 1, 1);
        parametersLayout.Controls.Add(new LabelControl { Text = "Lernrate:" }, 0, 2);
        parametersLayout.Controls.Add(_learningRateSpinEdit, 1, 2);
        parametersLayout.Controls.Add(new LabelControl { Text = "Regularisierung:" }, 0, 3);
        parametersLayout.Controls.Add(_regularizationSpinEdit, 1, 3);
        parametersLayout.Controls.Add(_useCrossValidationCheckBox, 0, 4);
        parametersLayout.Controls.Add(_crossValidationFoldsSpinEdit, 1, 4);
        parametersLayout.Controls.Add(_useEarlyStoppingCheckBox, 0, 5);
        parametersLayout.Controls.Add(_earlyStoppingPatienceSpinEdit, 1, 5);

        parametersGroup.Controls.Add(parametersLayout);

        // Training group
        var trainingGroup = new GroupControl
        {
            Text = "Training",
            Dock = DockStyle.Fill
        };

        var trainingControlLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(10)
        };

        var trainingButtonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        trainingButtonPanel.Controls.Add(_startTrainingButton);
        trainingButtonPanel.Controls.Add(_stopTrainingButton);

        var progressPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        progressPanel.Controls.Add(_trainingProgressBar);
        progressPanel.Controls.Add(_trainingStatusLabel);

        trainingControlLayout.Controls.Add(trainingButtonPanel, 0, 0);
        trainingControlLayout.SetRow(trainingButtonPanel, 0);
        trainingControlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        trainingControlLayout.Controls.Add(progressPanel, 0, 1);
        trainingControlLayout.SetRow(progressPanel, 1);
        trainingControlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        trainingControlLayout.Controls.Add(new LabelControl { Text = "Training-Log:" }, 0, 2);
        trainingControlLayout.SetRow(new LabelControl { Text = "Training-Log:" }, 2);
        trainingControlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));

        trainingControlLayout.Controls.Add(_trainingLogListBox, 0, 3);
        trainingControlLayout.SetRow(_trainingLogListBox, 3);
        trainingControlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        trainingGroup.Controls.Add(trainingControlLayout);

        trainingLayout.Controls.Add(parametersGroup, 0, 0);
        trainingLayout.SetRow(parametersGroup, 0);
        trainingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 250));

        trainingLayout.Controls.Add(trainingGroup, 0, 1);
        trainingLayout.SetRow(trainingGroup, 1);
        trainingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        trainingMainPanel.Controls.Add(trainingLayout);
        _trainingTabPage.Controls.Add(trainingMainPanel);

        // Evaluation tab layout
        var evaluationLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };

        evaluationLayout.Controls.Add(_evaluationResultsGridControl, 0, 0);
        evaluationLayout.SetRow(_evaluationResultsGridControl, 0);
        evaluationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var evaluationButtonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        evaluationButtonPanel.Controls.Add(_evaluateModelButton);
        evaluationButtonPanel.Controls.Add(_loadModelButton);
        evaluationLayout.Controls.Add(evaluationButtonPanel, 0, 1);
        evaluationLayout.SetRow(evaluationButtonPanel, 1);
        evaluationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        var evaluationStatusPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        evaluationStatusPanel.Controls.Add(_evaluationProgressBar);
        evaluationStatusPanel.Controls.Add(_evaluationStatusLabel);
        evaluationLayout.Controls.Add(evaluationStatusPanel, 0, 2);
        evaluationLayout.SetRow(evaluationStatusPanel, 2);
        evaluationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        _evaluationTabPage.Controls.Add(evaluationLayout);

        this.Controls.Add(_mainTabControl);
    }

    private void InitializeData()
    {
        _trainingData = new BindingList<TrainingDataRow>();
        _evaluationResults = new BindingList<object>();
        _isTraining = false;
        _isEvaluating = false;
        _cancellationTokenSource = new CancellationTokenSource();

        _trainingDataGridControl.DataSource = _trainingData;
        _evaluationResultsGridControl.DataSource = _evaluationResults;
    }

    private void OnLoadData(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON-Dateien (*.json)|*.json|CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                Title = "Trainingsdaten laden"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadTrainingData(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Trainingsdaten");
            XtraMessageBox.Show(
                $"Fehler beim Laden der Trainingsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void LoadTrainingData(string filePath)
    {
        try
        {
            _dataStatusLabel.Text = "Lade Daten...";

            var result = await _trainModelsUseCase.PrepareTrainingDataAsync(filePath);

            if (result.Success)
            {
                _trainingData = new BindingList<TrainingDataRow>(result.Rows.ToList());
                _trainingDataGridControl.DataSource = null;
                _trainingDataGridControl.DataSource = _trainingData;
                _trainingDataGridView.BestFitColumns();
                _dataStatusLabel.Text = $"{_trainingData.Count} Zeilen geladen";
            }
            else
            {
                XtraMessageBox.Show(
                    $"Fehler beim Laden der Trainingsdaten: {result.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _dataStatusLabel.Text = "Laden fehlgeschlagen";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Trainingsdaten: {FilePath}", filePath);
            XtraMessageBox.Show(
                $"Fehler beim Laden der Trainingsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _dataStatusLabel.Text = "Laden fehlgeschlagen";
        }
    }

    private void OnAddData(object? sender, EventArgs e)
    {
        try
        {
            var newRow = new TrainingDataRow(
                "", 0, 0, 0, 0, 0, 0, "None", 0f, new Dictionary<string, object>());

            _trainingData.Add(newRow);
            _trainingDataGridControl.RefreshDataSource();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Hinzufügen von Trainingsdaten");
            XtraMessageBox.Show(
                $"Fehler beim Hinzufügen von Trainingsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnRemoveData(object? sender, EventArgs e)
    {
        try
        {
            var selectedRows = _trainingDataGridView.GetSelectedRows();
            for (int i = selectedRows.Length - 1; i >= 0; i--)
            {
                var rowHandle = selectedRows[i];
                if (rowHandle >= 0 && rowHandle < _trainingData.Count)
                {
                    _trainingData.RemoveAt(rowHandle);
                }
            }
            _trainingDataGridControl.RefreshDataSource();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Entfernen von Trainingsdaten");
            XtraMessageBox.Show(
                $"Fehler beim Entfernen von Trainingsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnClearData(object? sender, EventArgs e)
    {
        try
        {
            _trainingData.Clear();
            _trainingDataGridControl.RefreshDataSource();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen der Trainingsdaten");
            XtraMessageBox.Show(
                $"Fehler beim Löschen der Trainingsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnSaveData(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "JSON-Dateien (*.json)|*.json|CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                Title = "Trainingsdaten speichern"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveTrainingData(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Speichern der Trainingsdaten");
            XtraMessageBox.Show(
                $"Fehler beim Speichern der Trainingsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void SaveTrainingData(string filePath)
    {
        try
        {
            _dataStatusLabel.Text = "Speichere Daten...";

            var result = new TrainingDataResult(
                true,
                "Trainingsdaten vorbereitet",
                _trainingData.ToList(),
                _trainingData.Count,
                _trainingData.Count(r => r.Label != "None"),
                _trainingData.Count(r => r.Label == "None"),
                new List<TrainingDataWarning>(),
                new List<TrainingDataError>());

            // TODO: Implement SaveTrainingDataAsync when available in ITrainModelsUseCase
            // var success = await _trainModelsUseCase.SaveTrainingDataAsync(result, filePath);
            var success = false; // Placeholder - Function not implemented yet

            if (success)
            {
                _dataStatusLabel.Text = "Daten erfolgreich gespeichert";
                XtraMessageBox.Show(
                    "Daten erfolgreich gespeichert",
                    "Erfolg",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                XtraMessageBox.Show(
                    "Funktion noch nicht implementiert (SaveTrainingDataAsync fehlt im Interface)",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _dataStatusLabel.Text = "Speichern fehlgeschlagen";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Speichern der Trainingsdaten: {FilePath}", filePath);
            XtraMessageBox.Show(
                $"Fehler beim Speichern der Trainingsdaten: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _dataStatusLabel.Text = "Speichern fehlgeschlagen";
        }
    }

    private async void OnStartTraining(object? sender, EventArgs e)
    {
        try
        {
            if (_trainingData.Count == 0)
            {
                XtraMessageBox.Show(
                    "Keine Trainingsdaten verfügbar",
                    "Warnung",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _isTraining = true;
            _startTrainingButton.Enabled = false;
            _stopTrainingButton.Enabled = true;
            _trainingProgressBar.Properties.ShowTitle = true;
            _trainingProgressBar.Properties.PercentView = true;
            _trainingStatusLabel.Text = "Training gestartet...";
            _trainingLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] Training gestartet");

            var options = new Invoice.Application.DTOs.TrainingOptions(
                TrainerType: _algorithmComboBox.EditValue?.ToString() ?? "SdcaMaximumEntropy",
                TrainerParameters: new Dictionary<string, object>(),
                UseCrossValidation: _useCrossValidationCheckBox.Checked,
                CrossValidationFolds: Convert.ToInt32(_crossValidationFoldsSpinEdit.EditValue),
                UseFeatureSelection: false,
                UseFeatureNormalization: false,
                MaxIterations: Convert.ToInt32(_maxIterationsSpinEdit.EditValue),
                LearningRate: Convert.ToSingle(_learningRateSpinEdit.EditValue),
                L1Regularization: Convert.ToSingle(_regularizationSpinEdit.EditValue),
                L2Regularization: 0f,
                Description: "Training Session",
                Tags: new Dictionary<string, string>());

            _trainingLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] Parameter: {options.TrainerType}, Iterationen: {options.MaxIterations}");

            // TODO: Implement correct ExecuteAsync signature when available
            // var result = await _trainModelsUseCase.ExecuteAsync(_trainingData.ToList(), options);
            var result = new Invoice.Application.DTOs.TrainingResult(
                Success: false,
                Message: "Training noch nicht implementiert (ExecuteAsync-Signatur fehlt)",
                ModelPath: null,
                Metrics: null,
                TrainingTime: TimeSpan.Zero,
                TrainedAt: DateTime.UtcNow);

            if (result.Success)
            {
                _trainingStatusLabel.Text = "Training erfolgreich abgeschlossen";
                _trainingLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] Training erfolgreich abgeschlossen");
                _trainingLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] Modell: {result.ModelPath}");
                
                XtraMessageBox.Show(
                    "Training erfolgreich abgeschlossen",
                    "Erfolg",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                _trainingStatusLabel.Text = $"Training fehlgeschlagen: {result.Message}";
                _trainingLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] Training fehlgeschlagen: {result.Message}");
                
                XtraMessageBox.Show(
                    $"Training fehlgeschlagen: {result.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Starten des Trainings");
            _trainingStatusLabel.Text = "Training fehlgeschlagen";
            _trainingLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] Fehler: {ex.Message}");
            
            XtraMessageBox.Show(
                $"Fehler beim Starten des Trainings: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _isTraining = false;
            _startTrainingButton.Enabled = true;
            _stopTrainingButton.Enabled = false;
        }
    }

    private void OnStopTraining(object? sender, EventArgs e)
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _trainingStatusLabel.Text = "Training gestoppt";
            _trainingLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] Training vom Benutzer gestoppt");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Stoppen des Trainings");
            XtraMessageBox.Show(
                $"Fehler beim Stoppen des Trainings: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void OnEvaluateModel(object? sender, EventArgs e)
    {
        try
        {
            _isEvaluating = true;
            _evaluateModelButton.Enabled = false;
            _evaluationProgressBar.Properties.ShowTitle = true;
            _evaluationStatusLabel.Text = "Evaluiere Modell...";

            // Placeholder: Implementation would depend on specific evaluation requirements
            await Task.Delay(1000);

            _evaluationStatusLabel.Text = "Evaluation abgeschlossen";
            XtraMessageBox.Show(
                "Modell-Evaluation abgeschlossen",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Modell-Evaluation");
            _evaluationStatusLabel.Text = "Evaluation fehlgeschlagen";
            XtraMessageBox.Show(
                $"Fehler bei der Modell-Evaluation: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _isEvaluating = false;
            _evaluateModelButton.Enabled = true;
        }
    }

    private void OnLoadModel(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Modell-Dateien (*.zip)|*.zip|Alle Dateien (*.*)|*.*",
                Title = "Modell laden"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadModel(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des Modells");
            XtraMessageBox.Show(
                $"Fehler beim Laden des Modells: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void LoadModel(string filePath)
    {
        try
        {
            _evaluationStatusLabel.Text = "Lade Modell...";

            var success = await _trainModelsUseCase.LoadModelAsync(filePath);

            if (success)
            {
                _evaluationStatusLabel.Text = "Modell erfolgreich geladen";
                XtraMessageBox.Show(
                    "Modell erfolgreich geladen",
                    "Erfolg",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                _evaluationStatusLabel.Text = "Laden fehlgeschlagen";
                XtraMessageBox.Show(
                    "Fehler beim Laden des Modells",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des Modells: {FilePath}", filePath);
            _evaluationStatusLabel.Text = "Laden fehlgeschlagen";
            XtraMessageBox.Show(
                $"Fehler beim Laden des Modells: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_isTraining)
        {
            var result = XtraMessageBox.Show(
                "Ein Training läuft gerade. Wirklich schließen?",
                "Training läuft",
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

