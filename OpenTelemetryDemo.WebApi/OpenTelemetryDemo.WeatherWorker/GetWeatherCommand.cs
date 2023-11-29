namespace OpenTelemetry.WeatherWorker;

public class GetWeatherCommand
{
    public GetWeatherCommand(string location, string country)
    {
        Location = location;
        Country = country;
    }


    public string Location { get; }

    public string Country { get; }
}