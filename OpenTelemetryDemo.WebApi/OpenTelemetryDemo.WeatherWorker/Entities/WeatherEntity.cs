using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTelemetry.WeatherWorker.Entities
{
    public class WeatherEntity
    {
        public Guid Id { get; set; }
        public string Location { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
