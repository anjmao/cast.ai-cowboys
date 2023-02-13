using Cowboys.Contracts;
using Cowboys.DataAccess;
using Microsoft.AspNetCore.Mvc;
using RMQ;

namespace Cowboys.State.Api.Controllers;

[ApiController]
[Route("internal/cowboys")]
public class InternalCowboysController : ControllerBase
{
    private readonly IRmqGameClient gameClient;
    private readonly GameUnitOfWork gameUnitOfWork;

    public InternalCowboysController(GameUnitOfWork gameUnitOfWork, IRmqGameClient gameClient)
    {
        this.gameUnitOfWork = gameUnitOfWork;
        this.gameClient = gameClient;
    }

    [HttpGet]
    [Route("available/{gameId:guid}")]
    public async Task<string> GetAvailableCowboy([FromQuery] Guid gameId)
    {
        var cowboy = await gameUnitOfWork.GetAvailableCowboyForGame(gameId);
        return cowboy;
    }

    [HttpPost]
    [Route("shoot")]
    public async Task ShootEnemyAsync([FromBody] ShootCowboyRequest dto)
    {
        await gameUnitOfWork.ShootCowboyAsync(dto);
    }

    [HttpGet]
    [Route("enemies")]
    public async Task<IEnumerable<InGameCowboyResponseDto>> GetEnemiesAsync([FromQuery] CowboyQueryDto query)
    {
        var enemies = await gameUnitOfWork.GetCowboysToShootAsync(query.GameId, query.Name);

        var toReturn = enemies.Select(x => new InGameCowboyResponseDto
        {
            Name = x.Name,
            Health = x.Health
        });

        return toReturn;
    }

    [HttpPost]
    [Route("ready")]
    public async Task MarkReadyAsync(MarkCowboyReadyDto readyDto)
    {
        await gameUnitOfWork.MarkCowboyAsReadyAsync(readyDto.GameId, readyDto.Name);
    }

    [HttpPost]
    [Route("win")]
    public async Task MarkWinnerAsync(MarkCowboyWinDto dto)
    {
        await gameUnitOfWork.MarkCowboyAsWinnerAsync(dto.GameId, dto.Name);
    }

    [HttpGet]
    [Route("stats")]
    public async Task<ReturnCowboyStatsDto> GetCowboyStats([FromQuery] QueryCowboyStatsDto queryCowboyStatsDto)
    {
        var cowboy =
            await gameUnitOfWork.GetCowboyStatsAsync(queryCowboyStatsDto.GameId, queryCowboyStatsDto.CowboyName);

        var toReturn = new ReturnCowboyStatsDto
        {
            Health = cowboy.Health,
            Damage = cowboy.Damage
        };

        return toReturn;
    }
}