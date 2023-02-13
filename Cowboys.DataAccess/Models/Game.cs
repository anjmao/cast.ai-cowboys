using Microsoft.EntityFrameworkCore;

namespace Cowboys.DataAccess;

[Index(nameof(Id), IsUnique = true)]
public class Game
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public List<InGameCowboy> Cowboys { get; set; } = new();
    public List<GameEvent> GameEvents { get; set; } = new();
}

[Index(nameof(GameId), nameof(Name), IsUnique = true)]
public class InGameCowboy
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int Damage { get; set; }

    public Guid GameId { get; set; }
    public Game Game { get; set; }
    public bool isSelected { get; set; }
    public bool isReady { get; set; }
}

public class GameEvent
{
    public Guid Id { get; set; }
    public DateTime EventTime { get; set; }
    public string EventType { get; set; }
    public string Description { get; set; }

    public Guid GameId { get; set; }
    public Game Game { get; set; }
}