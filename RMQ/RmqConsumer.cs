using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RMQ.Contracts;

namespace RMQ;

public interface IRmqConsumer : IDisposable
{
    public Task SubscribeToGame<T>(string queueName, StartGameCommand command,
        Func<T, StartGameCommand, Task> messageProcessor,
        CancellationToken cancellationToken);

    Task<T?> BasicGet<T>(string queueName);
}

public class RmqConsumer : RmqClientBase, IRmqConsumer
{
    private readonly ILogger<RmqConsumer> logger;
    private EventingBasicConsumer consumer;

    public RmqConsumer(ConnectionFactory connectionFactory,
        ILogger<RmqConsumer> logger) : base(connectionFactory, logger)
    {
        this.logger = logger;

        Channel.CallbackException += (sender, args) => { logger.LogError("Channel error: {@error}", args.Exception); };
    }

    public async Task<T?> BasicGet<T>(string queueName)
    {
        Channel.QueueDeclare(queueName, true, false);
        var data = Channel.BasicGet(queueName, true);
        var str = data?.Body.ToArray();

        if (str is null) return default;

        var message = Encoding.UTF8.GetString(str);
        var msg = JsonConvert.DeserializeObject<T>(message);

        logger.LogInformation("Got message -> {@msg}", msg);
        return msg;
    }

    public async Task SubscribeToGame<T>(string queueName, StartGameCommand command,
        Func<T, StartGameCommand, Task> messageProcessor,
        CancellationToken cancellationToken)
    {
        try
        {
            consumer = new EventingBasicConsumer(Channel);

            consumer.Received += async (sender, @event) =>
            {
                await OnEventReceived(sender, command, @event, messageProcessor);
            };

            Channel.QueueDeclare(queueName, true, false);
            ;

            Channel.BasicQos(0, 1, false);
            Channel.BasicConsume(queueName, false, consumer);

            logger.LogInformation("Subscribed to queue: {@queue}", queueName);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while consuming message");
        }
    }

    private async Task OnEventReceived<T>(object sender, StartGameCommand command, BasicDeliverEventArgs @event,
        Func<T, StartGameCommand, Task> messageProcessor)
    {
        var body = Encoding.UTF8.GetString(@event.Body.ToArray());
        try
        {
            var message = JsonConvert.DeserializeObject<T>(body);

            logger.LogDebug("Rmq: got message {@message}", message);
            await messageProcessor.Invoke(message, command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving message from queue. message: {@message}", body);
        }
        finally
        {
            Channel.BasicAck(@event.DeliveryTag, false);
        }
    }
}