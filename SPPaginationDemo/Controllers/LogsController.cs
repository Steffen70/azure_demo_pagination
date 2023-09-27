using System.Text;
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
        var logEntries = _database.ListRange("Logs").Select(entry => JsonSerializer.Deserialize<ApiLogger.LogEntry>(entry!, HttpExtensions.Options)!);

        logEntries = logEntries.Reverse();

        logEntries = logEntries.Take(200).ToArray();

        var timezone = HttpContext.Request.Headers["timezone"].First();

        var log = logEntries.Aggregate(new List<string>(), (list, entry) =>
        {
            var timestamp = string.IsNullOrWhiteSpace(timezone) ? entry.Timestamp : TimeZoneInfo.ConvertTime(entry.Timestamp, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById(timezone));

            var entryStackTraceBase64 = entry.StackTrace != null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(entry.StackTrace)) : string.Empty;

            list.Add($"{timestamp:dd.MM.yyyy HH:mm:ss.fff} {entry.LogLevel} {entry.InstanceId} {entry.Message} {entryStackTraceBase64}");

            return list;
        });

        return Ok(string.Join('\n', log.ToArray().Reverse()));
    }
}