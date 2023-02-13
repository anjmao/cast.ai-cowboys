namespace RMQ;

public static class Exchange
{
    private const string GameStartExchange = "GameStart";
    private const string GameRoundExchange = "GameRoud";
    private const string StartGameQueue = "StartGameCowboyQueue";
    private const string StartRoundQueue = "StartRoundCowboyQueue";

    public const string StartGameRoutingKey = "StartGame";

    public static string GetGameExchange()
    {
        return GameStartExchange;
    }

    public static string GetGameRoundExchange(string gameId)
    {
        return GameRoundExchange + gameId;
    }

    public static string GetStartGameQueue()
    {
        return StartGameQueue;
    }

    public static string GetStartRoundCowboyQueue(string gameId, string cowboy)
    {
        return StartRoundQueue + gameId + cowboy;
    }
}