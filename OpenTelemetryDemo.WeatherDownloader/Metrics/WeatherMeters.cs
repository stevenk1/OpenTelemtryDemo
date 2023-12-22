using System.Diagnostics.Metrics;

namespace OpenTelemetryDemo.WeatherDownloader.Metrics;

public class WeatherMeters
{
    private readonly Meter _meter;

    public WeatherMeters()
    {
        _meter = new Meter("OpenTelemetryDemoMeter");

        TemperatureHistogram = _meter.CreateHistogram<double>("temperature", "C", "Temperature at a location");
        HellosSend = _meter.CreateCounter<int>("hellos-send", "Times");
    }

    private Counter<int> HellosSend { get; }
    private Histogram<double> TemperatureHistogram { get; }


    public void AddHello()
    {
        HellosSend.Add(1);
    }

    public Meter GetMeter()
    {
        return _meter;
    }

    public void RecordTemperature(double temperature, string city)
    {
        TemperatureHistogram.Record(temperature, KeyValuePair.Create<string, object>("City", city));
    }
}