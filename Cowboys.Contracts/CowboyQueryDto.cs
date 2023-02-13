namespace Cowboys.Contracts;

public record CowboyQueryDto
{
    public string Name { get; set; }
    public Guid GameId { get; set; }
}

public record InGameCowboyResponseDto
{
    public string Name { get; set; }
    public int Health { get; set; }
}