using DevExpress.XtraEditors;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Filtration.Custom;
using DevExpress.XtraGrid.Views.Grid;

namespace SPPaginatedGridControl;

public partial class MainForm : XtraForm
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
        Controls.Add(_gridControl);

        Load += OnLoad;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        var gridView = (GridView)_gridControl.MainView;
        gridView.OptionsView.ShowGroupPanel = false;
    }
}