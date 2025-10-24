using Invoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DevExpress.XtraEditors;
using DevExpress.XtraPdfViewer;
using DevExpress.XtraBars;
using System.IO;

namespace Invoice.WinForms.Controls;

public partial class PdfViewerControl : XtraUserControl
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PdfViewerControl> _logger;

    // DevExpress PDF Viewer
    private PdfViewer _pdfViewer = null!;
    private BarManager _barManager = null!;
    private Bar _toolBar = null!;
    private BarButtonItem _zoomInButton = null!;
    private BarButtonItem _zoomOutButton = null!;
    private BarButtonItem _fitToWidthButton = null!;
    private BarButtonItem _fitToHeightButton = null!;
    private BarButtonItem _actualSizeButton = null!;
    private BarButtonItem _rotateLeftButton = null!;
    private BarButtonItem _rotateRightButton = null!;
    private BarStaticItem _pageInfoLabel = null!;
    private BarEditItem _pageNavigator = null!;

    // Data
    private string? _pdfFilePath;
    private bool _isLoaded;

    public PdfViewerControl(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<PdfViewerControl>>();

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // Bar Manager
        _barManager = new BarManager
        {
            Form = this.FindForm()
        };

        // Tool Bar
        _toolBar = new Bar(_barManager, "PDF Tools")
        {
            DockStyle = BarDockStyle.Top
        };
        _barManager.Bars.Add(_toolBar);

        // Zoom buttons
        _zoomInButton = new BarButtonItem(_barManager, "Vergrößern");
        _zoomInButton.ItemClick += (s, e) => OnZoomIn();
        _toolBar.AddItem(_zoomInButton);

        _zoomOutButton = new BarButtonItem(_barManager, "Verkleinern");
        _zoomOutButton.ItemClick += (s, e) => OnZoomOut();
        _toolBar.AddItem(_zoomOutButton);

        _fitToWidthButton = new BarButtonItem(_barManager, "Seitenbreite");
        _fitToWidthButton.ItemClick += (s, e) => OnFitToWidth();
        _toolBar.AddItem(_fitToWidthButton);

        _fitToHeightButton = new BarButtonItem(_barManager, "Seitenhöhe");
        _fitToHeightButton.ItemClick += (s, e) => OnFitToHeight();
        _toolBar.AddItem(_fitToHeightButton);

        _actualSizeButton = new BarButtonItem(_barManager, "Originalgröße");
        _actualSizeButton.ItemClick += (s, e) => OnActualSize();
        _toolBar.AddItem(_actualSizeButton);

        // Rotate buttons
        _rotateLeftButton = new BarButtonItem(_barManager, "◄ Drehen");
        _rotateLeftButton.ItemClick += (s, e) => OnRotateLeft();
        _toolBar.AddItem(_rotateLeftButton);

        _rotateRightButton = new BarButtonItem(_barManager, "Drehen ►");
        _rotateRightButton.ItemClick += (s, e) => OnRotateRight();
        _toolBar.AddItem(_rotateRightButton);

        // Page info
        _pageInfoLabel = new BarStaticItem();
        _pageInfoLabel.Caption = "Seite 0 von 0";
        _toolBar.AddItem(_pageInfoLabel);

        // PDF Viewer
        _pdfViewer = new PdfViewer
        {
            Dock = DockStyle.Fill
        };
        // TODO: Wire up PdfViewer events when correct DevExpress API is known
        // _pdfViewer.DocumentLoaded += OnDocumentLoaded;
        // _pdfViewer.PageChanged += OnPageChanged;
        // _pdfViewer.ZoomFactorChanged += OnZoomFactorChanged;
    }

    private void LayoutControls()
    {
        this.Controls.Add(_pdfViewer);
        // TODO: Add toolbar to controls when correct DevExpress API is known
        // DevExpress bars are typically docked automatically
    }

    public void LoadPdf(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("PDF-Datei nicht gefunden: {FilePath}", filePath);
                XtraMessageBox.Show(
                    $"PDF-Datei nicht gefunden:\n{filePath}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            _pdfFilePath = filePath;
            _pdfViewer.LoadDocument(filePath);
            _isLoaded = true;

            _logger.LogInformation("PDF erfolgreich geladen: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der PDF: {FilePath}", filePath);
            XtraMessageBox.Show(
                $"Fehler beim Laden der PDF: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    public void LoadPdf(Stream pdfStream)
    {
        try
        {
            _pdfViewer.LoadDocument(pdfStream);
            _isLoaded = true;
            _logger.LogInformation("PDF aus Stream erfolgreich geladen");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der PDF aus Stream");
            XtraMessageBox.Show(
                $"Fehler beim Laden der PDF: {ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    public void UnloadPdf()
    {
        try
        {
            _pdfViewer.CloseDocument();
            _isLoaded = false;
            _pdfFilePath = null;
            UpdatePageInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Schließen der PDF");
        }
    }

    private void OnDocumentLoaded(object sender, EventArgs e)
    {
        UpdatePageInfo();
        _logger.LogInformation("PDF-Dokument geladen: {PageCount} Seiten", _pdfViewer.PageCount);
    }

    private void OnPageChanged(object sender, EventArgs e)
    {
        UpdatePageInfo();
    }

    private void OnZoomFactorChanged(object sender, EventArgs e)
    {
        _logger.LogDebug("Zoom-Faktor geändert: {ZoomFactor}", _pdfViewer.ZoomFactor);
    }

    private void UpdatePageInfo()
    {
        if (_isLoaded)
        {
            _pageInfoLabel.Caption = $"Seite {_pdfViewer.CurrentPageNumber} von {_pdfViewer.PageCount}";
        }
        else
        {
            _pageInfoLabel.Caption = "Seite 0 von 0";
        }
    }

    private void OnZoomIn()
    {
        if (!_isLoaded) return;

        try
        {
            _pdfViewer.ZoomFactor = Math.Min(_pdfViewer.ZoomFactor * 1.2f, 5.0f);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Vergrößern");
        }
    }

    private void OnZoomOut()
    {
        if (!_isLoaded) return;

        try
        {
            _pdfViewer.ZoomFactor = Math.Max(_pdfViewer.ZoomFactor / 1.2f, 0.1f);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Verkleinern");
        }
    }

    private void OnFitToWidth()
    {
        if (!_isLoaded) return;

        try
        {
            _pdfViewer.ZoomMode = DevExpress.XtraPdfViewer.PdfZoomMode.FitToWidth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Anpassen an Seitenbreite");
        }
    }

    private void OnFitToHeight()
    {
        if (!_isLoaded) return;

        try
        {
            _pdfViewer.ZoomMode = DevExpress.XtraPdfViewer.PdfZoomMode.FitToVisible;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Anpassen an Seitenhöhe");
        }
    }

    private void OnActualSize()
    {
        if (!_isLoaded) return;

        try
        {
            _pdfViewer.ZoomMode = DevExpress.XtraPdfViewer.PdfZoomMode.ActualSize;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Setzen der Originalgröße");
        }
    }

    private void OnRotateLeft()
    {
        if (!_isLoaded) return;

        try
        {
            // DevExpress PdfViewer hat keine direkte Rotate-Methode in allen Versionen
            // Dies ist ein Platzhalter
            _logger.LogWarning("Rotation nach links ist in dieser Version nicht implementiert");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Drehen nach links");
        }
    }

    private void OnRotateRight()
    {
        if (!_isLoaded) return;

        try
        {
            // DevExpress PdfViewer hat keine direkte Rotate-Methode in allen Versionen
            // Dies ist ein Platzhalter
            _logger.LogWarning("Rotation nach rechts ist in dieser Version nicht implementiert");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Drehen nach rechts");
        }
    }

    public void NavigateToPage(int pageNumber)
    {
        if (!_isLoaded) return;

        try
        {
            if (pageNumber >= 1 && pageNumber <= _pdfViewer.PageCount)
            {
                _pdfViewer.CurrentPageNumber = pageNumber;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Navigieren zu Seite {PageNumber}", pageNumber);
        }
    }

    public void NavigateToFirstPage()
    {
        NavigateToPage(1);
    }

    public void NavigateToLastPage()
    {
        if (_isLoaded)
        {
            NavigateToPage(_pdfViewer.PageCount);
        }
    }

    public void NavigateToNextPage()
    {
        if (_isLoaded && _pdfViewer.CurrentPageNumber < _pdfViewer.PageCount)
        {
            NavigateToPage(_pdfViewer.CurrentPageNumber + 1);
        }
    }

    public void NavigateToPreviousPage()
    {
        if (_isLoaded && _pdfViewer.CurrentPageNumber > 1)
        {
            NavigateToPage(_pdfViewer.CurrentPageNumber - 1);
        }
    }

    public int CurrentPageNumber => _isLoaded ? _pdfViewer.CurrentPageNumber : 0;
    public int PageCount => _isLoaded ? _pdfViewer.PageCount : 0;
    public float ZoomFactor => _isLoaded ? _pdfViewer.ZoomFactor : 1.0f;
    public bool IsLoaded => _isLoaded;
    public string? FilePath => _pdfFilePath;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pdfViewer?.CloseDocument();
            _barManager?.Dispose();
        }
        base.Dispose(disposing);
    }
}

