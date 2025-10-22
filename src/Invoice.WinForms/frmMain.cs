using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Invoice.Application.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraBars;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Invoice.WinForms
{
    public partial class frmMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<frmMain> _logger;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IImportInvoiceUseCase _importInvoiceUseCase;
        private readonly ISaveInvoiceUseCase _saveInvoiceUseCase;
        private readonly IExtractFieldsUseCase _extractFieldsUseCase;
        private readonly ITrainModelsUseCase _trainModelsUseCase;

        // Data
        private List<InvoiceDto> _invoices;
        private List<InvoiceDto> _filteredInvoices;
        private InvoiceDto? _selectedInvoice;

        public frmMain(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<frmMain>>();
            _invoiceRepository = serviceProvider.GetRequiredService<IInvoiceRepository>();
            _importInvoiceUseCase = serviceProvider.GetRequiredService<IImportInvoiceUseCase>();
            _saveInvoiceUseCase = serviceProvider.GetRequiredService<ISaveInvoiceUseCase>();
            _extractFieldsUseCase = serviceProvider.GetRequiredService<IExtractFieldsUseCase>();
            _trainModelsUseCase = serviceProvider.GetRequiredService<ITrainModelsUseCase>();

            _invoices = new List<InvoiceDto>();
            _filteredInvoices = new List<InvoiceDto>();
            _selectedInvoice = null;

            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Invoice Reader";
            this.WindowState = FormWindowState.Maximized;

            // Load data
            LoadInvoices();
        }

        private async void LoadInvoices()
        {
            try
            {
                SetStatus("Rechnungen werden geladen...", true);

                var invoices = await _invoiceRepository.GetAllAsync();
                _invoices = invoices.Select(i => i.ToDto()).ToList();
                _filteredInvoices = _invoices.ToList();

                RefreshGrid();
                SetStatus($"{_invoices.Count} Rechnungen geladen", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Laden der Rechnungen");
                XtraMessageBox.Show($"Fehler beim Laden der Rechnungen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Fehler beim Laden der Rechnungen", false);
            }
        }

        private void RefreshGrid()
        {
            gridControl.DataSource = null;
            gridControl.DataSource = _filteredInvoices;

            // Format columns
            FormatGridColumns();
        }

        private void FormatGridColumns()
        {
            if (gridView.Columns["InvoiceDate"] != null)
            {
                gridView.Columns["InvoiceDate"].DisplayFormat.FormatString = "dd.MM.yyyy";
                gridView.Columns["InvoiceDate"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            }

            if (gridView.Columns["GrossTotal"] != null)
            {
                gridView.Columns["GrossTotal"].DisplayFormat.FormatString = "c2";
                gridView.Columns["GrossTotal"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            }

            if (gridView.Columns["ExtractionConfidence"] != null)
            {
                gridView.Columns["ExtractionConfidence"].DisplayFormat.FormatString = "p0";
                gridView.Columns["ExtractionConfidence"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            }

            if (gridView.Columns["ImportedAt"] != null)
            {
                gridView.Columns["ImportedAt"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
                gridView.Columns["ImportedAt"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            }

            gridView.BestFitColumns();
        }

        private void SetStatus(string message, bool showProgress = false)
        {
            statusLabel.Caption = message;
            progressBarControl.Visible = showProgress;
            if (showProgress)
            {
                progressBarControl.Properties.ShowTitle = true;
            }
            System.Windows.Forms.Application.DoEvents();
        }

        private void gridView_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle >= 0)
            {
                _selectedInvoice = gridView.GetRow(e.FocusedRowHandle) as InvoiceDto;
            }
            else
            {
                _selectedInvoice = null;
            }
        }

        private void gridView_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                var invoice = gridView.GetRow(e.RowHandle) as InvoiceDto;
                if (invoice != null)
                {
                    if (invoice.ExtractionConfidence < 0.5f)
                    {
                        e.Appearance.BackColor = Color.LightCoral;
                    }
                    else if (invoice.ExtractionConfidence < 0.8f)
                    {
                        e.Appearance.BackColor = Color.LightYellow;
                    }
                    else
                    {
                        e.Appearance.BackColor = Color.LightGreen;
                    }
                }
            }
        }

        private void gridView_DoubleClick(object sender, EventArgs e)
        {
            OnViewInvoice();
        }

        private void barButtonImport_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnImportInvoice();
        }

        private void barButtonBatchImport_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnBatchImport();
        }

        private void barButtonExport_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnExport();
        }

        private void barButtonTrain_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnTrainModels();
        }

        private void barButtonSettings_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnSettings();
        }

        private void barButtonRefresh_ItemClick(object sender, ItemClickEventArgs e)
        {
            LoadInvoices();
        }

        private void barButtonDelete_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnDeleteInvoice();
        }

        private void barButtonEdit_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnEditInvoice();
        }

        private void barButtonView_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnViewInvoice();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            OnSearch();
        }

        private void searchTextEdit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                OnSearch();
                e.Handled = true;
            }
        }

        private void OnSearch()
        {
            try
            {
                var searchText = searchTextEdit.Text.Trim();
                var searchType = searchComboBoxEdit.SelectedItem?.ToString() ?? "Alle";

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
                            "Rechnungsnummer" => invoice.InvoiceNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase),
                            "Aussteller" => invoice.IssuerName.Contains(searchText, StringComparison.OrdinalIgnoreCase),
                            "Datum" => invoice.InvoiceDate.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase),
                            "Betrag" => invoice.GrossTotal.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase),
                            _ => invoice.InvoiceNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                 invoice.IssuerName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                 invoice.InvoiceDate.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                 invoice.GrossTotal.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase)
                        };
                    }).ToList();
                }

                RefreshGrid();
                SetStatus($"{_filteredInvoices.Count} Rechnungen gefunden", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der Suche");
                XtraMessageBox.Show($"Fehler bei der Suche: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnImportInvoice()
        {
            try
            {
                using var dialog = new OpenFileDialog
                {
                    Filter = "PDF Dateien (*.pdf)|*.pdf|Alle Dateien (*.*)|*.*",
                    Title = "Rechnung auswählen"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ImportInvoice(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Importieren");
                XtraMessageBox.Show($"Fehler beim Importieren: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ImportInvoice(string filePath)
        {
            try
            {
                SetStatus("Rechnung wird importiert...", true);

                var result = await _importInvoiceUseCase.ExecuteAsync(filePath);

                if (result.Success)
                {
                    XtraMessageBox.Show($"Rechnung erfolgreich importiert: {result.Invoice?.InvoiceNumber}", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadInvoices();
                }
                else
                {
                    XtraMessageBox.Show($"Fehler beim Importieren: {result.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Importieren: {FilePath}", filePath);
                XtraMessageBox.Show($"Fehler beim Importieren: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetStatus("Bereit", false);
            }
        }

        private void OnBatchImport()
        {
            try
            {
                using var dialog = new OpenFileDialog
                {
                    Filter = "PDF Dateien (*.pdf)|*.pdf|Alle Dateien (*.*)|*.*",
                    Title = "Rechnungen auswählen",
                    Multiselect = true
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    BatchImportInvoices(dialog.FileNames);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Batch-Import");
                XtraMessageBox.Show($"Fehler beim Batch-Import: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BatchImportInvoices(string[] filePaths)
        {
            try
            {
                SetStatus("Batch-Import läuft...", true);

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
                        _logger.LogError(ex, "Fehler beim Importieren: {FilePath}", filePath);
                        errorCount++;
                    }
                }

                XtraMessageBox.Show($"Batch-Import abgeschlossen: {successCount} erfolgreich, {errorCount} fehlgeschlagen", "Batch-Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadInvoices();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Batch-Import");
                XtraMessageBox.Show($"Fehler beim Batch-Import: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetStatus("Bereit", false);
            }
        }

        private void OnExport()
        {
            try
            {
                using var dialog = new SaveFileDialog
                {
                    Filter = "CSV Dateien (*.csv)|*.csv|Excel Dateien (*.xlsx)|*.xlsx|Alle Dateien (*.*)|*.*",
                    Title = "Rechnungen exportieren"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ExportInvoices(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Exportieren");
                XtraMessageBox.Show($"Fehler beim Exportieren: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportInvoices(string filePath)
        {
            try
            {
                SetStatus("Rechnungen werden exportiert...", true);

                // TODO: Export-Funktionalität implementieren
                XtraMessageBox.Show($"Export nach {filePath} noch nicht implementiert", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);

                SetStatus("Bereit", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Exportieren: {FilePath}", filePath);
                XtraMessageBox.Show($"Fehler beim Exportieren: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Bereit", false);
            }
        }

        private void OnTrainModels()
        {
            try
            {
                // TODO: TrainingForm implementieren
                XtraMessageBox.Show("TrainingForm noch nicht implementiert", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Öffnen des Trainingsformulars");
                XtraMessageBox.Show($"Fehler beim Öffnen des Trainingsformulars: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSettings()
        {
            try
            {
                // TODO: SettingsForm implementieren
                XtraMessageBox.Show("SettingsForm noch nicht implementiert", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Öffnen der Einstellungen");
                XtraMessageBox.Show($"Fehler beim Öffnen der Einstellungen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnViewInvoice()
        {
            if (_selectedInvoice != null)
            {
                try
                {
                    // TODO: ReviewForm implementieren
                    XtraMessageBox.Show("ReviewForm noch nicht implementiert", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Öffnen der Rechnungsansicht");
                    XtraMessageBox.Show($"Fehler beim Öffnen der Rechnungsansicht: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnEditInvoice()
        {
            if (_selectedInvoice != null)
            {
                try
                {
                    // TODO: ReviewForm mit Edit-Mode implementieren
                    XtraMessageBox.Show("ReviewForm noch nicht implementiert", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Öffnen der Rechnungsbearbeitung");
                    XtraMessageBox.Show($"Fehler beim Öffnen der Rechnungsbearbeitung: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnDeleteInvoice()
        {
            if (_selectedInvoice != null)
            {
                var result = XtraMessageBox.Show($"Möchten Sie die Rechnung {_selectedInvoice.InvoiceNumber} wirklich löschen?", "Löschen bestätigen", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                SetStatus("Rechnung wird gelöscht...", true);

                await _invoiceRepository.DeleteAsync(invoiceId);
                LoadInvoices();

                SetStatus("Rechnung gelöscht", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Löschen: {InvoiceId}", invoiceId);
                XtraMessageBox.Show($"Fehler beim Löschen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Bereit", false);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Cleanup if needed
                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Schließen");
                XtraMessageBox.Show($"Fehler beim Schließen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}