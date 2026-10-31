namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Middleware that handles exceptions occurring during the execution of the HTTP pipeline,
///     logs them and translates them into an appropriate HTTP status code.
/// </summary>
public class CustomExceptionHandlerMiddleware(ILogger<CustomExceptionHandlerMiddleware> logger, RequestDelegate next)
{
    /// <summary>
    ///     Invokes the next middleware; logs exceptions and converts them into error responses.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that completes when the pipeline has been executed.</returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Query strings may contain secrets (e.g. apiKey), so only the path is logged.
            logger.LogDebug("Request cancelled by client: {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogWarning(ex, "Bad request {Method} {Path}", context.Request.Method, context.Request.Path);
            SetStatusCode(context, ex.StatusCode);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method,
                context.Request.Path);
            SetStatusCode(context, StatusCodes.Status500InternalServerError);
        }
    }

    private static void SetStatusCode(HttpContext context, int statusCode)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = statusCode;
        }
    }
}
