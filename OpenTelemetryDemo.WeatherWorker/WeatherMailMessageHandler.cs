using OpenTelemetryDemo.Shared;
using Rebus.Handlers;

namespace OpeneTelemetryDemo.WeatherWorker;

public class WeatherMailMessageHandler : IHandleMessages<SendEmail>
{
    public async Task Handle(SendEmail message)
    {
        Console.WriteLine("Hier");
    }
}