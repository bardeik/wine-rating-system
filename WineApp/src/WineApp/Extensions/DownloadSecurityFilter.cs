using WineApp.Services;

namespace WineApp.Extensions;

public sealed class DownloadSecurityFilter(SecurityAccess access, ILogger<DownloadSecurityFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            await access.RequireResultsAsync(context.GetArgument<string>(0));
            return await next(context);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Download failed. Trace {TraceId}", context.HttpContext.TraceIdentifier);
            return Results.Problem("The download is temporarily unavailable.");
        }
    }
}