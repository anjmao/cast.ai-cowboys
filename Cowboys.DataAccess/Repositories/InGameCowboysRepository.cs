using Microsoft.EntityFrameworkCore;

namespace Cowboys.DataAccess;

public class InGameCowboysRepository
{
    private readonly CowboysDbContext dbContext;

    public InGameCowboysRepository(CowboysDbContext cowboysDbContext)
    {
        dbContext = cowboysDbContext;
    }

    public async Task<int> DamageCowboyAsync(Guid gameId, string cowboyName, int damage)
    {
        var cowboy = await dbContext.InGameCowboys.FirstAsync(x => x.GameId == gameId && x.Name == cowboyName);

        if (cowboy.Health > 0) cowboy.Health -= damage;

        return cowboy.Health;
    }

    public async Task MarkCowboyAsReadyAsync(Guid gameId, string cowboyName)
    {
        var cowboy = await dbContext
            .InGameCowboys
            .FirstAsync(x => x.GameId == gameId && x.Name == cowboyName);

        cowboy.isReady = true;

        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> AllCowboysAreReady(Guid gameId)
    {
        var cowboysAreReady = await dbContext.InGameCowboys.Where(x => x.GameId == gameId).AllAsync(x => x.isReady);
        return cowboysAreReady;
    }

    public async Task<IEnumerable<InGameCowboy>> GetCowboysToShoot(Guid gameId, string cowboyName)
    {
        var enemies = await dbContext.InGameCowboys
            .Where(x => x.GameId == gameId && x.Health > 0 && x.Name != cowboyName)
            .AsNoTracking()
            .ToListAsync();

        return enemies;
    }

    public async Task<InGameCowboy> GetCowboyStatsAsync(Guid gameId, string name)
    {
        return await dbContext.InGameCowboys
            .AsNoTracking()
            .FirstAsync(x => x.GameId == gameId && x.Name == name);
    }

    public async Task<InGameCowboy> GetNotSelectedCowboy(Guid gameId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var cowboy = await dbContext.InGameCowboys.FirstAsync(x => x.GameId == gameId && x.isSelected == false);

        cowboy.isSelected = true;

        await dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
        return cowboy;
    }

    public async Task<List<string>> GetAll(Guid gameId)
    {
        var cw = await dbContext.InGameCowboys.Where(x => x.GameId == gameId).Select(x => x.Name).ToListAsync();
        return cw;
    }

    public async Task<bool> AreAllCowboysDead(Guid gameId)
    {
        return await dbContext.InGameCowboys.Where(x => x.GameId == gameId).AllAsync(x => x.Health <= 0);
    }
}