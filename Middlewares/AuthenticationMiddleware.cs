namespace AspNetRoutingHomework.Middlewares;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (path == "/" || 
            path == "/index.html" || 
            path == "/login" || 
            path.StartsWith("/images/"))
        {
            await _next(context);
            return;
        }

        var authCookie = context.Request.Cookies["AuthUser"];
        bool isAuthenticated = !string.IsNullOrEmpty(authCookie) && authCookie == "admin";

        if (!isAuthenticated)
        {
            bool isHtmlRequest = context.Request.Headers["Accept"].ToString().Contains("text/html");

            if (isHtmlRequest)
            {
                context.Response.Redirect("/login");
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized access. Please login first.");
            }
            return;
        }

        await _next(context);
    }
}