var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OpenTelemetryDemo_WebApi>("opentelemetrydemo.webapi");

builder.Build().Run();
