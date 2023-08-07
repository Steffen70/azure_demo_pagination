using Microsoft.AspNetCore.Mvc;
using SPUpdateFramework;
using SP6LogicDemo;

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

        var processName = new Logic().GetProgramName();
        return new JsonResult($"Hello {demoDto.Name}, from {processName}");
    }
}