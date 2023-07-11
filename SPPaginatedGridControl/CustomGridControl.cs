using System.ComponentModel;
using System.Dynamic;
using System.Net.Http;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using SPPaginationDemo.Filtration;

namespace SPPaginatedGridControl;

public class CustomGridControl : GridControl
{
    private string _dataSourceUrl = null!;
    private readonly PaginatedBindingList<object> _paginatedBindingList;

    [Browsable(true)]
    [Category("Data")]
    [Description("The URL to fetch data from.")]
    [DefaultValue("")]
    public string DataSourceUrl
    {
        get => _dataSourceUrl;
        set
        {
            _dataSourceUrl = value;
            RefreshData();
        }
    }

    public CustomGridControl()
    {
        var client = new HttpClient();
        _paginatedBindingList = new PaginatedBindingList<object>(client);
    }

    private void RefreshData()
    {
        _paginatedBindingList.EndpointUrl = _dataSourceUrl;
        DataSource = _paginatedBindingList;
    }
}