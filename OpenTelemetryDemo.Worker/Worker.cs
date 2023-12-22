using OpenTelemetryDemo.Shared;
using OpenTelemetryDemo.Worker.Handlers;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Persistence.FileSystem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.FileSystem;

namespace OpenTelemetryDemo.Worker;

public class Worker : BackgroundService
{
    private readonly IServiceCollection _services = new ServiceCollection();

    public Worker()
    {
        _services.AddRebus(
            configure => configure
                .Transport(t => t.UseFileSystem("c:/rebus-opentelemetry", "weatherworker"))
                .Subscriptions(s => s.UseJsonFile("c:/rebus-opentelemetry/subscriptions.json"))
                .Routing(r => r.TypeBased().MapAssemblyOf<GetWeatherCommand>("emailsender"))
                .Options(o => o.EnableDiagnosticSources()) // Important for OpenTelemetry
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var provider = _services.BuildServiceProvider();

        // start the bus
        provider.StartRebus();

        while (!stoppingToken.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }
}