using System.Text.Json;

namespace SPPaginationDemo.Filtration;

public static class HttpExtensions
{
    public static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task<TParams?> GetFiltrationParamsAsync<TParams>(this HttpContext httpContext) where TParams : FiltrationParams
    {
        // Ensure the request can be read multiple times
        httpContext.Request.EnableBuffering();

        // Rewind the request stream to the beginning
        httpContext.Request.Body.Position = 0;

        // Read the request body and deserialize it into an object of type TParams
        var body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
        var filtrationParams = JsonSerializer.Deserialize<TParams>(body, Options);

        // Rewind the request stream to the beginning so it can be read again by the next middleware
        httpContext.Request.Body.Position = 0;

        return filtrationParams;
    }

    public static void AddFiltrationHeader<THeader>(this HttpResponse response, THeader header)
    {
        response.Headers.Add("Filtration", JsonSerializer.Serialize(header, Options));
        response.Headers.Add("Access-Control-Expose-Headers", "Filtration");
    }
}