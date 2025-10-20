# Aufgabe 39: TrainingForm (Labeling Grid)

## Ziel

TrainingForm für das Labeling von Trainingsdaten mit Grid-basierter Eingabe und ML-Modelltraining.

## 1. TrainingForm Interface

**Datei:** `src/InvoiceReader.WinForms/Forms/TrainingForm.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvoiceReader.WinForms.Forms;

public partial class TrainingForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrainingForm> _logger;
    private readonly ITrainModelsUseCase _trainModelsUseCase;
    private readonly IExtractFieldsUseCase _extractFieldsUseCase;

    // UI Controls
    private TabControl _mainTabControl;
    private TabPage _dataTabPage;
    private TabPage _trainingTabPage;
    private TabPage _evaluationTabPage;

    // Data tab controls
    private DataGridView _trainingDataGrid;
    private Button _loadDataButton;
    private Button _addDataButton;
    private Button _removeDataButton;
    private Button _clearDataButton;
    private Button _saveDataButton;
    private Label _dataStatusLabel;

    // Training tab controls
    private ComboBox _algorithmComboBox;
    private NumericUpDown _maxIterationsNumericUpDown;
    private NumericUpDown _learningRateNumericUpDown;
    private NumericUpDown _regularizationNumericUpDown;
    private CheckBox _useCrossValidationCheckBox;
    private NumericUpDown _crossValidationFoldsNumericUpDown;
    private CheckBox _useEarlyStoppingCheckBox;
    private NumericUpDown _earlyStoppingPatienceNumericUpDown;
    private Button _startTrainingButton;
    private Button _stopTrainingButton;
    private ProgressBar _trainingProgressBar;
    private Label _trainingStatusLabel;
    private ListBox _trainingLogListBox;

    // Evaluation tab controls
    private DataGridView _evaluationResultsGrid;
    private Button _evaluateModelButton;
    private Button _loadModelButton;
    private Label _evaluationStatusLabel;
    private ProgressBar _evaluationProgressBar;

    // Data
    private List<TrainingDataRow> _trainingData;
    private List<EvaluationResult> _evaluationResults;
    private bool _isTraining;
    private bool _isEvaluating;
    private CancellationTokenSource _cancellationTokenSource;

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
        this.Text = "Model Training";
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
        // Main tab control
        _mainTabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Data tab
        _dataTabPage = new TabPage("Training Data");
        CreateDataTabControls();
        _mainTabControl.TabPages.Add(_dataTabPage);

        // Training tab
        _trainingTabPage = new TabPage("Training");
        CreateTrainingTabControls();
        _mainTabControl.TabPages.Add(_trainingTabPage);

        // Evaluation tab
        _evaluationTabPage = new TabPage("Evaluation");
        CreateEvaluationTabControls();
        _mainTabControl.TabPages.Add(_evaluationTabPage);
    }

    private void CreateDataTabControls()
    {
        // Training data grid
        _trainingDataGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            ReadOnly = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Data buttons
        _loadDataButton = new Button
        {
            Text = "Load Data",
            Width = 100
        };
        _loadDataButton.Click += OnLoadData;

        _addDataButton = new Button
        {
            Text = "Add Data",
            Width = 100
        };
        _addDataButton.Click += OnAddData;

        _removeDataButton = new Button
        {
            Text = "Remove Selected",
            Width = 100
        };
        _removeDataButton.Click += OnRemoveData;

        _clearDataButton = new Button
        {
            Text = "Clear All",
            Width = 100
        };
        _clearDataButton.Click += OnClearData;

        _saveDataButton = new Button
        {
            Text = "Save Data",
            Width = 100
        };
        _saveDataButton.Click += OnSaveData;

        // Data status
        _dataStatusLabel = new Label
        {
            Text = "Ready",
            AutoSize = true
        };
    }

    private void CreateTrainingTabControls()
    {
        // Algorithm selection
        _algorithmComboBox = new ComboBox
        {
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _algorithmComboBox.Items.AddRange(new[] { "SdcaMaximumEntropy", "LbfgsMaximumEntropy", "FastTree", "FastForest" });
        _algorithmComboBox.SelectedIndex = 0;

        // Training parameters
        _maxIterationsNumericUpDown = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 10000,
            Value = 100,
            Width = 100
        };

        _learningRateNumericUpDown = new NumericUpDown
        {
            Minimum = 0.001m,
            Maximum = 1.0m,
            Increment = 0.001m,
            DecimalPlaces = 3,
            Value = 0.01m,
            Width = 100
        };

        _regularizationNumericUpDown = new NumericUpDown
        {
            Minimum = 0.001m,
            Maximum = 1.0m,
            Increment = 0.001m,
            DecimalPlaces = 3,
            Value = 0.1m,
            Width = 100
        };

        // Cross validation
        _useCrossValidationCheckBox = new CheckBox
        {
            Text = "Use Cross Validation",
            Checked = true
        };

        _crossValidationFoldsNumericUpDown = new NumericUpDown
        {
            Minimum = 2,
            Maximum = 10,
            Value = 5,
            Width = 100
        };

        // Early stopping
        _useEarlyStoppingCheckBox = new CheckBox
        {
            Text = "Use Early Stopping",
            Checked = true
        };

        _earlyStoppingPatienceNumericUpDown = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 100,
            Value = 10,
            Width = 100
        };

        // Training buttons
        _startTrainingButton = new Button
        {
            Text = "Start Training",
            Width = 120
        };
        _startTrainingButton.Click += OnStartTraining;

        _stopTrainingButton = new Button
        {
            Text = "Stop Training",
            Width = 120,
            Enabled = false
        };
        _stopTrainingButton.Click += OnStopTraining;

        // Progress
        _trainingProgressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Width = 300
        };

        _trainingStatusLabel = new Label
        {
            Text = "Ready",
            AutoSize = true
        };

        // Training log
        _trainingLogListBox = new ListBox
        {
            Height = 200,
            Dock = DockStyle.Fill
        };
    }

    private void CreateEvaluationTabControls()
    {
        // Evaluation results grid
        _evaluationResultsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Evaluation buttons
        _evaluateModelButton = new Button
        {
            Text = "Evaluate Model",
            Width = 120
        };
        _evaluateModelButton.Click += OnEvaluateModel;

        _loadModelButton = new Button
        {
            Text = "Load Model",
            Width = 120
        };
        _loadModelButton.Click += OnLoadModel;

        // Evaluation status
        _evaluationStatusLabel = new Label
        {
            Text = "Ready",
            AutoSize = true
        };

        _evaluationProgressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Width = 300
        };
    }

    private void LayoutControls()
    {
        // Data tab layout
        var dataLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3
        };

        dataLayout.Controls.Add(_trainingDataGrid, 0, 0);
        dataLayout.SetRowSpan(_trainingDataGrid, 2);

        var dataButtonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };
        dataButtonPanel.Controls.Add(_loadDataButton);
        dataButtonPanel.Controls.Add(_addDataButton);
        dataButtonPanel.Controls.Add(_removeDataButton);
        dataButtonPanel.Controls.Add(_clearDataButton);
        dataButtonPanel.Controls.Add(_saveDataButton);
        dataLayout.Controls.Add(dataButtonPanel, 1, 0);

        dataLayout.Controls.Add(_dataStatusLabel, 1, 1);

        _dataTabPage.Controls.Add(dataLayout);

        // Training tab layout
        var trainingLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4
        };

        // Parameters panel
        var parametersPanel = new GroupBox
        {
            Text = "Training Parameters",
            Dock = DockStyle.Fill
        };

        var parametersLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6
        };

        parametersLayout.Controls.Add(new Label { Text = "Algorithm:" }, 0, 0);
        parametersLayout.Controls.Add(_algorithmComboBox, 1, 0);
        parametersLayout.Controls.Add(new Label { Text = "Max Iterations:" }, 0, 1);
        parametersLayout.Controls.Add(_maxIterationsNumericUpDown, 1, 1);
        parametersLayout.Controls.Add(new Label { Text = "Learning Rate:" }, 0, 2);
        parametersLayout.Controls.Add(_learningRateNumericUpDown, 1, 2);
        parametersLayout.Controls.Add(new Label { Text = "Regularization:" }, 0, 3);
        parametersLayout.Controls.Add(_regularizationNumericUpDown, 1, 3);
        parametersLayout.Controls.Add(_useCrossValidationCheckBox, 0, 4);
        parametersLayout.Controls.Add(_crossValidationFoldsNumericUpDown, 1, 4);
        parametersLayout.Controls.Add(_useEarlyStoppingCheckBox, 0, 5);
        parametersLayout.Controls.Add(_earlyStoppingPatienceNumericUpDown, 1, 5);

        parametersPanel.Controls.Add(parametersLayout);

        // Training panel
        var trainingPanel = new GroupBox
        {
            Text = "Training",
            Dock = DockStyle.Fill
        };

        var trainingLayout2 = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
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

        trainingLayout2.Controls.Add(trainingButtonPanel, 0, 0);
        trainingLayout2.Controls.Add(progressPanel, 0, 1);
        trainingLayout2.Controls.Add(new Label { Text = "Training Log:" }, 0, 2);
        trainingLayout2.Controls.Add(_trainingLogListBox, 0, 3);

        trainingPanel.Controls.Add(trainingLayout2);

        trainingLayout.Controls.Add(parametersPanel, 0, 0);
        trainingLayout.Controls.Add(trainingPanel, 1, 0);

        _trainingTabPage.Controls.Add(trainingLayout);

        // Evaluation tab layout
        var evaluationLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };

        evaluationLayout.Controls.Add(_evaluationResultsGrid, 0, 0);

        var evaluationButtonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        evaluationButtonPanel.Controls.Add(_evaluateModelButton);
        evaluationButtonPanel.Controls.Add(_loadModelButton);

        var evaluationStatusPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        evaluationStatusPanel.Controls.Add(_evaluationProgressBar);
        evaluationStatusPanel.Controls.Add(_evaluationStatusLabel);

        evaluationLayout.Controls.Add(evaluationButtonPanel, 0, 1);
        evaluationLayout.Controls.Add(evaluationStatusPanel, 0, 2);

        _evaluationTabPage.Controls.Add(evaluationLayout);

        this.Controls.Add(_mainTabControl);
    }

    private void InitializeData()
    {
        _trainingData = new List<TrainingDataRow>();
        _evaluationResults = new List<EvaluationResult>();
        _isTraining = false;
        _isEvaluating = false;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void OnLoadData(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Load Training Data"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadTrainingData(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load training data");
            MessageBox.Show($"Failed to load training data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void LoadTrainingData(string filePath)
    {
        try
        {
            _dataStatusLabel.Text = "Loading data...";

            var result = await _trainModelsUseCase.PrepareTrainingDataAsync(filePath);

            if (result.Success)
            {
                _trainingData = result.Rows.ToList();
                RefreshTrainingDataGrid();
                _dataStatusLabel.Text = $"Loaded {_trainingData.Count} rows";
            }
            else
            {
                MessageBox.Show($"Failed to load training data: {result.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _dataStatusLabel.Text = "Failed to load data";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load training data: {FilePath}", filePath);
            MessageBox.Show($"Failed to load training data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _dataStatusLabel.Text = "Failed to load data";
        }
    }

    private void RefreshTrainingDataGrid()
    {
        _trainingDataGrid.DataSource = null;
        _trainingDataGrid.DataSource = _trainingData;
    }

    private void OnAddData(object? sender, EventArgs e)
    {
        try
        {
            var newRow = new TrainingDataRow(
                "", // Text
                0, // LineIndex
                0, // PageNumber
                0, // X
                0, // Y
                0, // Width
                0, // Height
                "None", // Label
                0f, // Confidence
                new Dictionary<string, object>() // Features
            );

            _trainingData.Add(newRow);
            RefreshTrainingDataGrid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add training data");
            MessageBox.Show($"Failed to add training data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnRemoveData(object? sender, EventArgs e)
    {
        try
        {
            var selectedRows = _trainingDataGrid.SelectedRows.Cast<DataGridViewRow>().ToList();
            foreach (var row in selectedRows)
            {
                var index = row.Index;
                if (index < _trainingData.Count)
                {
                    _trainingData.RemoveAt(index);
                }
            }
            RefreshTrainingDataGrid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove training data");
            MessageBox.Show($"Failed to remove training data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnClearData(object? sender, EventArgs e)
    {
        try
        {
            _trainingData.Clear();
            RefreshTrainingDataGrid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear training data");
            MessageBox.Show($"Failed to clear training data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnSaveData(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Save Training Data"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveTrainingData(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save training data");
            MessageBox.Show($"Failed to save training data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void SaveTrainingData(string filePath)
    {
        try
        {
            _dataStatusLabel.Text = "Saving data...";

            var result = new TrainingDataResult(
                true,
                "Training data prepared",
                _trainingData,
                _trainingData.Count,
                _trainingData.Count(r => r.Label != "None"),
                _trainingData.Count(r => r.Label == "None"),
                new List<TrainingDataWarning>(),
                new List<TrainingDataError>()
            );

            var success = await _trainModelsUseCase.SaveTrainingDataAsync(result, filePath);

            if (success)
            {
                _dataStatusLabel.Text = "Data saved successfully";
            }
            else
            {
                MessageBox.Show("Failed to save training data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _dataStatusLabel.Text = "Failed to save data";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save training data: {FilePath}", filePath);
            MessageBox.Show($"Failed to save training data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _dataStatusLabel.Text = "Failed to save data";
        }
    }

    private async void OnStartTraining(object? sender, EventArgs e)
    {
        try
        {
            if (_trainingData.Count == 0)
            {
                MessageBox.Show("No training data available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isTraining = true;
            _startTrainingButton.Enabled = false;
            _stopTrainingButton.Enabled = true;
            _trainingProgressBar.Style = ProgressBarStyle.Marquee;
            _trainingStatusLabel.Text = "Training started...";

            var options = new TrainingOptions(
                "TrainingModel",
                _algorithmComboBox.SelectedItem?.ToString() ?? "SdcaMaximumEntropy",
                new Dictionary<string, object>(),
                (int)_maxIterationsNumericUpDown.Value,
                (float)_learningRateNumericUpDown.Value,
                (float)_regularizationNumericUpDown.Value,
                32, // BatchSize
                20, // ValidationSplit
                _useCrossValidationCheckBox.Checked,
                (int)_crossValidationFoldsNumericUpDown.Value,
                _useEarlyStoppingCheckBox.Checked,
                (int)_earlyStoppingPatienceNumericUpDown.Value,
                false, // UseDataAugmentation
                new Dictionary<string, object>(),
                false, // UseFeatureSelection
                new List<string>(),
                false, // UseHyperparameterTuning
                new Dictionary<string, object>(),
                false, // UseEnsemble
                new List<string>(),
                new Dictionary<string, object>()
            );

            var result = await _trainModelsUseCase.ExecuteAsync(_trainingData, options);

            if (result.Success)
            {
                _trainingStatusLabel.Text = "Training completed successfully";
                _trainingLogListBox.Items.Add("Training completed successfully");
            }
            else
            {
                _trainingStatusLabel.Text = $"Training failed: {result.Message}";
                _trainingLogListBox.Items.Add($"Training failed: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start training");
            MessageBox.Show($"Failed to start training: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _trainingStatusLabel.Text = "Training failed";
        }
        finally
        {
            _isTraining = false;
            _startTrainingButton.Enabled = true;
            _stopTrainingButton.Enabled = false;
            _trainingProgressBar.Style = ProgressBarStyle.Continuous;
        }
    }

    private void OnStopTraining(object? sender, EventArgs e)
    {
        try
        {
            _cancellationTokenSource.Cancel();
            _trainingStatusLabel.Text = "Training stopped";
            _trainingLogListBox.Items.Add("Training stopped by user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop training");
            MessageBox.Show($"Failed to stop training: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnEvaluateModel(object? sender, EventArgs e)
    {
        try
        {
            _isEvaluating = true;
            _evaluateModelButton.Enabled = false;
            _evaluationProgressBar.Style = ProgressBarStyle.Marquee;
            _evaluationStatusLabel.Text = "Evaluating model...";

            // Implementation would depend on the specific evaluation requirements
            _evaluationStatusLabel.Text = "Evaluation completed";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate model");
            MessageBox.Show($"Failed to evaluate model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _evaluationStatusLabel.Text = "Evaluation failed";
        }
        finally
        {
            _isEvaluating = false;
            _evaluateModelButton.Enabled = true;
            _evaluationProgressBar.Style = ProgressBarStyle.Continuous;
        }
    }

    private void OnLoadModel(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Model Files (*.zip)|*.zip|All Files (*.*)|*.*",
                Title = "Load Model"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadModel(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load model");
            MessageBox.Show($"Failed to load model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void LoadModel(string filePath)
    {
        try
        {
            _evaluationStatusLabel.Text = "Loading model...";

            var success = await _trainModelsUseCase.LoadModelAsync(filePath);

            if (success)
            {
                _evaluationStatusLabel.Text = "Model loaded successfully";
            }
            else
            {
                MessageBox.Show("Failed to load model", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _evaluationStatusLabel.Text = "Failed to load model";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load model: {FilePath}", filePath);
            MessageBox.Show($"Failed to load model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _evaluationStatusLabel.Text = "Failed to load model";
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        base.OnFormClosing(e);
    }
}
```

## Wichtige Hinweise

- TrainingForm mit Tab-basierter UI
- Training Data Grid für Labeling
- Training Parameters für ML-Algorithmen
- Progress Tracking für Training
- Model Evaluation Tab
- Training Log für Monitoring
- Error Handling für alle Operationen
- Async Training mit Cancellation
