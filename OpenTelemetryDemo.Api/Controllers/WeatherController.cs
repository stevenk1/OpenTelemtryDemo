using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetryDemo.Api.Requests;
using OpenTelemetryDemo.Shared;
using Rebus.Bus;

namespace OpenTelemetryDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IBus _bus;

    public WeatherController(IBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    [Route("Send")]
    public async Task Send(GetWeatherRequest request)
    {
        var activityFeature = HttpContext.Features.Get<IHttpActivityFeature>();
        activityFeature?.Activity.AddTag("requested.city", $"{request.Location}, {request.Country}");
        activityFeature?.Activity.AddBaggage("requested.city", $"{request.Location}, {request.Country}");

        await _bus.Send(new GetWeatherCommand(request.Location, request.Country));
    }

    [HttpPost]
    [Route("Calculate")]
    public async Task Calculate()
    {
        var activityFeature = HttpContext.Features.Get<IHttpActivityFeature>();

        activityFeature?.Activity.AddTag("tag", "hier");

        Task.Delay(1000).Wait();
        activityFeature?.Activity.Stop();
    }
}