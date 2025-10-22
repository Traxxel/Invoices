namespace InvoiceReader.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // TODO: Dependency Injection Setup wird in Aufgabe 05 hinzugefügt
        // TODO: MainForm wird in späteren Aufgaben erstellt
        // Application.Run(new MainForm());
        
        Application.Run();
    }
}

