using Cowboys.Contracts;
using Cowboys.DataAccess;
using Microsoft.AspNetCore.Mvc;
using RMQ;

namespace Cowboys.State.Api.Controllers;

[ApiController]
[Route("cowboys")]
public class CowboysController : ControllerBase
{
    private readonly IRmqGameClient gameClient;
    private readonly GameUnitOfWork gameUnitOfWork;

    public CowboysController(GameUnitOfWork gameUnitOfWork, IRmqGameClient gameClient)
    {
        this.gameUnitOfWork = gameUnitOfWork;
        this.gameClient = gameClient;
    }

    /// <summary>
    ///     Create Game
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateGame()
    {
        var (gameId, cowboys) = await gameUnitOfWork.CreateGame();
        await gameClient.StartGameAsync(gameId, cowboys);
        return Ok(gameId);
    }

    /// <summary>
    ///     Get Game Results
    /// </summary>
    /// <param name="gameId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("results/game/{gameId:guid}")]
    public async Task<ActionResult<GameResultsDto>> GetGameResults(Guid gameId)
    {
        var game = await gameUnitOfWork.GetGameResultsAsync(gameId);

        var logs = game.GameEvents
            .OrderBy(x => x.EventTime).Select(@event =>
                $"Time: {@event.EventTime}, EventType:{@event.EventType}, Description: {@event.Description}");

        var toReturn = new GameResultsDto
        {
            Status = game.Status,
            Logs = logs
        };

        return Ok(toReturn);
    }
}