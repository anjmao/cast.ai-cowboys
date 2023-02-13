using Microsoft.EntityFrameworkCore;

namespace Cowboys.DataAccess.Repositories;

public class CowboyRepository
{
    private readonly CowboysDbContext dbContext;

    public CowboyRepository(CowboysDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<Cowboy>> GetCowboysAsync()
    {
        var cowboys = await dbContext.Cowboys.AsNoTracking().ToListAsync();
        return cowboys;
    }
}