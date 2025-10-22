namespace Invoice.WinForms
{
    partial class frmMain
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
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.barButtonImport = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonBatchImport = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonExport = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonTrain = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonSettings = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonRefresh = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonDelete = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonEdit = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonView = new DevExpress.XtraBars.BarButtonItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroupActions = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGroupData = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.statusLabel = new DevExpress.XtraBars.BarStaticItem();
            this.progressBarControl = new DevExpress.XtraEditors.ProgressBarControl();
            this.gridControl = new DevExpress.XtraGrid.GridControl();
            this.gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.searchPanel = new DevExpress.XtraEditors.PanelControl();
            this.searchButton = new DevExpress.XtraEditors.SimpleButton();
            this.searchTextEdit = new DevExpress.XtraEditors.TextEdit();
            this.searchComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.searchLabel = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchPanel)).BeginInit();
            this.searchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.searchTextEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchComboBoxEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.barButtonImport,
            this.barButtonBatchImport,
            this.barButtonExport,
            this.barButtonTrain,
            this.barButtonSettings,
            this.barButtonRefresh,
            this.barButtonDelete,
            this.barButtonEdit,
            this.barButtonView,
            this.statusLabel});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.MaxItemId = 12;
            this.ribbon.Name = "ribbon";
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            this.ribbon.Size = new System.Drawing.Size(1392, 193);
            this.ribbon.StatusBar = this.ribbonStatusBar;
            // 
            // barButtonImport
            // 
            this.barButtonImport.Caption = "Importieren";
            this.barButtonImport.Id = 1;
            this.barButtonImport.Name = "barButtonImport";
            this.barButtonImport.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonImport_ItemClick);
            // 
            // barButtonBatchImport
            // 
            this.barButtonBatchImport.Caption = "Batch-Import";
            this.barButtonBatchImport.Id = 2;
            this.barButtonBatchImport.Name = "barButtonBatchImport";
            this.barButtonBatchImport.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonBatchImport_ItemClick);
            // 
            // barButtonExport
            // 
            this.barButtonExport.Caption = "Exportieren";
            this.barButtonExport.Id = 3;
            this.barButtonExport.Name = "barButtonExport";
            this.barButtonExport.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonExport_ItemClick);
            // 
            // barButtonTrain
            // 
            this.barButtonTrain.Caption = "Training";
            this.barButtonTrain.Id = 4;
            this.barButtonTrain.Name = "barButtonTrain";
            this.barButtonTrain.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonTrain_ItemClick);
            // 
            // barButtonSettings
            // 
            this.barButtonSettings.Caption = "Einstellungen";
            this.barButtonSettings.Id = 5;
            this.barButtonSettings.Name = "barButtonSettings";
            this.barButtonSettings.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonSettings_ItemClick);
            // 
            // barButtonRefresh
            // 
            this.barButtonRefresh.Caption = "Aktualisieren";
            this.barButtonRefresh.Id = 6;
            this.barButtonRefresh.Name = "barButtonRefresh";
            this.barButtonRefresh.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonRefresh_ItemClick);
            // 
            // barButtonDelete
            // 
            this.barButtonDelete.Caption = "Löschen";
            this.barButtonDelete.Id = 7;
            this.barButtonDelete.Name = "barButtonDelete";
            this.barButtonDelete.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonDelete_ItemClick);
            // 
            // barButtonEdit
            // 
            this.barButtonEdit.Caption = "Bearbeiten";
            this.barButtonEdit.Id = 8;
            this.barButtonEdit.Name = "barButtonEdit";
            this.barButtonEdit.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonEdit_ItemClick);
            // 
            // barButtonView
            // 
            this.barButtonView.Caption = "Anzeigen";
            this.barButtonView.Id = 9;
            this.barButtonView.Name = "barButtonView";
            this.barButtonView.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonView_ItemClick);
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroupActions,
            this.ribbonPageGroupData});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "Start";
            // 
            // ribbonPageGroupActions
            // 
            this.ribbonPageGroupActions.ItemLinks.Add(this.barButtonImport);
            this.ribbonPageGroupActions.ItemLinks.Add(this.barButtonBatchImport);
            this.ribbonPageGroupActions.ItemLinks.Add(this.barButtonExport);
            this.ribbonPageGroupActions.ItemLinks.Add(this.barButtonTrain);
            this.ribbonPageGroupActions.ItemLinks.Add(this.barButtonSettings);
            this.ribbonPageGroupActions.Name = "ribbonPageGroupActions";
            this.ribbonPageGroupActions.Text = "Aktionen";
            // 
            // ribbonPageGroupData
            // 
            this.ribbonPageGroupData.ItemLinks.Add(this.barButtonView);
            this.ribbonPageGroupData.ItemLinks.Add(this.barButtonEdit);
            this.ribbonPageGroupData.ItemLinks.Add(this.barButtonDelete);
            this.ribbonPageGroupData.ItemLinks.Add(this.barButtonRefresh);
            this.ribbonPageGroupData.Name = "ribbonPageGroupData";
            this.ribbonPageGroupData.Text = "Daten";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.ItemLinks.Add(this.statusLabel);
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 802);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1392, 30);
            // 
            // statusLabel
            // 
            this.statusLabel.Caption = "Bereit";
            this.statusLabel.Id = 10;
            this.statusLabel.Name = "statusLabel";
            // 
            // progressBarControl
            // 
            this.progressBarControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarControl.Location = new System.Drawing.Point(1142, 807);
            this.progressBarControl.MenuManager = this.ribbon;
            this.progressBarControl.Name = "progressBarControl";
            this.progressBarControl.Size = new System.Drawing.Size(238, 18);
            this.progressBarControl.TabIndex = 2;
            this.progressBarControl.Visible = false;
            // 
            // gridControl
            // 
            this.gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl.Location = new System.Drawing.Point(0, 243);
            this.gridControl.MainView = this.gridView;
            this.gridControl.MenuManager = this.ribbon;
            this.gridControl.Name = "gridControl";
            this.gridControl.Size = new System.Drawing.Size(1392, 559);
            this.gridControl.TabIndex = 3;
            this.gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
            // 
            // gridView
            // 
            this.gridView.GridControl = this.gridControl;
            this.gridView.Name = "gridView";
            this.gridView.OptionsBehavior.Editable = false;
            this.gridView.OptionsView.ShowAutoFilterRow = true;
            this.gridView.OptionsView.ShowGroupPanel = false;
            this.gridView.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridView_FocusedRowChanged);
            this.gridView.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(this.gridView_RowStyle);
            this.gridView.DoubleClick += new System.EventHandler(this.gridView_DoubleClick);
            // 
            // searchPanel
            // 
            this.searchPanel.Controls.Add(this.searchLabel);
            this.searchPanel.Controls.Add(this.searchComboBoxEdit);
            this.searchPanel.Controls.Add(this.searchTextEdit);
            this.searchPanel.Controls.Add(this.searchButton);
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.Location = new System.Drawing.Point(0, 193);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(1392, 50);
            this.searchPanel.TabIndex = 4;
            // 
            // searchButton
            // 
            this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchButton.Location = new System.Drawing.Point(1292, 12);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(88, 27);
            this.searchButton.TabIndex = 0;
            this.searchButton.Text = "Suchen";
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // searchTextEdit
            // 
            this.searchTextEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTextEdit.Location = new System.Drawing.Point(1036, 14);
            this.searchTextEdit.MenuManager = this.ribbon;
            this.searchTextEdit.Name = "searchTextEdit";
            this.searchTextEdit.Size = new System.Drawing.Size(250, 22);
            this.searchTextEdit.TabIndex = 1;
            this.searchTextEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.searchTextEdit_KeyPress);
            // 
            // searchComboBoxEdit
            // 
            this.searchComboBoxEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchComboBoxEdit.EditValue = "Alle";
            this.searchComboBoxEdit.Location = new System.Drawing.Point(856, 14);
            this.searchComboBoxEdit.MenuManager = this.ribbon;
            this.searchComboBoxEdit.Name = "searchComboBoxEdit";
            this.searchComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.searchComboBoxEdit.Properties.Items.AddRange(new object[] {
            "Alle",
            "Rechnungsnummer",
            "Aussteller",
            "Datum",
            "Betrag"});
            this.searchComboBoxEdit.Size = new System.Drawing.Size(174, 22);
            this.searchComboBoxEdit.TabIndex = 2;
            // 
            // searchLabel
            // 
            this.searchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchLabel.Location = new System.Drawing.Point(805, 17);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(45, 16);
            this.searchLabel.TabIndex = 3;
            this.searchLabel.Text = "Suchen:";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1392, 832);
            this.Controls.Add(this.gridControl);
            this.Controls.Add(this.searchPanel);
            this.Controls.Add(this.progressBarControl);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbon);
            this.Name = "frmMain";
            this.Ribbon = this.ribbon;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "Invoice Reader";
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchPanel)).EndInit();
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.searchTextEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchComboBoxEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupActions;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupData;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.BarButtonItem barButtonImport;
        private DevExpress.XtraBars.BarButtonItem barButtonBatchImport;
        private DevExpress.XtraBars.BarButtonItem barButtonExport;
        private DevExpress.XtraBars.BarButtonItem barButtonTrain;
        private DevExpress.XtraBars.BarButtonItem barButtonSettings;
        private DevExpress.XtraBars.BarButtonItem barButtonRefresh;
        private DevExpress.XtraBars.BarButtonItem barButtonDelete;
        private DevExpress.XtraBars.BarButtonItem barButtonEdit;
        private DevExpress.XtraBars.BarButtonItem barButtonView;
        private DevExpress.XtraBars.BarStaticItem statusLabel;
        private DevExpress.XtraEditors.ProgressBarControl progressBarControl;
        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;
        private DevExpress.XtraEditors.PanelControl searchPanel;
        private DevExpress.XtraEditors.SimpleButton searchButton;
        private DevExpress.XtraEditors.TextEdit searchTextEdit;
        private DevExpress.XtraEditors.ComboBoxEdit searchComboBoxEdit;
        private DevExpress.XtraEditors.LabelControl searchLabel;
    }
}