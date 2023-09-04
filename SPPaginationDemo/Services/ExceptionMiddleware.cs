#pragma warning disable CA2254
#pragma warning disable IDE0290

namespace SPPaginationDemo.Services;

public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger _logger;

    public ExceptionMiddleware(ILogger logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        try
        {
            _logger.LogInformation($"Request: {httpContext.Request.Path}");
            await next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.Log<object?>(LogLevel.Error, new EventId(), null, ex, (s, e) => e!.Message);
            throw;
        }
    }
}
