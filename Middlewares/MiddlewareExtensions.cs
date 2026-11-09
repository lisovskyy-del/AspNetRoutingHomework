using AspNetRoutingHomework.Middlewares;

namespace AspNetRoutingHomework.Middlewares;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }

    public static IApplicationBuilder UseProductRouting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RoutingAndProductMiddleware>();
    }
}