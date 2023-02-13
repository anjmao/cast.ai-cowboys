namespace RMQ.Contracts;

public class StartGameCommand
{
    public Guid GameId { get; set; }
    public string CowboyName { get; set; }
}