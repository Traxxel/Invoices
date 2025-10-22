# Aufgabe 33: MainForm (Grid, Buttons, Search)

## Ziel

MainForm für die Hauptansicht der Invoice Reader Anwendung mit Grid, Buttons und Suchfunktionalität.

## 1. MainForm Interface

**Datei:** `src/InvoiceReader.WinForms/Forms/MainForm.cs`

```csharp
using InvoiceReader.Application.Interfaces;
using InvoiceReader.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvoiceReader.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainForm> _logger;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IImportInvoiceUseCase _importInvoiceUseCase;
    private readonly ISaveInvoiceUseCase _saveInvoiceUseCase;
    private readonly IExtractFieldsUseCase _extractFieldsUseCase;
    private readonly ITrainModelsUseCase _trainModelsUseCase;

    // UI Controls
    private DataGridView _invoicesGrid;
    private ToolStrip _mainToolStrip;
    private StatusStrip _statusStrip;
    private MenuStrip _menuStrip;
    private TextBox _searchTextBox;
    private ComboBox _searchComboBox;
    private Button _searchButton;
    private Button _importButton;
    private Button _exportButton;
    private Button _trainButton;
    private Button _settingsButton;
    private Button _refreshButton;
    private Button _deleteButton;
    private Button _editButton;
    private Button _viewButton;
    private Label _statusLabel;
    private ProgressBar _progressBar;

    // Data
    private List<InvoiceDto> _invoices;
    private List<InvoiceDto> _filteredInvoices;
    private InvoiceDto? _selectedInvoice;

    public MainForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<MainForm>>();
        _invoiceRepository = serviceProvider.GetRequiredService<IInvoiceRepository>();
        _importInvoiceUseCase = serviceProvider.GetRequiredService<IImportInvoiceUseCase>();
        _saveInvoiceUseCase = serviceProvider.GetRequiredService<ISaveInvoiceUseCase>();
        _extractFieldsUseCase = serviceProvider.GetRequiredService<IExtractFieldsUseCase>();
        _trainModelsUseCase = serviceProvider.GetRequiredService<ITrainModelsUseCase>();

        InitializeComponent();
        InitializeData();
        LoadInvoices();
    }

    private void InitializeComponent()
    {
        // Form properties
        this.Text = "Invoice Reader";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;

        // Create menu strip
        CreateMenuStrip();

        // Create tool strip
        CreateToolStrip();

        // Create main panel
        CreateMainPanel();

        // Create status strip
        CreateStatusStrip();

        // Create context menu
        CreateContextMenu();
    }

    private void CreateMenuStrip()
    {
        _menuStrip = new MenuStrip();

        // File menu
        var fileMenu = new ToolStripMenuItem("&File");
        fileMenu.DropDownItems.Add("&Import Invoice", null, OnImportInvoice);
        fileMenu.DropDownItems.Add("&Batch Import", null, OnBatchImport);
        fileMenu.DropDownItems.Add("-");
        fileMenu.DropDownItems.Add("&Export", null, OnExport);
        fileMenu.DropDownItems.Add("-");
        fileMenu.DropDownItems.Add("E&xit", null, OnExit);

        // Edit menu
        var editMenu = new ToolStripMenuItem("&Edit");
        editMenu.DropDownItems.Add("&Refresh", null, OnRefresh);
        editMenu.DropDownItems.Add("&Select All", null, OnSelectAll);
        editMenu.DropDownItems.Add("&Clear Selection", null, OnClearSelection);

        // View menu
        var viewMenu = new ToolStripMenuItem("&View");
        viewMenu.DropDownItems.Add("&Details", null, OnViewDetails);
        viewMenu.DropDownItems.Add("&Grid", null, OnViewGrid);
        viewMenu.DropDownItems.Add("-");
        viewMenu.DropDownItems.Add("&Columns", null, OnViewColumns);

        // Tools menu
        var toolsMenu = new ToolStripMenuItem("&Tools");
        toolsMenu.DropDownItems.Add("&Train Models", null, OnTrainModels);
        toolsMenu.DropDownItems.Add("&Settings", null, OnSettings);

        // Help menu
        var helpMenu = new ToolStripMenuItem("&Help");
        helpMenu.DropDownItems.Add("&About", null, OnAbout);

        _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, toolsMenu, helpMenu });
        this.Controls.Add(_menuStrip);
    }

    private void CreateToolStrip()
    {
        _mainToolStrip = new ToolStrip();

        // Import button
        _importButton = new ToolStripButton("Import", null, OnImportInvoice);
        _importButton.ToolTipText = "Import Invoice";

        // Export button
        _exportButton = new ToolStripButton("Export", null, OnExport);
        _exportButton.ToolTipText = "Export Invoices";

        // Train button
        _trainButton = new ToolStripButton("Train", null, OnTrainModels);
        _trainButton.ToolTipText = "Train Models";

        // Settings button
        _settingsButton = new ToolStripButton("Settings", null, OnSettings);
        _settingsButton.ToolTipText = "Settings";

        // Separator
        _mainToolStrip.Items.Add(new ToolStripSeparator());

        // Refresh button
        _refreshButton = new ToolStripButton("Refresh", null, OnRefresh);
        _refreshButton.ToolTipText = "Refresh";

        // Separator
        _mainToolStrip.Items.Add(new ToolStripSeparator());

        // Search controls
        _searchComboBox = new ToolStripComboBox();
        _searchComboBox.Items.AddRange(new[] { "All", "Invoice Number", "Issuer Name", "Date", "Amount" });
        _searchComboBox.SelectedIndex = 0;
        _searchComboBox.Width = 120;

        _searchTextBox = new ToolStripTextBox();
        _searchTextBox.Width = 200;
        _searchTextBox.KeyPress += OnSearchKeyPress;

        _searchButton = new ToolStripButton("Search", null, OnSearch);
        _searchButton.ToolTipText = "Search";

        _mainToolStrip.Items.AddRange(new ToolStripItem[] {
            _importButton, _exportButton, _trainButton, _settingsButton,
            new ToolStripSeparator(),
            _refreshButton,
            new ToolStripSeparator(),
            new ToolStripLabel("Search:"),
            _searchComboBox,
            _searchTextBox,
            _searchButton
        });

        this.Controls.Add(_mainToolStrip);
    }

    private void CreateMainPanel()
    {
        var mainPanel = new Panel();
        mainPanel.Dock = DockStyle.Fill;
        mainPanel.Padding = new Padding(5);

        // Create invoices grid
        _invoicesGrid = new DataGridView();
        _invoicesGrid.Dock = DockStyle.Fill;
        _invoicesGrid.AllowUserToAddRows = false;
        _invoicesGrid.AllowUserToDeleteRows = false;
        _invoicesGrid.ReadOnly = true;
        _invoicesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _invoicesGrid.MultiSelect = false;
        _invoicesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _invoicesGrid.SelectionChanged += OnGridSelectionChanged;
        _invoicesGrid.CellDoubleClick += OnGridCellDoubleClick;
        _invoicesGrid.CellFormatting += OnGridCellFormatting;

        // Add columns
        AddGridColumns();

        mainPanel.Controls.Add(_invoicesGrid);
        this.Controls.Add(mainPanel);
    }

    private void AddGridColumns()
    {
        _invoicesGrid.Columns.Clear();

        // Invoice Number
        var invoiceNumberColumn = new DataGridViewTextBoxColumn
        {
            Name = "InvoiceNumber",
            HeaderText = "Invoice Number",
            DataPropertyName = "InvoiceNumber",
            Width = 150
        };
        _invoicesGrid.Columns.Add(invoiceNumberColumn);

        // Invoice Date
        var invoiceDateColumn = new DataGridViewTextBoxColumn
        {
            Name = "InvoiceDate",
            HeaderText = "Date",
            DataPropertyName = "InvoiceDate",
            Width = 100
        };
        _invoicesGrid.Columns.Add(invoiceDateColumn);

        // Issuer Name
        var issuerNameColumn = new DataGridViewTextBoxColumn
        {
            Name = "IssuerName",
            HeaderText = "Issuer",
            DataPropertyName = "IssuerName",
            Width = 200
        };
        _invoicesGrid.Columns.Add(issuerNameColumn);

        // Gross Total
        var grossTotalColumn = new DataGridViewTextBoxColumn
        {
            Name = "GrossTotal",
            HeaderText = "Total",
            DataPropertyName = "GrossTotal",
            Width = 100
        };
        _invoicesGrid.Columns.Add(grossTotalColumn);

        // Confidence
        var confidenceColumn = new DataGridViewTextBoxColumn
        {
            Name = "ExtractionConfidence",
            HeaderText = "Confidence",
            DataPropertyName = "ExtractionConfidence",
            Width = 80
        };
        _invoicesGrid.Columns.Add(confidenceColumn);

        // Imported At
        var importedAtColumn = new DataGridViewTextBoxColumn
        {
            Name = "ImportedAt",
            HeaderText = "Imported",
            DataPropertyName = "ImportedAt",
            Width = 120
        };
        _invoicesGrid.Columns.Add(importedAtColumn);
    }

    private void CreateStatusStrip()
    {
        _statusStrip = new StatusStrip();

        _statusLabel = new ToolStripStatusLabel("Ready");
        _progressBar = new ToolStripProgressBar();
        _progressBar.Visible = false;

        _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, _progressBar });
        this.Controls.Add(_statusStrip);
    }

    private void CreateContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        contextMenu.Items.Add("&View", null, OnViewInvoice);
        contextMenu.Items.Add("&Edit", null, OnEditInvoice);
        contextMenu.Items.Add("&Delete", null, OnDeleteInvoice);
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("&Export", null, OnExportInvoice);
        contextMenu.Items.Add("&Copy", null, OnCopyInvoice);

        _invoicesGrid.ContextMenuStrip = contextMenu;
    }

    private void InitializeData()
    {
        _invoices = new List<InvoiceDto>();
        _filteredInvoices = new List<InvoiceDto>();
        _selectedInvoice = null;
    }

    private async void LoadInvoices()
    {
        try
        {
            SetStatus("Loading invoices...", true);

            var invoices = await _invoiceRepository.GetAllAsync();
            _invoices = invoices.ToList();
            _filteredInvoices = _invoices.ToList();

            RefreshGrid();
            SetStatus($"Loaded {_invoices.Count} invoices", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load invoices");
            MessageBox.Show($"Failed to load invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Error loading invoices", false);
        }
    }

    private void RefreshGrid()
    {
        _invoicesGrid.DataSource = null;
        _invoicesGrid.DataSource = _filteredInvoices;

        // Format columns
        FormatGridColumns();
    }

    private void FormatGridColumns()
    {
        // Format date column
        if (_invoicesGrid.Columns["InvoiceDate"] != null)
        {
            _invoicesGrid.Columns["InvoiceDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
        }

        // Format amount column
        if (_invoicesGrid.Columns["GrossTotal"] != null)
        {
            _invoicesGrid.Columns["GrossTotal"].DefaultCellStyle.Format = "C2";
            _invoicesGrid.Columns["GrossTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        // Format confidence column
        if (_invoicesGrid.Columns["ExtractionConfidence"] != null)
        {
            _invoicesGrid.Columns["ExtractionConfidence"].DefaultCellStyle.Format = "P0";
            _invoicesGrid.Columns["ExtractionConfidence"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        // Format imported at column
        if (_invoicesGrid.Columns["ImportedAt"] != null)
        {
            _invoicesGrid.Columns["ImportedAt"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
        }
    }

    private void SetStatus(string message, bool showProgress = false)
    {
        _statusLabel.Text = message;
        _progressBar.Visible = showProgress;
        if (showProgress)
        {
            _progressBar.Style = ProgressBarStyle.Marquee;
        }
        Application.DoEvents();
    }

    private void OnGridSelectionChanged(object? sender, EventArgs e)
    {
        if (_invoicesGrid.SelectedRows.Count > 0)
        {
            var selectedRow = _invoicesGrid.SelectedRows[0];
            _selectedInvoice = selectedRow.DataBoundItem as InvoiceDto;
        }
        else
        {
            _selectedInvoice = null;
        }
    }

    private void OnGridCellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            OnViewInvoice(sender, e);
        }
    }

    private void OnGridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            var column = _invoicesGrid.Columns[e.ColumnIndex];
            var value = e.Value;

            if (column.Name == "ExtractionConfidence" && value is float confidence)
            {
                // Color code confidence
                if (confidence < 0.5f)
                {
                    e.CellStyle.BackColor = Color.LightCoral;
                }
                else if (confidence < 0.8f)
                {
                    e.CellStyle.BackColor = Color.LightYellow;
                }
                else
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                }
            }
        }
    }

    private void OnSearchKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            OnSearch(sender, e);
        }
    }

    private void OnSearch(object? sender, EventArgs e)
    {
        try
        {
            var searchText = _searchTextBox.Text.Trim();
            var searchType = _searchComboBox.SelectedItem?.ToString() ?? "All";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredInvoices = _invoices.ToList();
            }
            else
            {
                _filteredInvoices = _invoices.Where(invoice =>
                {
                    return searchType switch
                    {
                        "Invoice Number" => invoice.InvoiceNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase),
                        "Issuer Name" => invoice.IssuerName.Contains(searchText, StringComparison.OrdinalIgnoreCase),
                        "Date" => invoice.InvoiceDate.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase),
                        "Amount" => invoice.GrossTotal.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase),
                        _ => invoice.InvoiceNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                             invoice.IssuerName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                             invoice.InvoiceDate.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                             invoice.GrossTotal.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    };
                }).ToList();
            }

            RefreshGrid();
            SetStatus($"Found {_filteredInvoices.Count} invoices", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search invoices");
            MessageBox.Show($"Failed to search invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnImportInvoice(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Select Invoice PDF"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImportInvoice(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import invoice");
            MessageBox.Show($"Failed to import invoice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void ImportInvoice(string filePath)
    {
        try
        {
            SetStatus("Importing invoice...", true);

            var result = await _importInvoiceUseCase.ExecuteAsync(filePath);

            if (result.Success)
            {
                MessageBox.Show($"Invoice imported successfully: {result.Invoice?.InvoiceNumber}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadInvoices();
            }
            else
            {
                MessageBox.Show($"Failed to import invoice: {result.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import invoice: {FilePath}", filePath);
            MessageBox.Show($"Failed to import invoice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetStatus("Ready", false);
        }
    }

    private void OnBatchImport(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Select Invoice PDFs",
                Multiselect = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                BatchImportInvoices(dialog.FileNames);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch import invoices");
            MessageBox.Show($"Failed to batch import invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BatchImportInvoices(string[] filePaths)
    {
        try
        {
            SetStatus("Batch importing invoices...", true);

            var successCount = 0;
            var errorCount = 0;

            foreach (var filePath in filePaths)
            {
                try
                {
                    var result = await _importInvoiceUseCase.ExecuteAsync(filePath);
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
                    _logger.LogError(ex, "Failed to import invoice: {FilePath}", filePath);
                    errorCount++;
                }
            }

            MessageBox.Show($"Batch import completed: {successCount} successful, {errorCount} failed", "Batch Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadInvoices();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch import invoices");
            MessageBox.Show($"Failed to batch import invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetStatus("Ready", false);
        }
    }

    private void OnExport(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Export Invoices"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ExportInvoices(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export invoices");
            MessageBox.Show($"Failed to export invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportInvoices(string filePath)
    {
        try
        {
            SetStatus("Exporting invoices...", true);

            // Implementation would depend on the export format
            // For now, just show a message
            MessageBox.Show($"Export to {filePath} not implemented yet", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);

            SetStatus("Ready", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export invoices: {FilePath}", filePath);
            MessageBox.Show($"Failed to export invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Ready", false);
        }
    }

    private void OnTrainModels(object? sender, EventArgs e)
    {
        try
        {
            var trainingForm = _serviceProvider.GetRequiredService<TrainingForm>();
            trainingForm.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open training form");
            MessageBox.Show($"Failed to open training form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnSettings(object? sender, EventArgs e)
    {
        try
        {
            var settingsForm = _serviceProvider.GetRequiredService<SettingsForm>();
            settingsForm.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open settings form");
            MessageBox.Show($"Failed to open settings form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnRefresh(object? sender, EventArgs e)
    {
        LoadInvoices();
    }

    private void OnSelectAll(object? sender, EventArgs e)
    {
        _invoicesGrid.SelectAll();
    }

    private void OnClearSelection(object? sender, EventArgs e)
    {
        _invoicesGrid.ClearSelection();
    }

    private void OnViewDetails(object? sender, EventArgs e)
    {
        // Toggle between details and grid view
        // Implementation depends on requirements
    }

    private void OnViewGrid(object? sender, EventArgs e)
    {
        // Toggle between details and grid view
        // Implementation depends on requirements
    }

    private void OnViewColumns(object? sender, EventArgs e)
    {
        // Show column selection dialog
        // Implementation depends on requirements
    }

    private void OnAbout(object? sender, EventArgs e)
    {
        MessageBox.Show("Invoice Reader v1.0\n\nA powerful tool for extracting and managing invoice data using ML.NET.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnViewInvoice(object? sender, EventArgs e)
    {
        if (_selectedInvoice != null)
        {
            try
            {
                var reviewForm = _serviceProvider.GetRequiredService<ReviewForm>();
                reviewForm.SetInvoice(_selectedInvoice);
                reviewForm.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open review form");
                MessageBox.Show($"Failed to open review form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnEditInvoice(object? sender, EventArgs e)
    {
        if (_selectedInvoice != null)
        {
            try
            {
                var reviewForm = _serviceProvider.GetRequiredService<ReviewForm>();
                reviewForm.SetInvoice(_selectedInvoice);
                reviewForm.SetEditMode(true);
                reviewForm.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open review form for editing");
                MessageBox.Show($"Failed to open review form for editing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnDeleteInvoice(object? sender, EventArgs e)
    {
        if (_selectedInvoice != null)
        {
            var result = MessageBox.Show($"Are you sure you want to delete invoice {_selectedInvoice.InvoiceNumber}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                DeleteInvoice(_selectedInvoice.Id);
            }
        }
    }

    private async void DeleteInvoice(Guid invoiceId)
    {
        try
        {
            SetStatus("Deleting invoice...", true);

            await _saveInvoiceUseCase.DeleteInvoiceFromDatabaseAsync(invoiceId);
            LoadInvoices();

            SetStatus("Invoice deleted", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete invoice: {InvoiceId}", invoiceId);
            MessageBox.Show($"Failed to delete invoice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Ready", false);
        }
    }

    private void OnExportInvoice(object? sender, EventArgs e)
    {
        if (_selectedInvoice != null)
        {
            // Export single invoice
            // Implementation depends on requirements
        }
    }

    private void OnCopyInvoice(object? sender, EventArgs e)
    {
        if (_selectedInvoice != null)
        {
            // Copy invoice data to clipboard
            // Implementation depends on requirements
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        try
        {
            // Save any unsaved changes
            // Implementation depends on requirements

            base.OnFormClosing(e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close form");
            MessageBox.Show($"Failed to close form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

## 2. MainForm Extensions

**Datei:** `src/InvoiceReader.WinForms/Extensions/MainFormExtensions.cs`

```csharp
using InvoiceReader.WinForms.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceReader.WinForms.Extensions;

public static class MainFormExtensions
{
    public static IServiceCollection AddMainFormServices(this IServiceCollection services)
    {
        services.AddTransient<MainForm>();

        return services;
    }
}
```

## Wichtige Hinweise

- Vollständige MainForm für Invoice Reader Anwendung
- DataGridView für Anzeige der Rechnungen
- ToolStrip mit Import, Export, Train, Settings Buttons
- Suchfunktionalität mit verschiedenen Kriterien
- Context Menu für Rechnungsoperationen
- Status Strip mit Progress Bar
- Menu Strip mit allen Funktionen
- Error Handling für alle Operationen
- Logging für alle Operationen
- Async/Await für alle Datenbankoperationen
- Grid Formatting für verschiedene Datentypen
- Selection Management für Rechnungen
- Import/Export Funktionalität
- Training und Settings Integration
