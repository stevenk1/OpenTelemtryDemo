using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Rebus.Config;
using Rebus.OpenTelemetry.Configuration;
using Rebus.Persistence.FileSystem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.FileSystem;
using OpenTelemetryDemo.WebApi;
using OpenTelemetryDemo.Shared;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRebus(
    configure => configure
        .Transport(t => t.UseFileSystemAsOneWayClient("c:/rebus-opentelemetry"))
        .Subscriptions(s => s.UseJsonFile("c:/rebus-opentelemetry/subscriptions.json"))
        .Options(o => o.EnableDiagnosticSources()) // Important for OpenTelemetry
        .Routing(r => r.TypeBased().MapAssemblyOf<GetWeatherCommand>("weatherworker"))
);

// Configure important OpenTelemetry settings, the console exporter
builder.Services.AddOpenTelemetry()
    .WithTracing(b =>
    {
        b
            .AddSource("API")
            .ConfigureResource(resource =>
                resource.AddService(
                    "OpenTelemetryDemo"))
            .AddAspNetCoreInstrumentation(o =>
            {
                o.EnrichWithHttpRequest = (activity, httpRequest) =>
                {
                    activity.SetTag("requestProtocol", httpRequest.Protocol);
                };
                o.EnrichWithHttpResponse = (activity, httpResponse) =>
                {
                    activity.SetTag("responseLength", httpResponse.ContentLength);
                };
                o.EnrichWithException = (activity, exception) =>
                {
                    activity.SetTag("exceptionType", exception.GetType().ToString());
                };
            })
            .AddRebusInstrumentation()
            .AddZipkinExporter(o => { o.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("Microsoft.AspNetCore.Hosting");
        metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
        metrics.AddMeter("Sysgtem.Net.Http");
        //metrics.AddPrometheusExporter();

        //TODO set endpoint
        metrics.AddOtlpExporter();
    });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
