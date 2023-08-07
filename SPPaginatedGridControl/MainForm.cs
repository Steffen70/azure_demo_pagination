using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DemoUpdate;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Filtration.Custom;
using DevExpress.XtraGrid.Views.Grid;
using SPUpdateFramework;
using SPUpdateFramework.Extensions;
using DevExpress.CodeParser;

namespace SPPaginatedGridControl;

public partial class MainForm : RibbonForm
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

    private async Task OnItemClick_bbiUpload(object sender, ItemClickEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = @"Update DLL (*.dll)|*.dll",
            Title = @"Select Update DLL"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        var assemblyPath = new FileInfo(dialog.FileName);

        var assemblyBytes = await File.ReadAllBytesAsync(assemblyPath.FullName);

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


#pragma warning disable CS4014
    private void OnItemClick_bbiTestEndpoint(object sender, ItemClickEventArgs e) => CallEndpoint(false);

    private void OnItemClick_bbiRunServerside(object sender, ItemClickEventArgs e) => CallEndpoint(true);
#pragma warning restore CS4014

    private static async Task CallEndpoint(bool runServerside)
    {
        //TODO: Create HTTP Client to abstract Signal R responses to avoid timeouts

        var dto = new DemoDto
        {
            Name = "World"
        };

        var client = new Sp7WebClient();

        var stringResult = await client.CallEnpointAsync<DemoCallback, DemoDto>(dto, runServerside);

        XtraMessageBox.Show(stringResult);
    }
}