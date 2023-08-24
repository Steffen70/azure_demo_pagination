namespace SPPaginatedGridControl;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        var pageSize = args[0] switch
        {
            "int.MaxValue" => int.MaxValue,
            _ => int.TryParse(args[0], out var ps) ? ps : throw new ArgumentException("Invalid page size argument")
        };

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(pageSize));
    }
}