using System.Text.Json;

namespace SPPaginationDemo.Filtration;

public static class HttpExtensions
{
    public static FilteredList<TList> AddFiltrationHeader<TList>(this HttpResponse response, FilteredList<TList> filteredList)
    {
        AddFiltrationHeader(response, filteredList.Header);

        return filteredList;
    }

    public static FilteredList<TList, THeader> AddFiltrationHeader<TList, THeader>(this HttpResponse response, FilteredList<TList, THeader> filteredList)
        where THeader : FiltrationHeader
    {
        AddFiltrationHeader(response, filteredList.Header);

        return filteredList;
    }

    public static void AddFiltrationHeader<THeader>(this HttpResponse response, THeader header)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        response.Headers.Add("Filtration", JsonSerializer.Serialize(header, options));
        response.Headers.Add("Access-Control-Expose-Headers", "Filtration");
    }
}