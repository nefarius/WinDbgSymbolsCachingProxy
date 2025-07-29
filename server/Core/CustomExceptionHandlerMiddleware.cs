using Microsoft.AspNetCore.Http.Extensions;

namespace WinDbgSymbolsCachingProxy.Core;

public class CustomExceptionHandlerMiddleware(ILogger<CustomExceptionHandlerMiddleware> logger, RequestDelegate next)
{
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