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
        // Configure metrics with the build in AspNetCore and HttpClient instrumentation
        metrics
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation();
        // Also add a custom metric to track the number of page views
        metrics.AddMeter(PageCountMetric.Meter.Name);
        metrics.AddOtlpExporter(opt => opt.Endpoint = openTelemetryUri);
    }).WithTracing(tracing =>
        {
            // Configure tracing with the build in AspNetCore and HttpClient instrumentation
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("UmbObservability.ContactForm");
            tracing.AddOtlpExporter(opt => opt.Endpoint = openTelemetryUri);

        }
    );

// Configure Logging to send signals to the Aspire Dashboard
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
