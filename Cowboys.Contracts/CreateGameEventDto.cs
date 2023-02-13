namespace Cowboys.Contracts;

public record CreateGameEventDto
{
    public DateTime EventTime { get; set; }
    public string EventType { get; set; }
    public string EventDescription { get; set; }

    public Guid GameId { get; set; }

    public string Description { get; set; }
}