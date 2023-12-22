using OpenTelemetryDemo.Shared;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Transport.FileSystem;

namespace OpeneTelemetryDemo.WeatherWorker;

public class Worker : BackgroundService
{
    private readonly IServiceCollection _services;

    public Worker()
    {
        _services = new ServiceCollection();
        _services.AddTransient<WeatherMailMessageHandler>();
        _services.AddRebus(configure => configure
            .Transport(t => t.UseFileSystem("c:/rebus-opentelemetry", "weatherworker"))
            .Routing(r => r.TypeBased().MapAssemblyOf<SendEmail>("emailsender")));
        _services.AutoRegisterHandlersFromAssemblyOf<Worker>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var provider = _services.BuildServiceProvider();

        // start the bus
        provider.StartRebus();

        while (!stoppingToken.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }
}