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
using DevExpress.CodeParser;
using SP6LogicDemo;

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
            var processName = logic.GetProgramName();
            XtraMessageBox.Show($"Hello from {processName}!");
        }
        catch (Exception exception)
        {
            XtraMessageBox.Show(exception.Message);
        }
    }
}