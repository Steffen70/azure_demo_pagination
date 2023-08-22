using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Filtration.Custom;
using DevExpress.XtraGrid.Views.Grid;
using SP6LogicDemo;
using SPPaginationDemo.Extensions;

namespace SPPaginatedGridControl;

public partial class MainForm : RibbonForm
{
    public static readonly Stopwatch Stopwatch = new();
    private readonly Sp7GridControl<CustomFiltrationParams, FiltrationHeader> _gridControl;

    public MainForm()
    {
        InitializeComponent();

        if (Stopwatch.Elapsed == TimeSpan.Zero)
            Stopwatch.Start();

        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(100);

                if (Stopwatch.Elapsed == TimeSpan.Zero)
                    continue;

                Invoke(() => bsiStopwatchOutput.Caption = $@"Elapsed: {Stopwatch.ElapsedMilliseconds} ms");

                if (!Stopwatch.IsRunning)
                    return;
            }
        }).Start();

        // Create an instance of the custom grid control
        _gridControl = new Sp7GridControl<CustomFiltrationParams, FiltrationHeader>
        {
            Dock = DockStyle.Fill,
            BaseUrl = "https://sppaginationdemo.azurewebsites.net/",
            // BaseUrl = "https://localhost:7269/",
            ActionName = "DemoSelect",
            FiltrationParams = new CustomFiltrationParams
            {
                CustomFilter = "test",
                CurrentPage = 1,
                // PageSize = int.MaxValue
                PageSize = 50
            }
        };

        // Add the grid control to the MainForm
        pContent.Controls.Add(_gridControl);

        Load += OnLoad;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        var gridView = (GridView)_gridControl.MainView;
        gridView.OptionsView.ShowGroupPanel = false;
    }

    private void OnItemClick_bbiTestEndpoint(object sender, ItemClickEventArgs e)
    {
        try
        {
            var logic = new Logic();

            // Todo: DS: Add properties to Blueprint objetct and allow to pass the object to the server

            var processName = logic.GetProgramName();

            // Todo: DS: Add SignalR support to the server and client to avoid timeout errors

            XtraMessageBox.Show($"Hello from {processName}!");
        }
        catch (Exception exception)
        {
            XtraMessageBox.Show(exception.Message);
        }
    }

    private void OnItemClick_bbiEncryptPassword(object sender, ItemClickEventArgs e)
    {
        var passwordPlainText = XtraInputBox.Show("Enter password", "Encrypt password", null, MessageBoxButtons.OK);

        if (string.IsNullOrEmpty(passwordPlainText))
            return;

        var passwordBytes = Encoding.UTF8.GetBytes(passwordPlainText);

        //New RSA Parameters with public key
        var rsa = RSA.Create().ImportKeyAndCache(Path.Combine("ServerPublicKey", "public_key.pem"));

        var encryptedPasswordBytes = passwordBytes.HybridEncrypt(rsa);
        var base64EncryptedPassword = Convert.ToBase64String(encryptedPasswordBytes);

        // Copy encrypted password to clipboard
        Clipboard.SetText(base64EncryptedPassword);
    }
}