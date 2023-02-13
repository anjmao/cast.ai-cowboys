using Microsoft.Extensions.DependencyInjection;

namespace Cowboys.Http.Sdk;

public class CowboysHttpApiSettings
{
    public static string Position = "CowboysHttpApi";

    public string Url { get; set; }
}

public static class CowboysHttpSdkExtensions
{
    public static void AddCowboysHttpApi(this IServiceCollection services,
        Action<CowboysHttpApiSettings> settingsAction)
    {
        var settings = new CowboysHttpApiSettings();
        settingsAction.Invoke(settings);

        services.AddHttpClient<CowboysApiClient>(x => { x.BaseAddress = new Uri(settings.Url); });
    }
}