using SPPaginationDemo.Filtration;
using StackExchange.Redis;
using System.Text.Json;

#pragma warning disable IDE0290

namespace SPPaginationDemo.Services;

public class ApiLogger : ILogger
{
    private static readonly Guid InstanceId = Guid.NewGuid();
    private readonly IDatabase _database;
    private readonly LogLevel _minLogLevel;

    public ApiLogger(Appsettings appSettings)
    {
        _database = appSettings.RedisDatabase;
        _minLogLevel = LogLevel.Information;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLogLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logEntry = new LogEntry(InstanceId, DateTime.UtcNow, logLevel, message, logLevel == LogLevel.Error ? exception?.StackTrace : null);

        var logEntryJson = JsonSerializer.Serialize(logEntry, HttpExtensions.Options);

        _database.ListRightPush("Logs", logEntryJson);
    }

    public record LogEntry(Guid InstanceId, DateTime Timestamp, LogLevel LogLevel, string Message, string? StackTrace)
    {
        public override string ToString()
        {
            return $"{{ InstanceId = {InstanceId}, Timestamp = {Timestamp}, LogLevel = {LogLevel}, Message = {Message}, StackTrace = {StackTrace} }}";
        }
    }
}

