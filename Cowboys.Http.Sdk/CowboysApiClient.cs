using System.Text;
using System.Web;
using Cowboys.Contracts;
using Newtonsoft.Json;

namespace Cowboys.Http.Sdk;

public class CowboysApiClient
{
    private const string PathPrefix = "internal/cowboys";

    private readonly HttpClient httpClient;

    public CowboysApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task ShootEnemyAsync(ShootCowboyRequest dto)
    {
        var stringContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{PathPrefix}/shoot", stringContent);

        response.EnsureSuccessStatusCode();
    }

    public async Task MarkWinnerAsync(MarkCowboyWinDto dto)
    {
        var stringContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{PathPrefix}/win", stringContent);

        response.EnsureSuccessStatusCode();
    }

    public async Task MarkReadyAsync(MarkCowboyReadyDto dto)
    {
        var stringContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{PathPrefix}/ready", stringContent);

        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<InGameCowboyResponseDto>> GetEnemiesAsync(CowboyQueryDto dto)
    {
        var query = HttpUtility.ParseQueryString("");
        query["gameId"] = dto.GameId.ToString();
        query["name"] = dto.Name;
        var response = await httpClient.GetAsync($"{PathPrefix}/enemies?{query}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var toReturn = JsonConvert.DeserializeObject<IEnumerable<InGameCowboyResponseDto>>(content);

        if (toReturn is null)
            throw new ArgumentNullException($"Failed Deserialize: {nameof(GetEnemiesAsync)} response");

        return toReturn;
    }

    public async Task<ReturnCowboyStatsDto> GetCowboyStatsAsync(QueryCowboyStatsDto queryCowboyStatsDto)
    {
        var query = HttpUtility.ParseQueryString("");
        query["gameId"] = queryCowboyStatsDto.GameId.ToString();
        query["cowboyName"] = queryCowboyStatsDto.CowboyName;

        var response = await httpClient.GetAsync($"{PathPrefix}/stats?{query}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var toReturn = JsonConvert.DeserializeObject<ReturnCowboyStatsDto>(content);

        if (toReturn is null)
            throw new ArgumentNullException($"Failed Deserialize: {nameof(GetCowboyStatsAsync)} response");

        return toReturn;
    }
}