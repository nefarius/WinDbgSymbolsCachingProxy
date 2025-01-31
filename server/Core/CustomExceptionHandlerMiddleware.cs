using Microsoft.AspNetCore.Http.Extensions;

namespace WinDbgSymbolsCachingProxy.Core;

public class CustomExceptionHandlerMiddleware
{
    private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;

    public CustomExceptionHandlerMiddleware(ILogger<CustomExceptionHandlerMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BadHttpRequestException ex)
        {
            _logger.LogWarning(ex, "BadHttpRequestException");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                $"Exception when invoke {context.Request.Method}:{context.Request.GetDisplayUrl()}");
        }
    }
}