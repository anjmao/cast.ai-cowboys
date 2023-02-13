namespace Cowboys.DataAccess;

public class GameEventsRepository
{
    private readonly CowboysDbContext context;

    public GameEventsRepository(CowboysDbContext context)
    {
        this.context = context;
    }

    public async Task StoreEventAsync(GameEvent model)
    {
        await context.GameEvents.AddAsync(model);
    }
}