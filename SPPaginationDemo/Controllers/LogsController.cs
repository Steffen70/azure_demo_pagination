using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Services;
using StackExchange.Redis;

#pragma warning disable IDE0290

namespace SPPaginationDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class LogsController : ControllerBase
{
    private readonly IDatabase _database;

    public LogsController(Appsettings appSettings)
    {
        _database = appSettings.RedisDatabase;
    }

    [HttpGet]
    public IActionResult GetLogs()
    {
        var logEntries = _database.ListRange("Logs").Select(entry => JsonSerializer.Deserialize<object>(entry!, HttpExtensions.Options)).ToArray();

        return Ok(logEntries);
    }
}