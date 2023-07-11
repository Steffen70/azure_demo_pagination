namespace SPPaginationDemo.Filtration.Custom;

public class CustomFiltrationParams : FiltrationParams
{
    private string? _customFilter;

    public string? CustomFilter
    {
        get => _customFilter?.ToUpper();
        set => _customFilter = value;
    }
}