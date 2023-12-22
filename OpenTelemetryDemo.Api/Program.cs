using System.Text;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryDemo.Shared;
using Rebus.Config;
using Rebus.OpenTelemetry.Configuration;
using Rebus.Persistence.FileSystem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.FileSystem;

var builder = WebApplication.CreateBuilder(args);

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
var honeycombOptions = builder.Configuration.GetHoneycombOptions();

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

                o.EnrichWithHttpRequest = async (activity, httpRequest) =>
                {
                    if (!httpRequest.Body.CanSeek)
                        // We only do this if the stream isn't *already* seekable,
                        // as EnableBuffering will create a new stream instance
                        // each time it's called
                        httpRequest.EnableBuffering();

                    httpRequest.Body.Position = 0;

                    var reader = new StreamReader(httpRequest.Body, Encoding.UTF8);

                    string body = await reader.ReadToEndAsync().ConfigureAwait(false);

                    httpRequest.Body.Position = 0;
                    activity.SetTag("requestBody", body);
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
            .AddHoneycomb(honeycombOptions)
            .AddZipkinExporter(o => { o.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); });
    })
    .WithMetrics(metrics =>
    {
        metrics.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenTelemetryDemo.Api"))
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddMeter("System.Net.Http")
            .AddView("request-duration",new ExplicitBucketHistogramConfiguration()
            {
                Boundaries = new []{ 0, 0.005, 0.01, 0.05, 0.1, 0.5, 1 }
            })
            .AddProcessInstrumentation();


        metrics.AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317"); });
    });


var app = builder.Build();

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