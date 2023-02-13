using Cowboys.Contracts;
using Cowboys.DataAccess;
using RMQ;
using RMQ.Contracts;

namespace Cowboys.Worker.Manager;

public class CowboyTaskManager : IHostedService
{
    private readonly IRmqGameClient gameClient;
    private readonly ILogger<CowboyTaskManager> logger;

    private readonly IServiceProvider serviceProvider;


    public CowboyTaskManager(IRmqGameClient gameClient,
        ILogger<CowboyTaskManager> logger, IServiceProvider serviceProvider)
    {
        this.gameClient = gameClient;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var gameUnitOfWork = scope.ServiceProvider.GetService<GameUnitOfWork>() ??
                                     throw new ArgumentNullException(
                                         $"Failed to retrieve {nameof(GameUnitOfWork)} in {nameof(CowboyTaskManager)}");

                await StartGames(gameUnitOfWork);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to execute CowboyTaskManager");
            }

            logger.LogDebug("Waiting for 1 second");
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task StartGames(GameUnitOfWork gameUnitOfWork)
    {
        var games = await gameUnitOfWork.GetAllNotFinishedGamesAsync();

        logger.LogInformation($"In Progress game count: {games.Count}");

        foreach (var game in games)
        {
            var g = await gameUnitOfWork.GetGameResultsAsync(game);

            if (g.Status == GameStatus.Pending)
                await gameUnitOfWork.TryStartGameAsync(game);
            else if (g.Status == GameStatus.InProgress)
                await StartGameRound(gameUnitOfWork, game);
            else
                logger.LogDebug($"Status: {g.Status} is not registered");
        }
    }

    private async Task StartGameRound(GameUnitOfWork gameUnitOfWork, Guid game)
    {
        var cowboys = await gameUnitOfWork.GetAllInGameCowboys(game);

        if (await gameUnitOfWork.AreAllCowboysDead(game))
        {
            await gameUnitOfWork.EndGameAsync(game);
            return;
        }

        await gameUnitOfWork.MarkRoundStarted(game);
        await gameClient.StartRoundAsync(new StartRoundCommand
        {
            GameId = game
        }, cowboys);
    }
}