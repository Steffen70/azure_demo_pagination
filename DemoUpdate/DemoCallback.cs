using Microsoft.AspNetCore.Mvc;
using SPUpdateFramework;
using System.Net.Http.Headers;
using System.Text;
using DevExpress.XtraEditors;

namespace DemoUpdate;

public class DemoDto
{
    public string Name { get; set; } = null!;
}

public class DemoCallback : IEndpointCallback
{
    public ActionResult OnEndpointCallback(string json)
    {
        var demoDto = System.Text.Json.JsonSerializer.Deserialize<DemoDto>(json)!;

        return new JsonResult($"Hello {demoDto.Name}");
    }
}

public class DemoRibbonFunction : IRibbonFunction
{
    public string Name => "Demo Call";

    public string Image =>
        "iVBORw0KGgoAAAANSUhEUgAAAGAAAABgCAYAAADimHc4AAAACXBIWXMAAAsTAAALEwEAmpwYAAAEk0lEQVR4nO2cy29cNRTGD+FR8SivtkKQqIIdj1UlSIaJnVnAkg2EIDb8AVNYlApaAYvyRgQERIBYwa4SpLQdxucOjbKogtTXBkSJkrGjkqbtApTCCkTKxsiXUUnbmYTk+ubc2OcnHWmkKJ74++xjX98TAzAMwzAMwzBFxe7p6jeNktDqZWHUfqlxUmr8XRr1dxrus8ZJodXXUquXhMY+9zvUf/a6p2IaPUKrd6TBc9KgXUkIrc4Kg2+XtOqm7se645GpA5uExs+kwQsrFb6NEQtSq09Lk4dup+7XukBo9bQweD6r8FcYYXB+YAafou5fYakcPnxNa9R7Ff5KI9Qn7ruo+1soSmdGr5daqbzFvxga6+47qftdCIbs6NXS4IE1E/+iCUrxTACX8/NPO0usCx9D7AuuJBJ/0ZrwJMSI2xYKg/P0BuB5t+2F2KBMPTL2VOSecH08ZElfBmi1ENUTc+t4wRYs3oIosHu6hMYzBRDcXhbn3JYYQsedahZAbNs2tOqF0BEaXyEX2rQPYXA3hE56np9VKO1SWDJYnq5tdNGv1ePSqGb2GYD7IHTSlykZxS+1OVpOnyu0OpttBqiTEDrSqN+yjdRksFPbYgaHMqageQidrPv/8nRtY6e2e03j5myzSy1A6GTN07AEj54avyVjCvoDQidPA6RJBjOmoJ8hdPIyoJQuwmgyGnACQse3Ab1p3k8Gs4rfMuBLCB3fBnQ1nrXXHtphbxh/0W757g1794kRu+3kXiuNWs0aUIXQ8Z6CkqptFxvGnrdbj31g+6b2/++2K836vRA6a2UAtMLNkO6jw/bh5sHlRv8sWHsVhM5aGwCtcGnq/u8/79y2xhcgBqgMgDS22+4jw+6B63Lx/4ymco7WgGoamyZetf26vtiAdyEWimAAJFW7eeI1J7x7B3C6Mjl6E8RCUQyApGp7jr5nhUkeg5golAHH3mcDJKeg+GbAlonXrTD/LcLCqGGIBeptaI/L+222odFUx1EZcN3YDvvAD190bFcYtQtigOQo4siwLTdrS7et1Wk+ijD+DNgwttNuPf7hig7jBjTeB6HjewZ0NZ5L08uN47vS4+h7jo/YbT/tXVXbwuB2CB3fBpQ91gUJrb6C0PG+BnitC+JXkna1BvipC1KzEDp5zQAvdUFcloLLitR36ps7IK+6IC7MwuVFmsFncqwLiqI0MfM/UlSa9c351AVFUZyb2QDrKqTdvQ8u5/usC4qjPN2DATKniOI8iFpkuUSUp5MHIXSoRZad089cFLdskQttOoV6E2KAXmhsu/8vT9fughgo6OgfgVigFxsvDa1+FXN4G8QCueDm0hjQ+ATEBLXgclEIjR9BbBQo9agorywryK6nFu2lfUXY8QzFcCtKJ+hGPf4S3YLbDoJ085cb9ZXZg7dS970QrJn4Gufc8YKc+vZO6j4XCg/C7nMvTlqXfrh7Jy60Pv/4789w90Cz/lAUB2sUBgDDBqxreAYQwwYQwwYQwwYQwwYQwwYQwwYQwwYQwwYQwwYQwwYQwwYQwwYQwwYQwwYQwwYwDMMwDMMwDAOR8w+qIyyOnbfWgQAAAABJRU5ErkJggg==";

    private HttpClient? _client;

    private HttpClient Client
    {
        get
        {
            if (_client != null) return _client;

            _client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7269/")
            };

            // Add default headers as necessary
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("User-Agent", "Sp7DemoUpdate");

            return _client;
        }
    }

    public void OnClick(object? sender, EventArgs e)
    {
        //TODO: Create HTTP Client to abstract Signal R responses to avoid timeouts

        var dto = new DemoDto
        {
            Name = "World"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(dto);

        var response = Client.PostAsync("live-update/callback/DemoUpdate/DemoCallback", new StringContent(json, Encoding.UTF8, "application/json")).Result;

        //Open XtraMessageBox to show response

        XtraMessageBox.Show(response.Content.ReadAsStringAsync().Result);
    }
}