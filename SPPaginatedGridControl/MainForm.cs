using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Filtration.Custom;
using DevExpress.XtraGrid.Views.Grid;
using SPUpdateFramework;
using SPUpdateFramework.Extensions;

namespace SPPaginatedGridControl;

public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
{
    private readonly Sp7GridControl<CustomFiltrationParams, FiltrationHeader> _gridControl;

    public MainForm()
    {
        InitializeComponent();

        // Create an instance of the custom grid control
        _gridControl = new Sp7GridControl<CustomFiltrationParams, FiltrationHeader>
        {
            Dock = DockStyle.Fill,
            BaseUrl = "https://localhost:7269/",
            ActionName = "DemoSelect",
            FiltrationParams = new CustomFiltrationParams
            {
                CustomFilter = "test",
                CurrentPage = 1,
                PageSize = 50
            }
        };

        // Add the grid control to the MainForm
        pContent.Controls.Add(_gridControl);

        Load += OnLoad;
#pragma warning disable CS4014
        bbiUpload.ItemClick += (s, e) => OnItemClick_bbiUpload(s, e);
#pragma warning restore CS4014
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        var gridView = (GridView)_gridControl.MainView;
        gridView.OptionsView.ShowGroupPanel = false;
    }

    private async Task OnItemClick_bbiUpload(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = @"Update DLL (*.dll)|*.dll",
            Title = @"Select Update DLL"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        var assemblyPath = new FileInfo(dialog.FileName);

        var assemblyBytes = await File.ReadAllBytesAsync(assemblyPath.FullName);

        // Load the assembly into current AppDomain
        var assembly = Assembly.Load(assemblyBytes);

        var ribbonFunctionTypes = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRibbonFunction))).ToList();
        var ribbonFunctions = ribbonFunctionTypes.Select(t => (IRibbonFunction)Activator.CreateInstance(t)!).ToList();

        if (ribbonFunctions.Any())
        {
            foreach (var ribbonFunction in ribbonFunctions)
            {
                var barButtonItem = new BarButtonItem
                {
                    Caption = ribbonFunction.Name,
                    ImageOptions =
                    {
                        Image = Image.FromStream(new MemoryStream(Convert.FromBase64String(ribbonFunction.Image)))
                    },
                    RibbonStyle = RibbonItemStyles.Large,
                };

                barButtonItem.ItemClick += (s, e) =>
                {
                    var clickTask = new Task(() => ribbonFunction.OnClick(s, e));
                    clickTask.Start();
                };

                rpgDynamicConstruction.ItemLinks.Add(barButtonItem);
            };
        }

        // Upload the assembly to the server
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7269/")
        };

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("User-Agent", "DemoUpdateClient");

        var publicKey = (await httpClient.GetStringAsync("/live-update/get-public-key")).Replace("\"", "");

        var publicKeyString = Encoding.UTF8.GetString(Convert.FromBase64String(publicKey));

        var publicRsa = RSA.Create();
        publicRsa.ImportFromPem(publicKeyString);

        var compressedAssemblyBytes = assemblyBytes.Compress();

        var encryptedAssemblyBytes = compressedAssemblyBytes.HybridEncrypt(publicRsa);

        var encryptedAssemblyString = Convert.ToBase64String(encryptedAssemblyBytes);

        var response = await httpClient.PostAsync("/live-update/update-endpoint", new StringContent(encryptedAssemblyString));

        var assemblyName = await response.Content.ReadAsStringAsync();

        XtraMessageBox.Show($"Successfully uploaded update: {assemblyName}");
    }
}