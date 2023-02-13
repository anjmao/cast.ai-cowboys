using Cowboys.Contracts;
using Cowboys.DataAccess.Repositories;

namespace Cowboys.DataAccess;

public class GameUnitOfWork
{
    private readonly CowboyRepository cowboyRepository;
    private readonly CowboysDbContext dbContext;
    private readonly GameEventsRepository gameEventsRepository;
    private readonly GameRepository gameRepository;
    private readonly InGameCowboysRepository inGameCowboysRepository;

    public GameUnitOfWork(CowboyRepository cowboyRepository, GameRepository gameRepository,
        GameEventsRepository gameEventsRepository, InGameCowboysRepository inGameCowboysRepository,
        CowboysDbContext dbContext)
    {
        this.cowboyRepository = cowboyRepository;
        this.gameRepository = gameRepository;
        this.gameEventsRepository = gameEventsRepository;
        this.inGameCowboysRepository = inGameCowboysRepository;
        this.dbContext = dbContext;
    }

    public async Task<(Guid, List<string>)> CreateGame()
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var cowboys = await cowboyRepository.GetCowboysAsync();
        var gameId = Guid.NewGuid();

        var inGameCowboys = cowboys.Select(x => new InGameCowboy
        {
            Name = x.Name,
            Damage = x.Damage,
            Health = x.Health,
            isReady = false,
            GameId = gameId
        }).ToList();

        var game = new Game
        {
            Id = gameId,
            Cowboys = inGameCowboys,
            Status = GameStatus.Pending
        };

        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(CreateGame),
            Description = $"Game with id {gameId} is created."
        });


        await gameRepository.CreateGameAsync(game);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return (gameId, inGameCowboys.Select(x => x.Name).ToList());
    }

    public async Task MarkRoundStarted(Guid gameId)
    {
        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(MarkRoundStarted),
            Description = "--------------------- ROUND STARTED ---------------------"
        });

        await dbContext.SaveChangesAsync();
    }

    public async Task MarkCowboyAsReadyAsync(Guid gameId, string cowboyName)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(MarkCowboyAsReadyAsync),
            Description = $"Cowboy: {cowboyName} reported ready status"
        });

        await inGameCowboysRepository.MarkCowboyAsReadyAsync(gameId, cowboyName);
        await transaction.CommitAsync();
    }

    public async Task<IEnumerable<InGameCowboy>> GetCowboysToShootAsync(Guid gameId, string cowboyName)
    {
        var game = await gameRepository.GetGameAsync(gameId);

        if (game.Status == GameStatus.Finished) return Enumerable.Empty<InGameCowboy>();

        var enemies = await inGameCowboysRepository.GetCowboysToShoot(gameId, cowboyName);
        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(GetCowboysToShootAsync),
            Description = $"Cowboy: {cowboyName} requested ENEMIES LIST"
        });

        return enemies;
    }

    public async Task<Game> GetGameResultsAsync(Guid gameId)
    {
        var game = await gameRepository.GetGameAsync(gameId);
        return game;
    }

    public async Task<InGameCowboy> GetCowboyStatsAsync(Guid gameId, string name)
    {
        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(GetCowboyStatsAsync),
            Description = $"Cowboy: {name} STATS REQUESTED"
        });

        return await inGameCowboysRepository.GetCowboyStatsAsync(gameId, name);
    }

    public async Task ShootCowboyAsync(ShootCowboyRequest request)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var hp = await inGameCowboysRepository.DamageCowboyAsync(request.GameId, request.Name, request.Damage);
        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = request.GameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(ShootCowboyRequest),
            Description =
                $"Cowboy: {request.RequestorName} SHOT -> {request.Name}. Damage: {request.Damage}. Health left: {hp}"
        });

        await dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task<bool> TryStartGameAsync(Guid gameId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var allCowboysAreReady = await inGameCowboysRepository.AllCowboysAreReady(gameId);

        if (!allCowboysAreReady) return false;

        await gameRepository.UpdateGameStatus(gameId, GameStatus.InProgress);

        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(TryStartGameAsync),
            Description = "Game is starting"
        });

        await dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return true;
    }

    public async Task EndGameAsync(Guid gameId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        await gameRepository.UpdateGameStatus(gameId, GameStatus.Finished);
        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(EndGameAsync),
            Description = "Game is Finished"
        });

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<string> GetAvailableCowboyForGame(Guid gameId)
    {
        var cowboy = await inGameCowboysRepository.GetNotSelectedCowboy(gameId);

        return cowboy.Name;
    }

    public async Task<List<string>> GetAllInGameCowboys(Guid gameId)
    {
        return await inGameCowboysRepository.GetAll(gameId);
    }

    public async Task<bool> AreAllCowboysDead(Guid gameId)
    {
        return await inGameCowboysRepository.AreAllCowboysDead(gameId);
    }

    public async Task<List<Guid>> GetAllNotFinishedGamesAsync()
    {
        return await gameRepository.GetAllNotFinishedGamesAsync();
    }

    public async Task MarkCowboyAsWinnerAsync(Guid gameId, string name)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var cw = await inGameCowboysRepository.GetAll(gameId);

        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(MarkCowboyAsWinnerAsync),
            Description = $"WINNER IS {name}."
        });

        await gameRepository.UpdateGameStatus(gameId, GameStatus.Finished);
        await gameEventsRepository.StoreEventAsync(new GameEvent
        {
            GameId = gameId,
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            EventType = nameof(EndGameAsync),
            Description = "Game is Finished"
        });

        await transaction.CommitAsync();
    }
}