namespace SPPaginationDemo.Filtration;

public class FiltrationHeader
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }

    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}