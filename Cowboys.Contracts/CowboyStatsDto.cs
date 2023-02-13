namespace Cowboys.Contracts;

public record QueryCowboyStatsDto
{
    public Guid GameId { get; set; }
    public string CowboyName { get; set; }
}

public record ReturnCowboyStatsDto
{
    public int Health { get; set; }
    public int Damage { get; set; }
}