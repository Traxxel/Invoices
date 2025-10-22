# Aufgabe 37: PDF-Viewer Control (PdfPig Rendering)

## Ziel

PDF-Viewer Control für die Anzeige von PDF-Dateien mit PdfPig Rendering und Bounding Box Highlighting.

## 1. PDF-Viewer Control Interface

**Datei:** `src/Invoice.WinForms/Controls/PdfViewerControl.cs`

```csharp
using Invoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Graphics;

namespace Invoice.WinForms.Controls;

public partial class PdfViewerControl : UserControl
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PdfViewerControl> _logger;
    private readonly IPdfParserService _pdfParserService;

    // UI Controls
    private Panel _pdfPanel;
    private VScrollBar _verticalScrollBar;
    private HScrollBar _horizontalScrollBar;
    private ToolStrip _toolStrip;
    private ToolStripButton _zoomInButton;
    private ToolStripButton _zoomOutButton;
    private ToolStripButton _fitToWidthButton;
    private ToolStripButton _fitToHeightButton;
    private ToolStripButton _actualSizeButton;
    private ToolStripComboBox _pageComboBox;
    private ToolStripLabel _pageLabel;

    // Data
    private string? _pdfFilePath;
    private Document? _pdfDocument;
    private List<PageInfo> _pages;
    private int _currentPage;
    private float _zoomFactor;
    private Point _scrollPosition;
    private List<BoundingBox> _highlightedBoxes;
    private Color _highlightColor;

    public PdfViewerControl(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<PdfViewerControl>>();
        _pdfParserService = serviceProvider.GetRequiredService<IPdfParserService>();

        InitializeComponent();
        InitializeData();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.White;

        CreateControls();
        LayoutControls();
    }

    private void CreateControls()
    {
        // Tool strip
        _toolStrip = new ToolStrip();

        _zoomInButton = new ToolStripButton("Zoom In", null, OnZoomIn);
        _zoomOutButton = new ToolStripButton("Zoom Out", null, OnZoomOut);
        _fitToWidthButton = new ToolStripButton("Fit Width", null, OnFitToWidth);
        _fitToHeightButton = new ToolStripButton("Fit Height", null, OnFitToHeight);
        _actualSizeButton = new ToolStripButton("Actual Size", null, OnActualSize);

        _pageComboBox = new ToolStripComboBox();
        _pageComboBox.SelectedIndexChanged += OnPageChanged;

        _pageLabel = new ToolStripLabel("Page 1 of 1");

        _toolStrip.Items.AddRange(new ToolStripItem[] {
            _zoomInButton, _zoomOutButton, _fitToWidthButton, _fitToHeightButton, _actualSizeButton,
            new ToolStripSeparator(),
            new ToolStripLabel("Page:"),
            _pageComboBox,
            _pageLabel
        });

        // PDF panel
        _pdfPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White
        };
        _pdfPanel.Paint += OnPdfPanelPaint;
        _pdfPanel.MouseWheel += OnPdfPanelMouseWheel;

        // Scroll bars
        _verticalScrollBar = new VScrollBar
        {
            Dock = DockStyle.Right,
            Visible = true
        };
        _verticalScrollBar.Scroll += OnVerticalScroll;

        _horizontalScrollBar = new HScrollBar
        {
            Dock = DockStyle.Bottom,
            Visible = true
        };
        _horizontalScrollBar.Scroll += OnHorizontalScroll;
    }

    private void LayoutControls()
    {
        this.Controls.Add(_toolStrip);
        this.Controls.Add(_pdfPanel);
        this.Controls.Add(_verticalScrollBar);
        this.Controls.Add(_horizontalScrollBar);
    }

    private void InitializeData()
    {
        _pdfFilePath = null;
        _pdfDocument = null;
        _pages = new List<PageInfo>();
        _currentPage = 0;
        _zoomFactor = 1.0f;
        _scrollPosition = Point.Empty;
        _highlightedBoxes = new List<BoundingBox>();
        _highlightColor = Color.Yellow;
    }

    public async void LoadPdf(string filePath)
    {
        try
        {
            _pdfFilePath = filePath;
            _pdfDocument = await _pdfParserService.OpenDocumentAsync(filePath);

            if (_pdfDocument != null)
            {
                await LoadPages();
                UpdatePageComboBox();
                _currentPage = 0;
                _pdfPanel.Invalidate();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load PDF: {FilePath}", filePath);
            MessageBox.Show($"Failed to load PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadPages()
    {
        if (_pdfDocument == null) return;

        _pages.Clear();

        for (int i = 1; i <= _pdfDocument.NumberOfPages; i++)
        {
            var page = _pdfDocument.GetPage(i);
            var pageInfo = new PageInfo
            {
                PageNumber = i,
                Width = page.Width,
                Height = page.Height,
                TextLines = new List<TextLine>(),
                Words = new List<TextWord>()
            };

            // Extract text lines
            var textLines = await _pdfParserService.ExtractTextLinesFromPageAsync(_pdfFilePath!, i);
            pageInfo.TextLines.AddRange(textLines);

            // Extract words
            var words = await _pdfParserService.ExtractWordsFromPageAsync(_pdfFilePath!, i);
            pageInfo.Words.AddRange(words);

            _pages.Add(pageInfo);
        }
    }

    private void UpdatePageComboBox()
    {
        _pageComboBox.Items.Clear();
        for (int i = 1; i <= _pages.Count; i++)
        {
            _pageComboBox.Items.Add($"Page {i}");
        }
        _pageComboBox.SelectedIndex = _currentPage;
        _pageLabel.Text = $"Page {_currentPage + 1} of {_pages.Count}";
    }

    private void OnPdfPanelPaint(object? sender, PaintEventArgs e)
    {
        if (_pages.Count == 0) return;

        var page = _pages[_currentPage];
        var graphics = e.Graphics;

        // Set up graphics
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Calculate page position
        var pageRect = CalculatePageRect();

        // Draw page background
        graphics.FillRectangle(Brushes.White, pageRect);
        graphics.DrawRectangle(Pens.Black, pageRect);

        // Draw text lines
        DrawTextLines(graphics, page, pageRect);

        // Draw highlighted boxes
        DrawHighlightedBoxes(graphics, pageRect);

        // Draw page number
        DrawPageNumber(graphics, pageRect);
    }

    private RectangleF CalculatePageRect()
    {
        var panelSize = _pdfPanel.ClientSize;
        var page = _pages[_currentPage];

        var scaleX = (float)panelSize.Width / page.Width;
        var scaleY = (float)panelSize.Height / page.Height;
        var scale = Math.Min(scaleX, scaleY) * _zoomFactor;

        var scaledWidth = page.Width * scale;
        var scaledHeight = page.Height * scale;

        var x = (panelSize.Width - scaledWidth) / 2;
        var y = (panelSize.Height - scaledHeight) / 2;

        return new RectangleF(x, y, scaledWidth, scaledHeight);
    }

    private void DrawTextLines(Graphics graphics, PageInfo page, RectangleF pageRect)
    {
        var scale = pageRect.Width / page.Width;

        foreach (var textLine in page.TextLines)
        {
            var x = pageRect.X + textLine.X * scale;
            var y = pageRect.Y + textLine.Y * scale;
            var width = textLine.Width * scale;
            var height = textLine.Height * scale;

            var rect = new RectangleF(x, y, width, height);

            // Draw text line background
            graphics.FillRectangle(Brushes.LightGray, rect);

            // Draw text
            using var font = new Font("Arial", 8);
            graphics.DrawString(textLine.Text, font, Brushes.Black, rect);
        }
    }

    private void DrawHighlightedBoxes(Graphics graphics, RectangleF pageRect)
    {
        if (_highlightedBoxes.Count == 0) return;

        var page = _pages[_currentPage];
        var scale = pageRect.Width / page.Width;

        foreach (var box in _highlightedBoxes)
        {
            var x = pageRect.X + box.X * scale;
            var y = pageRect.Y + box.Y * scale;
            var width = box.Width * scale;
            var height = box.Height * scale;

            var rect = new RectangleF(x, y, width, height);

            // Draw highlight
            using var brush = new SolidBrush(Color.FromArgb(128, _highlightColor));
            graphics.FillRectangle(brush, rect);
            graphics.DrawRectangle(Pens.Red, rect);
        }
    }

    private void DrawPageNumber(Graphics graphics, RectangleF pageRect)
    {
        var text = $"Page {_currentPage + 1} of {_pages.Count}";
        using var font = new Font("Arial", 10);
        var size = graphics.MeasureString(text, font);

        var x = pageRect.Right - size.Width - 10;
        var y = pageRect.Bottom - size.Height - 10;

        graphics.DrawString(text, font, Brushes.Black, x, y);
    }

    private void OnPdfPanelMouseWheel(object? sender, MouseEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            // Zoom
            if (e.Delta > 0)
            {
                OnZoomIn(sender, e);
            }
            else
            {
                OnZoomOut(sender, e);
            }
        }
        else
        {
            // Scroll
            var delta = e.Delta / 120;
            _verticalScrollBar.Value = Math.Max(0, Math.Min(_verticalScrollBar.Maximum, _verticalScrollBar.Value - delta));
        }
    }

    private void OnVerticalScroll(object? sender, ScrollEventArgs e)
    {
        _pdfPanel.AutoScrollPosition = new Point(_pdfPanel.AutoScrollPosition.X, e.NewValue);
        _pdfPanel.Invalidate();
    }

    private void OnHorizontalScroll(object? sender, ScrollEventArgs e)
    {
        _pdfPanel.AutoScrollPosition = new Point(e.NewValue, _pdfPanel.AutoScrollPosition.Y);
        _pdfPanel.Invalidate();
    }

    private void OnZoomIn(object? sender, EventArgs e)
    {
        _zoomFactor = Math.Min(5.0f, _zoomFactor * 1.2f);
        _pdfPanel.Invalidate();
    }

    private void OnZoomOut(object? sender, EventArgs e)
    {
        _zoomFactor = Math.Max(0.1f, _zoomFactor / 1.2f);
        _pdfPanel.Invalidate();
    }

    private void OnFitToWidth(object? sender, EventArgs e)
    {
        if (_pages.Count == 0) return;

        var page = _pages[_currentPage];
        var panelWidth = _pdfPanel.ClientSize.Width;
        _zoomFactor = panelWidth / page.Width;
        _pdfPanel.Invalidate();
    }

    private void OnFitToHeight(object? sender, EventArgs e)
    {
        if (_pages.Count == 0) return;

        var page = _pages[_currentPage];
        var panelHeight = _pdfPanel.ClientSize.Height;
        _zoomFactor = panelHeight / page.Height;
        _pdfPanel.Invalidate();
    }

    private void OnActualSize(object? sender, EventArgs e)
    {
        _zoomFactor = 1.0f;
        _pdfPanel.Invalidate();
    }

    private void OnPageChanged(object? sender, EventArgs e)
    {
        if (_pageComboBox.SelectedIndex >= 0)
        {
            _currentPage = _pageComboBox.SelectedIndex;
            _pageLabel.Text = $"Page {_currentPage + 1} of {_pages.Count}";
            _pdfPanel.Invalidate();
        }
    }

    public void HighlightBoxes(List<BoundingBox> boxes)
    {
        _highlightedBoxes = boxes;
        _pdfPanel.Invalidate();
    }

    public void ClearHighlights()
    {
        _highlightedBoxes.Clear();
        _pdfPanel.Invalidate();
    }

    public void SetHighlightColor(Color color)
    {
        _highlightColor = color;
        _pdfPanel.Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _pdfPanel.Invalidate();
    }
}

public class PageInfo
{
    public int PageNumber { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public List<TextLine> TextLines { get; set; } = new List<TextLine>();
    public List<TextWord> Words { get; set; } = new List<TextWord>();
}

public class BoundingBox
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
}
```

## Wichtige Hinweise

- PDF-Viewer Control mit PdfPig Rendering
- Zoom und Scroll Funktionalität
- Page Navigation mit ComboBox
- Bounding Box Highlighting
- Text Line und Word Rendering
- Mouse Wheel Support für Zoom/Scroll
- Tool Strip mit Zoom Controls
- Error Handling für PDF Loading
- Performance-optimiertes Rendering
