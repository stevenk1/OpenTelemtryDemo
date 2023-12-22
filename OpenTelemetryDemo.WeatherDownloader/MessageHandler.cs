using System.Globalization;
using Flurl.Http;
using Grpc.Net.Client;
using GrpcGreeterClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTelemetryDemo.Database.Entities;
using OpenTelemetryDemo.Shared;
using OpenTelemetryDemo.WeatherDownloader.Dto;
using OpenTelemetryDemo.WeatherDownloader.Metrics;
using OpenWeatherMap;
using Rebus.Bus;
using Rebus.Handlers;

namespace OpenTelemetryDemo.WeatherDownloader;

public class MessageHandler : IHandleMessages<GetWeatherCommand>, IHandleMessages<StoreWeatherCommand>
{
    private readonly IBus _bus;
    private readonly WeatherMeters _weatherMeters;

    public MessageHandler(IBus bus, WeatherMeters weatherMeters)
    {
        _bus = bus;
        _weatherMeters = weatherMeters;
    }

    public async Task Handle(GetWeatherCommand message)
    {
        var coordinates = await $"https://geocode.maps.co/search?q={message.Location},{message.Country}".GetAsync()
            .ReceiveJson<Coordinates[]>();
        var firstCoordinate = coordinates.FirstOrDefault();
        if (firstCoordinate is null)
            throw new Exception(
                $"No coordinates found for location {message.Location} in country {message.Country}, this place does not exist.");


        IOpenWeatherMapService openWeatherMapService = new OpenWeatherMapService(
            new NullLogger<OpenWeatherMapService>(), new OpenWeatherMapConfiguration
            {
                ApiKey = "fd3c8a1c5edacd9e697b25c4f5ce84d3",
                Language = "en",
                ApiEndpoint = "https://api.openweathermap.org",
                UnitSystem = "metric"
            });

        Console.WriteLine(firstCoordinate.lat + "'" + firstCoordinate.lon);
        var weatherInfo = await openWeatherMapService.GetCurrentWeatherAsync(
            double.Parse(firstCoordinate.lat, CultureInfo.InvariantCulture),
            double.Parse(firstCoordinate.lon, CultureInfo.InvariantCulture));

        await _bus.SendLocal(new StoreWeatherCommand
        {
            Location = weatherInfo.CityName,
            Temperature = weatherInfo.Main.Temperature.Value,
            Humidity = weatherInfo.Main.Humidity.Value
        });
    }

    public async Task Handle(StoreWeatherCommand message)
    {
        await using var db = new WeatherStoreContext();

        await db.WeatherStore.AddAsync(new WeatherEntity
        {
            Location = message.Location,
            Temperature = message.Temperature,
            Humidity = message.Humidity
        });
        _weatherMeters.RecordTemperature(message.Temperature, message.Location);
        await db.SaveChangesAsync();

        var test = await db.WeatherStore.ToListAsync();
        //  throw new Exception("oops"); // TODO wrap in custom activity


        using var channel = GrpcChannel.ForAddress("http://localhost:5085");
        var client = new Greeter.GreeterClient(channel);
        var reply = await client.SayHelloAsync(
            new HelloRequest { Name = "GreeterClient" });
        _weatherMeters.AddHello();
        Console.WriteLine("Greeting: " + reply.Message);
    }
}