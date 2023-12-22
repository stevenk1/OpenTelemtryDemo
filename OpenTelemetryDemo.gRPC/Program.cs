using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryDemo.gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddOpenTelemetry()
    .WithTracing(b =>
    {
        b
            .AddSource("GRPC")
            .ConfigureResource(resource =>
                resource.AddService(
                    "OpenTelemetryDemo GRPC"))
            .AddAspNetCoreInstrumentation(o =>
            {
                o.EnrichWithHttpRequest = (activity, httpRequest) =>
                {
                    activity.SetTag("requestProtocol", httpRequest.Protocol);
                };
                o.EnrichWithHttpResponse = (activity, httpResponse) =>
                {
                    activity.SetTag("responseLength", httpResponse.ContentLength);
                };
                o.EnrichWithException = (activity, exception) =>
                {
                    activity.SetTag("exceptionType", exception.GetType().ToString());
                };
            })
            .AddZipkinExporter(o => { o.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); });
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();