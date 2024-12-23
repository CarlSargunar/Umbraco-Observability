using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using UmbObservability.Demo.Middleware;
using UmbObservability.Demo.OTel;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Logging
var openTelemetryUri = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
var serviceName = builder.Configuration["OTEL_SERVICE_NAME"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(res => res
        .AddService(serviceName))
    .WithMetrics(metrics =>
    {
        metrics
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation();
        metrics.AddMeter(DiagnosticsConfig.Meter.Name);
        metrics.AddOtlpExporter(opt => opt.Endpoint = openTelemetryUri);
    }).WithTracing(tracing =>
        {

            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("ContactSource");

            
            tracing.AddOtlpExporter(opt=> opt.Endpoint = openTelemetryUri);
            
        }
    );

builder.Logging.AddOpenTelemetry(log =>
{
    log.AddOtlpExporter(opt => opt.Endpoint = openTelemetryUri);
    log.IncludeScopes = true;
    log.IncludeFormattedMessage = true;
});


builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .AddCustomServices() // Add middleware
    .Build();


WebApplication app = builder.Build();

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
