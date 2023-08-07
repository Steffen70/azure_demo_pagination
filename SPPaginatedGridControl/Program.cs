using System.Reflection;

namespace SPPaginatedGridControl;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

#if RELEASE
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var name = args.Name.Split(',').First();
            return name == "SP6LogicDemo" ? Assembly.LoadFrom($"Blueprint_{name}.dll") : null;
        };
#endif

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}