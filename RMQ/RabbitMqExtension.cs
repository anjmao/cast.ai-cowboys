using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace RMQ;

public static class RabbitMqExtension
{
    public static void AddRabbitMq(this IServiceCollection services, Action<RmqSettings> rmqSettings)
    {
        services.Configure(rmqSettings);

        var rmq = new RmqSettings();
        rmqSettings.Invoke(rmq);

        services.AddSingleton(new ConnectionFactory
        {
            HostName = rmq.Host,
            Password = rmq.Password,
            UserName = rmq.Username,
            Port = rmq.Port,
            VirtualHost = "/",
            ContinuationTimeout = TimeSpan.FromSeconds(30),
            RequestedHeartbeat = TimeSpan.FromSeconds(30)
        });

        services.AddTransient<IRmqConsumer, RmqConsumer>();
        services.AddTransient<IRmqGameClient, RmqGameClient>();
    }
}