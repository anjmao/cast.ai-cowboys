using Cowboys.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Cowboys.DataAccess;

public class GameRepository
{
    private readonly CowboysDbContext dbContext;

    public GameRepository(CowboysDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task CreateGameAsync(Game model)
    {
        await dbContext.Game.AddAsync(model);
    }

    public async Task UpdateGameStatus(Guid gameId, string status)
    {
        var game = await dbContext.Game.FirstAsync(x => x.Id == gameId);
        game.Status = status;

        await dbContext.SaveChangesAsync();
    }

    public async Task<Game> GetGameAsync(Guid id)
    {
        var game = await dbContext.Game
            .Include(x => x.GameEvents)
            .Include(x => x.Cowboys)
            .AsNoTracking()
            .FirstAsync(x => x.Id == id);

        return game;
    }

    public async Task<List<Guid>> GetAllNotFinishedGamesAsync()
    {
        var games = await dbContext.Game.Where(x => x.Status != GameStatus.Finished).Select(x => x.Id).ToListAsync();

        return games;
    }
}