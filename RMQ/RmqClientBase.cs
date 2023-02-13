using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RMQ;

public abstract class RmqClientBase : IDisposable
{
    private readonly ConnectionFactory connectionFactory;
    private readonly ILogger<RmqClientBase> logger;
    private IConnection connection;

    protected RmqClientBase(
        ConnectionFactory connectionFactory,
        ILogger<RmqClientBase> logger)
    {
        this.connectionFactory = connectionFactory;
        this.logger = logger;
        ConnectToRabbitMq();
    }

    protected IModel Channel { get; private set; }

    public void Dispose()
    {
        try
        {
            Channel?.Close();
            Channel?.Dispose();
            Channel = null;

            connection?.Close();
            connection?.Dispose();
            connection = null;
        }
        catch (Exception ex)
        {
            logger.LogCritical("Cannot dispose RabbitMQ channel or connection {@error}", ex);
        }
    }

    private void ConnectToRabbitMq()
    {
        if (connection == null || connection.IsOpen == false)
        {
            connection = connectionFactory.CreateConnection();
            logger.LogInformation("Established RMQ Connection");
        }

        if (Channel == null || Channel.IsOpen == false)
        {
            Channel = connection.CreateModel();
            logger.LogInformation("Created RMQ Channel");
        }
    }
}