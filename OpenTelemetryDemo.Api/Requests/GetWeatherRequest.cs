namespace OpenTelemetryDemo.Api.Requests;

public class GetWeatherRequest
{
    public string Location { get; set; }

    public string Country { get; set; }
}