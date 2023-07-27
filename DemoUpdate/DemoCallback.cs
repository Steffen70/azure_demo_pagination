using Microsoft.AspNetCore.Mvc;
using SPPaginationDemo.Model;

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