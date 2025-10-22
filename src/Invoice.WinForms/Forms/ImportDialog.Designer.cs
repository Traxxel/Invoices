namespace Invoice.WinForms.Forms
{
    partial class ImportDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.fileListBoxControl = new DevExpress.XtraEditors.ListBoxControl();
            this.addFilesButton = new DevExpress.XtraEditors.SimpleButton();
            this.removeFileButton = new DevExpress.XtraEditors.SimpleButton();
            this.clearAllButton = new DevExpress.XtraEditors.SimpleButton();
            this.checkDuplicatesCheckEdit = new DevExpress.XtraEditors.CheckEdit();
            this.requireManualReviewCheckEdit = new DevExpress.XtraEditors.CheckEdit();
            this.confidenceThresholdSpinEdit = new DevExpress.XtraEditors.SpinEdit();
            this.importButton = new DevExpress.XtraEditors.SimpleButton();
            this.cancelButton = new DevExpress.XtraEditors.SimpleButton();
            this.progressBarControl = new DevExpress.XtraEditors.ProgressBarControl();
            this.statusLabel = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.searchComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            ((System.ComponentModel.ISupportInitialize)(this.fileListBoxControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkDuplicatesCheckEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.requireManualReviewCheckEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.confidenceThresholdSpinEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.searchComboBoxEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // fileListBoxControl
            // 
            this.fileListBoxControl.Location = new System.Drawing.Point(12, 42);
            this.fileListBoxControl.Name = "fileListBoxControl";
            this.fileListBoxControl.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.fileListBoxControl.Size = new System.Drawing.Size(456, 250);
            this.fileListBoxControl.TabIndex = 0;
            // 
            // addFilesButton
            // 
            this.addFilesButton.Location = new System.Drawing.Point(474, 42);
            this.addFilesButton.Name = "addFilesButton";
            this.addFilesButton.Size = new System.Drawing.Size(120, 27);
            this.addFilesButton.TabIndex = 1;
            this.addFilesButton.Text = "Dateien hinzufügen";
            this.addFilesButton.Click += new System.EventHandler(this.addFilesButton_Click);
            // 
            // removeFileButton
            // 
            this.removeFileButton.Location = new System.Drawing.Point(474, 75);
            this.removeFileButton.Name = "removeFileButton";
            this.removeFileButton.Size = new System.Drawing.Size(120, 27);
            this.removeFileButton.TabIndex = 2;
            this.removeFileButton.Text = "Entfernen";
            this.removeFileButton.Click += new System.EventHandler(this.removeFileButton_Click);
            // 
            // clearAllButton
            // 
            this.clearAllButton.Location = new System.Drawing.Point(474, 108);
            this.clearAllButton.Name = "clearAllButton";
            this.clearAllButton.Size = new System.Drawing.Size(120, 27);
            this.clearAllButton.TabIndex = 3;
            this.clearAllButton.Text = "Alle löschen";
            this.clearAllButton.Click += new System.EventHandler(this.clearAllButton_Click);
            // 
            // checkDuplicatesCheckEdit
            // 
            this.checkDuplicatesCheckEdit.EditValue = true;
            this.checkDuplicatesCheckEdit.Location = new System.Drawing.Point(12, 10);
            this.checkDuplicatesCheckEdit.Name = "checkDuplicatesCheckEdit";
            this.checkDuplicatesCheckEdit.Properties.Caption = "Auf Duplikate prüfen";
            this.checkDuplicatesCheckEdit.Size = new System.Drawing.Size(200, 24);
            this.checkDuplicatesCheckEdit.TabIndex = 0;
            // 
            // requireManualReviewCheckEdit
            // 
            this.requireManualReviewCheckEdit.Location = new System.Drawing.Point(12, 40);
            this.requireManualReviewCheckEdit.Name = "requireManualReviewCheckEdit";
            this.requireManualReviewCheckEdit.Properties.Caption = "Manuelle Überprüfung bei niedriger Konfidenz";
            this.requireManualReviewCheckEdit.Size = new System.Drawing.Size(300, 24);
            this.requireManualReviewCheckEdit.TabIndex = 1;
            // 
            // confidenceThresholdSpinEdit
            // 
            this.confidenceThresholdSpinEdit.EditValue = new decimal(new int[] {
            7,
            0,
            0,
            65536});
            this.confidenceThresholdSpinEdit.Location = new System.Drawing.Point(474, 38);
            this.confidenceThresholdSpinEdit.Name = "confidenceThresholdSpinEdit";
            this.confidenceThresholdSpinEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.confidenceThresholdSpinEdit.Properties.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.confidenceThresholdSpinEdit.Properties.MaxValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.confidenceThresholdSpinEdit.Size = new System.Drawing.Size(120, 22);
            this.confidenceThresholdSpinEdit.TabIndex = 3;
            // 
            // importButton
            // 
            this.importButton.Enabled = false;
            this.importButton.Location = new System.Drawing.Point(420, 422);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(88, 27);
            this.importButton.TabIndex = 6;
            this.importButton.Text = "Importieren";
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(514, 422);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(88, 27);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // progressBarControl
            // 
            this.progressBarControl.Location = new System.Drawing.Point(12, 390);
            this.progressBarControl.Name = "progressBarControl";
            this.progressBarControl.Size = new System.Drawing.Size(582, 18);
            this.progressBarControl.TabIndex = 8;
            this.progressBarControl.Visible = false;
            // 
            // statusLabel
            // 
            this.statusLabel.Location = new System.Drawing.Point(12, 368);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(31, 16);
            this.statusLabel.TabIndex = 9;
            this.statusLabel.Text = "Bereit";
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 20);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(120, 16);
            this.labelControl1.TabIndex = 10;
            this.labelControl1.Text = "Zu importierende Dateien:";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(12, 303);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(57, 16);
            this.labelControl2.TabIndex = 11;
            this.labelControl2.Text = "Optionen:";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(330, 41);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(138, 16);
            this.labelControl3.TabIndex = 12;
            this.labelControl3.Text = "Konfidenz-Schwellenwert:";
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.checkDuplicatesCheckEdit);
            this.panelControl1.Controls.Add(this.labelControl3);
            this.panelControl1.Controls.Add(this.requireManualReviewCheckEdit);
            this.panelControl1.Controls.Add(this.confidenceThresholdSpinEdit);
            this.panelControl1.Location = new System.Drawing.Point(12, 321);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(606, 75);
            this.panelControl1.TabIndex = 13;
            // 
            // panelControl2
            // 
            this.panelControl2.Location = new System.Drawing.Point(0, 0);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(200, 100);
            this.panelControl2.TabIndex = 0;
            // 
            // searchComboBoxEdit
            // 
            this.searchComboBoxEdit.EditValue = "Alle";
            this.searchComboBoxEdit.Location = new System.Drawing.Point(0, 0);
            this.searchComboBoxEdit.Name = "searchComboBoxEdit";
            this.searchComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.searchComboBoxEdit.Size = new System.Drawing.Size(100, 22);
            this.searchComboBoxEdit.TabIndex = 0;
            this.searchComboBoxEdit.Visible = false;
            // 
            // ImportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(614, 461);
            this.Controls.Add(this.searchComboBoxEdit);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.progressBarControl);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.clearAllButton);
            this.Controls.Add(this.removeFileButton);
            this.Controls.Add(this.addFilesButton);
            this.Controls.Add(this.fileListBoxControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rechnungen importieren";
            ((System.ComponentModel.ISupportInitialize)(this.fileListBoxControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkDuplicatesCheckEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.requireManualReviewCheckEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.confidenceThresholdSpinEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchComboBoxEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.ListBoxControl fileListBoxControl;
        private DevExpress.XtraEditors.SimpleButton addFilesButton;
        private DevExpress.XtraEditors.SimpleButton removeFileButton;
        private DevExpress.XtraEditors.SimpleButton clearAllButton;
        private DevExpress.XtraEditors.CheckEdit checkDuplicatesCheckEdit;
        private DevExpress.XtraEditors.CheckEdit requireManualReviewCheckEdit;
        private DevExpress.XtraEditors.SpinEdit confidenceThresholdSpinEdit;
        private DevExpress.XtraEditors.SimpleButton importButton;
        private DevExpress.XtraEditors.SimpleButton cancelButton;
        private DevExpress.XtraEditors.ProgressBarControl progressBarControl;
        private DevExpress.XtraEditors.LabelControl statusLabel;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraEditors.ComboBoxEdit searchComboBoxEdit;
    }
}

