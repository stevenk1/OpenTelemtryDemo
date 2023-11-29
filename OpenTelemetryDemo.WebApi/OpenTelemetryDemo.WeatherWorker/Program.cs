﻿using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.WeatherWorker;
using Rebus.Activation;
using Rebus.Config;
using Rebus.OpenTelemetry.Configuration;
using Rebus.Persistence.FileSystem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.FileSystem;

using var activator = new BuiltinHandlerActivator();
activator.Register(() => new MessageHandler(activator.Bus));

Configure.With(activator)
    .Transport(t => t.UseFileSystem("c:/rebus-opentelemetry", "weatherworker"))
    .Subscriptions(s => s.UseJsonFile("c:/rebus-opentelemetry/subscriptions.json"))
    .Routing(r => r.TypeBased().MapAssemblyOf<GetWeatherCommand>("emailsender"))
    .Options(o => o.EnableDiagnosticSources()) // Important for OpenTelemetry
    .Start();


using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("WeatherSender")
    .SetResourceBuilder(ResourceBuilder
        .CreateDefault()
        .AddService("OpenTelemetryDemo"))
    .AddRebusInstrumentation()
    .AddHttpClientInstrumentation()
    .SetErrorStatusOnException()
    .AddEntityFrameworkCoreInstrumentation(options => { options.SetDbStatementForText = true; })
    .AddZipkinExporter(o => { o.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); })
    .Build();

//TODO metrics


Console.WriteLine("This is Subscriber 1");
Console.WriteLine("Press ENTER to quit");
Console.ReadLine();
Console.WriteLine("Quitting...");