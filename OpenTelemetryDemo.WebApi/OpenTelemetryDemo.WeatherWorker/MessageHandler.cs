﻿using OpenWeatherMap;
using OpenWeatherMap.Models;
using Rebus.Bus;
using Rebus.Handlers;
using Flurl.Http;
using OpenTelemetry.WeatherWorker.Entities;
using Microsoft.EntityFrameworkCore;

namespace OpenTelemetry.WeatherWorker;

public class MessageHandler : IHandleMessages<GetWeatherCommand>, IHandleMessages<StoreWeatherCommand>
{
    private readonly IBus _bus;

    public MessageHandler(IBus bus)
    {
        _bus = bus;
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
            new OpenWeatherMapOptions
            {
                ApiKey = "fd3c8a1c5edacd9e697b25c4f5ce84d3",
                Language = "en",
                ApiEndpoint = "https://api.openweathermap.org",
                UnitSystem = "metric"
            });
        var weatherInfo = await openWeatherMapService.GetCurrentWeatherAsync(firstCoordinate.Latitude, firstCoordinate.Longitude);

        await _bus.SendLocal(new StoreWeatherCommand
        {
            Location = weatherInfo.CityName,
            Temperature = weatherInfo.Main.Temperature.Value,
            Humidity = weatherInfo.Main.Humidity.Percent,
    
        });
    }

    public async Task Handle(StoreWeatherCommand message)
    {
        await using var db = new WeatherStoreContext();

        await db.WeatherStore.AddAsync(new WeatherEntity
        {
            Location = message.Location,
            Temperature = message.Temperature,
            Humidity = message.Humidity,
        });
        await db.SaveChangesAsync();

        var test = await db.WeatherStore.ToListAsync();
        //  throw new Exception("oops"); // TODO wrap in custom activity
        Console.WriteLine(db.DbPath);
    }
}