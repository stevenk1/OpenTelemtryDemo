using Microsoft.AspNetCore.Mvc;
using OpenTelemetryDemo.Shared;
using Rebus.Bus;

namespace OpenTelemetryDemo.WebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IBus _bus;

        public WeatherController(IBus bus)
        {
            _bus = bus;
        }

        [HttpPost(Name = "Download")]
        public async Task Send(GetWeatherRequest request)
        {
            await _bus.Send(new GetWeatherCommand(request.Location, request.Country));
        }
    }
}
