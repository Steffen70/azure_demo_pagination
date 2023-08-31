// ReSharper disable ConvertToAutoProperty
namespace SPPaginationDemo.Filtration;

public class FiltrationParams
{
#pragma warning disable IDE0051
    private const int MaxPageSize = 50;
#pragma warning restore IDE0051
    private int _pageSize = 10;

    public int CurrentPage { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value /*> MaxPageSize ? MaxPageSize : value*/;
    }
}