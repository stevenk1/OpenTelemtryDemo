namespace OpenTelemetryDemo.Worker;

public class StoreWeatherCommand
{
    public string Location { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}