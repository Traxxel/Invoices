namespace Invoice.Application.Models;

public class NormalizedTextLine
{
    public string Text { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public int PageNumber { get; set; }
    public int LineIndex { get; set; }
    public float FontSize { get; set; }
    public string FontName { get; set; } = string.Empty;
    public List<NormalizedWord> Words { get; set; } = new();
    public bool IsNormalized { get; set; }
    public List<string> NormalizationChanges { get; set; } = new();
}

public class NormalizedWord
{
    public string Text { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float FontSize { get; set; }
    public string FontName { get; set; } = string.Empty;
    public bool IsNormalized { get; set; }
    public List<string> NormalizationChanges { get; set; } = new();
}

