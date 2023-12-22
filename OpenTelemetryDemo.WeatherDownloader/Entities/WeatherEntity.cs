namespace OpenTelemetryDemo.Database.Entities;

public class WeatherEntity
{
    public Guid Id { get; set; }
    public string Location { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}