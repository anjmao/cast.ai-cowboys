namespace Cowboys.Contracts;

public class MarkCowboyReadyDto
{
    public string Name { get; set; }
    public Guid GameId { get; set; }
}

public class MarkCowboyWinDto
{
    public string Name { get; set; }
    public Guid GameId { get; set; }
}