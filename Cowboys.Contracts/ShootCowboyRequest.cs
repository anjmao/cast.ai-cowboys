namespace Cowboys.Contracts;

public record ShootCowboyRequest
{
    public Guid GameId { get; set; }

    public string Name { get; set; }
    public int Damage { get; set; }
    public string RequestorName { get; set; }
}