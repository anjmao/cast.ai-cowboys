using Cowboys.Http.Sdk;
using Cowboys.Telemetry;
using Cowboys.Worker;
using RMQ;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builder, services) =>
    {
        services.AddOpenTelemetryTraces(builder.Configuration);
        services.AddRabbitMq(x => builder.Configuration.GetSection("RMQ").Bind(x));
        services.AddCowboysHttpApi(x => builder.Configuration.GetSection(CowboysHttpApiSettings.Position).Bind(x));
        services.AddHostedService<CowboyWorker>();
    })
    .Build();

host.Run();