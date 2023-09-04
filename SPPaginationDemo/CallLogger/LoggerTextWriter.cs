using System.Text;

#pragma warning disable IDE0290
#pragma warning disable CA2254

namespace SPPaginationDemo.CallLogger;

public class LoggerTextWriter : TextWriter
{
    private readonly ILogger _logger;

    public LoggerTextWriter(ILogger logger)
    {
        _logger = logger;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        // Ignore single character writes
    }

    public override void Write(string? value)
    {
        // Write string to logger
        if (value == null) return;

        if (!value.StartsWith("Call Logging:")) return;

        _logger.LogInformation(value);
    }

    public override void WriteLine(string? value)
    {
        if (value == null) return;

        if (!value.StartsWith("Call Logging:")) return;

        _logger.LogInformation(value);
    }
}

