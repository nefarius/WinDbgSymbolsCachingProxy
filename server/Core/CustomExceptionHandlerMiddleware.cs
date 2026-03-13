using Microsoft.AspNetCore.Http.Extensions;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Middleware that handles exceptions occurring during the execution of the HTTP pipeline
///     and logs them using the provided logger.
/// </summary>
public class CustomExceptionHandlerMiddleware(ILogger<CustomExceptionHandlerMiddleware> logger, RequestDelegate next)
{
    /// <summary>
    ///     Invokes the next middleware; logs and swallows exceptions so the pipeline does not throw.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that completes when the pipeline has been executed.</returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogWarning(ex, "BadHttpRequestException");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Exception when invoke {RequestMethod}:{GetDisplayUrl}", context.Request.Method,
                context.Request.GetDisplayUrl());
        }
    }
}