using Invoice.Application.Interfaces;
using Invoice.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Invoice.WinForms.Forms
{
    public partial class ImportDialog : XtraForm
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ImportDialog> _logger;
        private readonly IImportInvoiceUseCase _importInvoiceUseCase;

        // Data
        private List<string> _selectedFiles;
        private CancellationTokenSource? _cancellationTokenSource;

        public int SuccessCount { get; private set; }
        public int ErrorCount { get; private set; }

        public ImportDialog(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<ImportDialog>>();
            _importInvoiceUseCase = serviceProvider.GetRequiredService<IImportInvoiceUseCase>();

            _selectedFiles = new List<string>();

            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            SuccessCount = 0;
            ErrorCount = 0;

            // Initialize search combo
            searchComboBoxEdit.Properties.Items.Clear();
            searchComboBoxEdit.Properties.Items.AddRange(new[] { "Alle", "Rechnungsnummer", "Aussteller", "Datum", "Betrag" });
            searchComboBoxEdit.SelectedIndex = 0;
        }

        private void addFilesButton_Click(object sender, EventArgs e)
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
                    foreach (var file in dialog.FileNames)
                    {
                        if (!_selectedFiles.Contains(file))
                        {
                            _selectedFiles.Add(file);
                        }
                    }
                    RefreshFileList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Hinzufügen von Dateien");
                XtraMessageBox.Show($"Fehler beim Hinzufügen von Dateien: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void removeFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedIndices = fileListBoxControl.SelectedIndices;
                if (selectedIndices.Count > 0)
                {
                    // Rückwärts durchlaufen um Index-Probleme zu vermeiden
                    for (int i = selectedIndices.Count - 1; i >= 0; i--)
                    {
                        var index = selectedIndices[i];
                        if (index < _selectedFiles.Count)
                        {
                            _selectedFiles.RemoveAt(index);
                        }
                    }
                    RefreshFileList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Entfernen von Dateien");
                XtraMessageBox.Show($"Fehler beim Entfernen von Dateien: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clearAllButton_Click(object sender, EventArgs e)
        {
            try
            {
                _selectedFiles.Clear();
                RefreshFileList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Löschen der Dateiliste");
                XtraMessageBox.Show($"Fehler beim Löschen der Dateiliste: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshFileList()
        {
            fileListBoxControl.Items.Clear();
            foreach (var file in _selectedFiles)
            {
                fileListBoxControl.Items.Add(Path.GetFileName(file));
            }
            importButton.Enabled = _selectedFiles.Count > 0;
            statusLabel.Text = $"{_selectedFiles.Count} Datei(en) ausgewählt";
        }

        private async void importButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedFiles.Count == 0)
                {
                    XtraMessageBox.Show("Bitte wählen Sie mindestens eine Datei aus.", "Keine Dateien", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // UI für Import vorbereiten
                importButton.Enabled = false;
                addFilesButton.Enabled = false;
                removeFileButton.Enabled = false;
                clearAllButton.Enabled = false;
                
                progressBarControl.Properties.Minimum = 0;
                progressBarControl.Properties.Maximum = _selectedFiles.Count;
                progressBarControl.EditValue = 0;
                progressBarControl.Visible = true;

                SuccessCount = 0;
                ErrorCount = 0;

                // Optionen aus UI lesen
                var options = new ImportOptions(
                    UseMLExtraction: true,
                    RequireManualReview: requireManualReviewCheckEdit.Checked,
                    ConfidenceThreshold: (float)confidenceThresholdSpinEdit.Value,
                    CheckForDuplicates: checkDuplicatesCheckEdit.Checked,
                    AutoSave: true,
                    CreateBackup: true,
                    ModelVersion: "1.0",
                    CustomSettings: new Dictionary<string, object>()
                );

                // Import durchführen
                foreach (var filePath in _selectedFiles)
                {
                    try
                    {
                        SetStatus($"Importiere {Path.GetFileName(filePath)}...");

                        var result = await _importInvoiceUseCase.ExecuteAsync(filePath);

                        if (result.Success)
                        {
                            SuccessCount++;
                            _logger.LogInformation("Import erfolgreich: {FileName}", Path.GetFileName(filePath));
                        }
                        else
                        {
                            ErrorCount++;
                            _logger.LogWarning("Import fehlgeschlagen: {FileName} - {Message}", Path.GetFileName(filePath), result.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorCount++;
                        _logger.LogError(ex, "Fehler beim Importieren: {FilePath}", filePath);
                    }

                    progressBarControl.EditValue = SuccessCount + ErrorCount;
                    System.Windows.Forms.Application.DoEvents();

                    // Bei Cancellation abbrechen
                    if (_cancellationTokenSource?.IsCancellationRequested == true)
                    {
                        break;
                    }
                }

                SetStatus($"Import abgeschlossen: {SuccessCount} erfolgreich, {ErrorCount} fehlgeschlagen");

                // Dialog schließen wenn mindestens ein Import erfolgreich war
                if (SuccessCount > 0)
                {
                    XtraMessageBox.Show(
                        $"Import abgeschlossen:\n{SuccessCount} Rechnung(en) erfolgreich importiert\n{ErrorCount} Fehler",
                        "Import abgeschlossen",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show(
                        $"Alle Importe sind fehlgeschlagen.\n{ErrorCount} Fehler",
                        "Import fehlgeschlagen",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Import-Vorgang");
                XtraMessageBox.Show($"Fehler beim Import-Vorgang: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // UI wiederherstellen
                importButton.Enabled = true;
                addFilesButton.Enabled = true;
                removeFileButton.Enabled = true;
                clearAllButton.Enabled = true;
                progressBarControl.Visible = false;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SetStatus(string message)
        {
            statusLabel.Text = message;
            System.Windows.Forms.Application.DoEvents();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            base.OnFormClosing(e);
        }
    }
}

