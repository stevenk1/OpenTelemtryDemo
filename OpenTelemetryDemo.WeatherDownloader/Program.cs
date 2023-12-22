using Honeycomb.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryDemo.Shared;
using OpenTelemetryDemo.WeatherDownloader;
using OpenTelemetryDemo.WeatherDownloader.Metrics;
using Rebus.Activation;
using Rebus.Config;
using Rebus.OpenTelemetry.Configuration;
using Rebus.Persistence.FileSystem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.FileSystem;
using System.Diagnostics;
using System.Diagnostics.Metrics;



var meters = new WeatherMeters();

using var activator = new BuiltinHandlerActivator();
activator.Register(() => new MessageHandler(activator.Bus, meters));

Configure.With(activator)
    .Transport(t => t.UseFileSystem("c:/rebus-opentelemetry", "weatherworker"))
    .Subscriptions(s => s.UseJsonFile("c:/rebus-opentelemetry/subscriptions.json"))
    .Routing(r => r.TypeBased().MapAssemblyOf<GetWeatherCommand>("emailsender"))
    .Options(o => o.EnableDiagnosticSources()) // Important for OpenTelemetry
    .Start();

var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", optional: false);
IConfiguration config = configBuilder.Build();
var honeycombOptions = config.GetSection("Honeycomb").Get<HoneycombOptions>();


using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("WeatherSender")
    .SetResourceBuilder(ResourceBuilder
        .CreateDefault()
        .AddService("OpenTelemetryDemo"))
    .AddRebusInstrumentation()
    .AddGrpcClientInstrumentation()
    .AddHttpClientInstrumentation()
    .SetErrorStatusOnException()
    .AddEntityFrameworkCoreInstrumentation(options => { options.SetDbStatementForText = true; })
    .AddHoneycomb(honeycombOptions)
    //.AddCommonInstrumentations()
    .AddZipkinExporter(o => { o.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); })
    .Build();
using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenTelemetryDemo.WeatherDownloader"))
    .AddMeter(meters.GetMeter().Name)
    .AddConsoleExporter()
    .AddOtlpExporter(opts =>
    {
        opts.Endpoint = new Uri("http://localhost:4317");
    }).Build();   


Console.WriteLine("This is Subscriber 1");
Console.WriteLine("Press ENTER to quit");
Console.ReadLine();
Console.WriteLine("Quitting...");
