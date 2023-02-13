using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RMQ.Contracts;

namespace RMQ;

public interface IRmqGameClient
{
    Task StartGameAsync(Guid gameId, List<string> cowboys);

    Task StartRoundAsync(StartRoundCommand command, List<string> cowboys);
}

public class RmqGameClient : RmqClientBase, IRmqGameClient
{
    private readonly ILogger<RmqGameClient> logger;

    public RmqGameClient(ConnectionFactory connectionFactory, ILogger<RmqGameClient> logger) : base(connectionFactory,
        logger)
    {
        this.logger = logger;
    }

    public async Task StartGameAsync(Guid gameId, List<string> cowboys)
    {
        var exchangeName = Exchange.GetGameExchange();

        Channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, true);
        var queueName = Exchange.GetStartGameQueue();

        Channel.QueueDeclare(queueName, true, false);
        Channel.QueueBind(queueName, exchangeName, Exchange.StartGameRoutingKey);

        foreach (var cowboy in cowboys)
        {
            var roundExchange = Exchange.GetGameRoundExchange(gameId.ToString());
            Channel.ExchangeDeclare(roundExchange, ExchangeType.Topic, true);
            var cowboyQueue = Exchange.GetStartRoundCowboyQueue(gameId.ToString(), cowboy);
            Channel.QueueDeclare(cowboyQueue, true, false);
            Channel.QueueBind(cowboyQueue, roundExchange, cowboy);

            Publish(new StartGameCommand
                {
                    CowboyName = cowboy,
                    GameId = gameId
                }, exchangeName,
                Exchange.StartGameRoutingKey);
        }
    }

    public async Task StartRoundAsync(StartRoundCommand command, List<string> cowboys)
    {
        var exchangeName = Exchange.GetGameRoundExchange(command.GameId.ToString());
        Channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, true);

        foreach (var cowboy in cowboys) Publish(command, exchangeName, cowboy);
    }

    protected virtual void Publish<T>(T @event, string exchange, string routingKey)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

            var properties = Channel.CreateBasicProperties();
            properties.ContentType = "application/json";
            properties.DeliveryMode = 1; // Doesn't persist to disk
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Channel.BasicPublish(exchange, routingKey, body: body, basicProperties: properties);
            logger.LogDebug("Published message to RMQ {@event}, {@exchange}, @{@routingKey}", @event, exchange,
                routingKey);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error while publishing {@error}", ex);
        }
    }
}