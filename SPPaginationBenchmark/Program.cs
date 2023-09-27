using SPPaginatedGridControl;
using System.Net.Http.Headers;
using SPPaginationDemo.Filtration;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;
using SPPaginationDemo.Filtration.Custom;

// ReSharper disable ConvertToLocalFunction

var client = new HttpClient
{
    BaseAddress = new Uri("https://spagds-devwebapp.azurewebsites.net/")
};

// Add default headers as necessary
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
client.DefaultRequestHeaders.Add("User-Agent", "Sp7GridControl");

const string actionName = "DemoSelect";

var filtrationParams = new CustomFiltrationParams
{
    CustomFilter = "test",
    CurrentPage = 1,
    PageSize = 50
};

var testProcess = async () =>
{
    var stopwatch = new CustomStopwatch();

    stopwatch.Start();

    var id = (await client.GetStringAsync($"/Paginated/sql-query-identifier/{actionName}")).Replace("\"", "");

    //_ = await client.GetStringAsync($"/Paginated/assembly-bytes/{id}");

    var request = new HttpRequestMessage(HttpMethod.Get, $"/Paginated/{actionName}");
    var jsonParams = JsonSerializer.Serialize(filtrationParams, HttpExtensions.Options);
    request.Content = new StringContent(jsonParams, Encoding.UTF8, "application/json");

    var response = await client.SendAsync(request);

    if (!response.IsSuccessStatusCode)
        throw new Exception($"Error: {response.StatusCode} {response.ReasonPhrase}");

    stopwatch.Stop();

    return stopwatch.TotalMilliseconds;
};

var tasks = new List<Task<int>>();
for (var i = 0; i < 5; i++) tasks.Add(new Task<int>(() => testProcess().Result));

tasks.ForEach(t =>
{
    t.Start();

    Task.Delay(100).Wait();
});

var results = tasks.Select(t => t.Result).ToArray();

var average = results.Sum() / results.Length;

Console.WriteLine($@"Average: {average} ms");

Console.ReadLine();