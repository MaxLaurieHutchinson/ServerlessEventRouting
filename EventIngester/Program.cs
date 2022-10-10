using Azure.Monitor.OpenTelemetry.Exporter;
using Ingester.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureFunctionsWorkerDefaults(WorkerApp =>
    {
        WorkerApp.UseMiddleware<ExceptionLoggingMiddleware>();
    })

.ConfigureServices(builder =>
{
    var azureMonitorInstrumentationKey = Environment.GetEnvironmentVariable(@"APPINSIGHTS_INSTRUMENTATIONKEY");
    if (!string.IsNullOrWhiteSpace(azureMonitorInstrumentationKey))
    {
        /// <remarks>
        /// Logs will be ingested as an Application Insights trace.
        /// These can be differentiated by their severityLevel.
        /// </remarks>
        builder.AddOpenTelemetryTracing((t) => t

            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            //.AddSource(nameof(CloudEventIngester)) // add services like this
            .AddAzureMonitorTraceExporter(o =>
            {
                o.ConnectionString = azureMonitorInstrumentationKey;
            }));

        /// <remarks>
        /// These counters will be aggregated and ingested as Application Insights customMetrics.
        /// </remarks>
        builder.AddOpenTelemetryMetrics((m) => m

            //.AddMeter("MyCompany.MyProduct.MyLibrary")
            .AddAzureMonitorMetricExporter(o =>
            {
                o.ConnectionString = azureMonitorInstrumentationKey;
            }));

        ///// <remarks>
        ///// Logs will be ingested as an Application Insights trace.
        ///// These can be differentiated by their severityLevel.
        ///// </remarks>
        //loggerFactory = LoggerFactory.Create(builder =>
        //{))
        //    builder.AddOpenTelemetry(options =>
        //    {
        //        options.AddAzureMonitorLogExporter(o => o.ConnectionString = azureMonitorInstrumentationKey);
        //    });
        //});
    }
}).Build();

host.Run();