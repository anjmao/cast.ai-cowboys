using System.Security.Cryptography;
using Cowboys.Contracts;
using Cowboys.Http.Sdk;
using RMQ;
using RMQ.Contracts;

namespace Cowboys.Worker;

public class CowboyWorker : IHostedService
{
    private readonly CowboysApiClient api;
    private readonly ILogger<CowboyWorker> logger;
    private readonly IRmqConsumer rmqConsumer;

    public CowboyWorker(IRmqConsumer rmqConsumer, CowboysApiClient api, ILogger<CowboyWorker> logger)
    {
        this.rmqConsumer = rmqConsumer;
        this.api = api;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var command = await rmqConsumer.BasicGet<StartGameCommand>(Exchange.GetStartGameQueue());

                if (command is null)
                {
                    logger.LogInformation("No new game started");
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                    continue;
                }

                await api.MarkReadyAsync(new MarkCowboyReadyDto
                {
                    GameId = command.GameId,
                    Name = command.CowboyName
                });

                await SubscribeToGame(cancellationToken, command);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to execute flow");
            }

            logger.LogInformation("Waiting for messages");
            await Task.Delay(500, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task SubscribeToGame(CancellationToken cancellationToken, StartGameCommand command)
    {
        var queue = Exchange.GetStartRoundCowboyQueue(command.GameId.ToString(), command.CowboyName);

        await rmqConsumer.SubscribeToGame<StartRoundCommand>(queue, command,
            async (round, c) => { await PlayRound(c, round); }, cancellationToken);
    }

    private async Task PlayRound(StartGameCommand c, StartRoundCommand round)
    {
        var query = new QueryCowboyStatsDto
        {
            CowboyName = c.CowboyName,
            GameId = c.GameId
        };

        var currentStats = await api.GetCowboyStatsAsync(query);
        var cowboyIsDead = currentStats.Health <= 0;

        if (cowboyIsDead)
        {
            logger.LogDebug("Im so dead {@cowboy}", query);
            return;
        }

        var enemies = await api.GetEnemiesAsync(new CowboyQueryDto
        {
            GameId = round.GameId,
            Name = c.CowboyName
        });


        var count = enemies?.Count() ??
                    throw new ArgumentNullException($"Enemies are null: {nameof(StartAsync)}");

        if (count == 0 && !cowboyIsDead)
        {
            logger.LogInformation("I WON {@cowboy}", query);
            await api.MarkWinnerAsync(new MarkCowboyWinDto
            {
                GameId = c.GameId,
                Name = c.CowboyName
            });
            return;
        }

        var cowboyToShoot = SelectCowboyToShoot(count, enemies);

        await api.ShootEnemyAsync(new ShootCowboyRequest
        {
            GameId = c.GameId,
            Damage = currentStats.Damage,
            Name = cowboyToShoot.Name,
            RequestorName = c.CowboyName
        });
    }

    private static InGameCowboyResponseDto SelectCowboyToShoot(int count, IEnumerable<InGameCowboyResponseDto> enemies)
    {
        var getRandomNumber = RandomNumberGenerator.GetInt32(0, count);
        var cowboyToShoot = enemies.ToArray()[getRandomNumber];
        return cowboyToShoot;
    }
}