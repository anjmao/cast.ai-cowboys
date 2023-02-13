using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Cowboys.DataAccess;

public class CowboysDbContext : DbContext
{
    public CowboysDbContext(DbContextOptions<CowboysDbContext> options) : base(options)
    {
    }

    public DbSet<Cowboy> Cowboys { get; set; }
    public DbSet<GameEvent> GameEvents { get; set; }
    public DbSet<Game> Game { get; set; }

    public DbSet<InGameCowboy> InGameCowboys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var cowboys = JsonConvert.DeserializeObject<List<Cowboy>>(GetCowboys());

        modelBuilder.Entity<Cowboy>().HasKey(x => x.Name);
        modelBuilder.Entity<Cowboy>().HasData(cowboys);

        modelBuilder.Entity<InGameCowboy>().HasKey(x => new {x.GameId, x.Name});
        modelBuilder.Entity<Game>().HasKey(x => x.Id);
        modelBuilder.Entity<GameEvent>().HasKey(x => x.Id);

        base.OnModelCreating(modelBuilder);
    }

    private string GetCowboys()
    {
        return """
          [
            {
              "name": "John",
              "health": 10,
              "damage": 1
            },
            {
              "name": "Bill",
              "health": 8,
              "damage": 2
            },
            {
              "name": "Sam",
              "health": 10,
              "damage": 1
            },
            {
              "name": "Peter",
              "health": 5,
              "damage": 3
            },
            {
              "name": "Philip",
              "health": 15,
              "damage": 1
            }
          ]
    """;
    }
}