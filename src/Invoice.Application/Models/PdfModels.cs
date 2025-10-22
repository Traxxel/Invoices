namespace Invoice.Application.Models;

public class ParsedDocument
{
    public string FilePath { get; set; } = string.Empty;
    public DocumentInfo Info { get; set; } = new();
    public List<Page> Pages { get; set; } = new();
    public string FullText { get; set; } = string.Empty;
    public List<TextLine> AllTextLines { get; set; } = new();
    public List<TextBlock> AllTextBlocks { get; set; } = new();
    public List<Word> AllWords { get; set; } = new();
    public List<Table> Tables { get; set; } = new();
    public List<Image> Images { get; set; } = new();
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentInfo
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public string Producer { get; set; } = string.Empty;
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
    public int PageCount { get; set; }
    public string Version { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool IsValid { get; set; }
}

public class Page
{
    public int PageNumber { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<TextLine> TextLines { get; set; } = new();
    public List<TextBlock> TextBlocks { get; set; } = new();
    public List<Word> Words { get; set; } = new();
    public List<Table> Tables { get; set; } = new();
    public List<Image> Images { get; set; } = new();
}

public class TextLine
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float FontSize { get; set; }
    public string FontName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int LineIndex { get; set; }
    public List<Word> Words { get; set; } = new();
}

public class TextBlock
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public int PageNumber { get; set; }
    public List<TextLine> Lines { get; set; } = new();
}

public class Word
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float FontSize { get; set; }
    public string FontName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public List<Letter> Letters { get; set; } = new();
}

public class Letter
{
    public char Character { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float FontSize { get; set; }
    public string FontName { get; set; } = string.Empty;
}

public class Table
{
    public int PageNumber { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public List<TableRow> Rows { get; set; } = new();
    public int ColumnCount { get; set; }
    public int RowCount { get; set; }
}

public class TableRow
{
    public int RowIndex { get; set; }
    public List<TableCell> Cells { get; set; } = new();
}

public class TableCell
{
    public int ColumnIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class Image
{
    public int PageNumber { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

