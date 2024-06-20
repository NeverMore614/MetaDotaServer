using System.Net;

public class DisableOptionsRequestsMiddleware
{
    private readonly RequestDelegate _next;

    public DisableOptionsRequestsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Options)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            return;
        }

        await _next(context);
    }
}

public static class DisableOptionsRequestsMiddlewareExtensions
{
    public static IApplicationBuilder UseDisableOptionsRequests(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DisableOptionsRequestsMiddleware>();
    }
}