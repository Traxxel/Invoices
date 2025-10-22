# DevExpress-Komponenten Mapping für WinForms-Projekt

## Regel
**Alle UI-Komponenten im WinForms-Projekt MÜSSEN DevExpress-Komponenten verwenden!**

## Standard WinForms → DevExpress Mapping

### Forms & Controls
| Standard WinForms | DevExpress | Namespace |
|-------------------|------------|-----------|
| `Form` | `XtraForm` | `DevExpress.XtraEditors` |
| `UserControl` | `XtraUserControl` | `DevExpress.XtraEditors` |
| `Panel` | `PanelControl` | `DevExpress.XtraEditors` |
| `GroupBox` | `GroupControl` | `DevExpress.XtraEditors` |

### Buttons & Inputs
| Standard WinForms | DevExpress | Namespace |
|-------------------|------------|-----------|
| `Button` | `SimpleButton` | `DevExpress.XtraEditors` |
| `TextBox` | `TextEdit` | `DevExpress.XtraEditors` |
| `ComboBox` | `ComboBoxEdit` | `DevExpress.XtraEditors` |
| `CheckBox` | `CheckEdit` | `DevExpress.XtraEditors` |
| `RadioButton` | `CheckEdit` (mit RadioGroupStyle) | `DevExpress.XtraEditors` |
| `NumericUpDown` | `SpinEdit` | `DevExpress.XtraEditors` |
| `DateTimePicker` | `DateEdit` | `DevExpress.XtraEditors` |
| `TrackBar` | `TrackBarControl` | `DevExpress.XtraEditors` |

### Data Display
| Standard WinForms | DevExpress | Namespace |
|-------------------|------------|-----------|
| `DataGridView` | `GridControl` + `GridView` | `DevExpress.XtraGrid` |
| `ListBox` | `ListBoxControl` | `DevExpress.XtraEditors` |
| `ListView` | `GridControl` + `GridView` | `DevExpress.XtraGrid` |
| `TreeView` | `TreeList` | `DevExpress.XtraTreeList` |
| `ProgressBar` | `ProgressBarControl` | `DevExpress.XtraEditors` |

### Labels & Text
| Standard WinForms | DevExpress | Namespace |
|-------------------|------------|-----------|
| `Label` | `LabelControl` | `DevExpress.XtraEditors` |
| `RichTextBox` | `MemoEdit` (Multiline) | `DevExpress.XtraEditors` |

### Menus & Navigation
| Standard WinForms | DevExpress | Namespace |
|-------------------|------------|-----------|
| `MenuStrip` | `RibbonControl` | `DevExpress.XtraBars.Ribbon` |
| `ToolStrip` | `BarManager` + `Bar` | `DevExpress.XtraBars` |
| `StatusStrip` | `RibbonStatusBar` | `DevExpress.XtraBars.Ribbon` |
| `ContextMenuStrip` | `PopupMenu` | `DevExpress.XtraBars` |
| `TabControl` | `XtraTabControl` | `DevExpress.XtraTab` |

### Containers
| Standard WinForms | DevExpress | Namespace |
|-------------------|------------|-----------|
| `SplitContainer` | `SplitContainerControl` | `DevExpress.XtraEditors` |
| `FlowLayoutPanel` | `LayoutControl` | `DevExpress.XtraLayout` |
| `TableLayoutPanel` | `LayoutControl` | `DevExpress.XtraLayout` |

### Dialogs
| Standard WinForms | DevExpress | Namespace |
|-------------------|------------|-----------|
| `MessageBox` | `XtraMessageBox` | `DevExpress.XtraEditors` |
| `OpenFileDialog` | `XtraOpenFileDialog` (optional, standard OK) | `System.Windows.Forms` |
| `SaveFileDialog` | `XtraSaveFileDialog` (optional, standard OK) | `System.Windows.Forms` |
| `FolderBrowserDialog` | Standard (kein DevExpress-Äquivalent) | `System.Windows.Forms` |

### Spezielle Komponenten
| Standard WinForms | DevExpress | Namespace |
|-------------------|------------|-----------|
| PDF Viewer (keins) | `PdfViewer` | `DevExpress.XtraPdfViewer` |
| - | `GridControl` | `DevExpress.XtraGrid` |
| - | `RibbonControl` | `DevExpress.XtraBars.Ribbon` |
| - | `BarManager` | `DevExpress.XtraBars` |

## Wichtige Hinweise

### GridControl + GridView
```csharp
// Erstelle GridControl
var gridControl = new DevExpress.XtraGrid.GridControl();
var gridView = new DevExpress.XtraGrid.Views.Grid.GridView();

gridControl.MainView = gridView;
gridView.GridControl = gridControl;

// Wichtig: Editable = false für readonly
gridView.OptionsBehavior.Editable = false;

// AutoFilter Row
gridView.OptionsView.ShowAutoFilterRow = true;

// Keine Gruppierung
gridView.OptionsView.ShowGroupPanel = false;
```

### RibbonControl
```csharp
// Form muss von RibbonForm erben
public partial class frmMain : DevExpress.XtraBars.Ribbon.RibbonForm
{
    // Ribbon hinzufügen
    this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
    
    // Pages und Groups hinzufügen
    var ribbonPage = new DevExpress.XtraBars.Ribbon.RibbonPage();
    var ribbonPageGroup = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
    
    // Buttons hinzufügen
    var barButton = new DevExpress.XtraBars.BarButtonItem();
    barButton.Caption = "Text";
    barButton.ItemClick += handler;
    
    // Ribbon Properties setzen
    this.Ribbon = ribbon;
    this.StatusBar = ribbonStatusBar;
}
```

### MessageBox
```csharp
// Statt MessageBox.Show():
XtraMessageBox.Show("Text", "Titel", MessageBoxButtons.OK, MessageBoxIcon.Information);
```

### TextEdit
```csharp
// TextBox.Text → TextEdit.Text (gleich)
// Aber Properties über .Properties:
textEdit.Properties.ReadOnly = true;
textEdit.Properties.MaxLength = 100;
```

### ComboBoxEdit
```csharp
// Items über Properties.Items:
comboBoxEdit.Properties.Items.AddRange(new[] { "Item1", "Item2" });
comboBoxEdit.EditValue = "Item1"; // Statt SelectedItem

// DropDownStyle über Properties.TextEditStyle:
comboBoxEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
```

### CheckEdit
```csharp
// CheckBox.Checked → CheckEdit.Checked (gleich)
// CheckBox.Text → CheckEdit.Text (gleich)
```

### SpinEdit (NumericUpDown)
```csharp
spinEdit.Properties.MinValue = 0;
spinEdit.Properties.MaxValue = 100;
spinEdit.Properties.Increment = 1;
spinEdit.Value = 50;
```

## Anwendung auf Konzepte

Alle Konzepte 34-40 müssen entsprechend angepasst werden:
- **34-ImportDialog**: Form → XtraForm, Buttons → SimpleButton, etc.
- **35-BatchImportDialog**: ListView → GridControl+GridView
- **36-ReviewForm**: SplitContainer → SplitContainerControl
- **37-PDF-Viewer-Control**: UserControl → XtraUserControl, PdfViewer verwenden
- **38-Field-Editor-Control**: Alle Controls anpassen
- **39-TrainingForm**: TabControl → XtraTabControl, DataGridView → GridControl
- **40-SettingsForm**: TabControl → XtraTabControl, alle Inputs anpassen

## Beispiel-Transformation

### Vorher (Standard WinForms):
```csharp
public partial class ImportDialog : Form
{
    private Button _importButton;
    private TextBox _filePathTextBox;
    private ListBox _fileListBox;
    
    _importButton = new Button
    {
        Text = "Import",
        Width = 100
    };
}
```

### Nachher (DevExpress):
```csharp
public partial class ImportDialog : XtraForm
{
    private SimpleButton _importButton;
    private TextEdit _filePathTextEdit;
    private ListBoxControl _fileListBoxControl;
    
    _importButton = new SimpleButton
    {
        Text = "Import",
        Width = 100
    };
}
```

