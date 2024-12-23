namespace UmbObservability.Demo.Middleware;

public static class MyBuilderExtensions
{
    public static IUmbracoBuilder RegisterCustomServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IStartupFilter, MiddlewareStartupFilter>(); 
        return builder;
    }

    public static IUmbracoBuilder AddCustomServices(this IUmbracoBuilder builder)
    {
        builder.RegisterCustomServices();
        return builder;
    }
}