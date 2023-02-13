using Cowboys.DataAccess;
using Cowboys.Telemetry;
using Cowboys.Worker.Manager;
using RMQ;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builder, services) =>
    {
        services.AddOpenTelemetryTraces(builder.Configuration);
        services.AddCowboysDataAccess(x => builder.Configuration.GetSection(DataAccessSettings.Position).Bind(x));
        services.AddRabbitMq(x => builder.Configuration.GetSection("RMQ").Bind(x));

        services.AddHostedService<CowboyTaskManager>();
    })
    .Build();

host.Run();