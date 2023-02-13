using Cowboys.DataAccess.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cowboys.DataAccess;

public static class DataAccessExtensions
{
    public static void AddCowboysDataAccess(this IServiceCollection services, Action<DataAccessSettings> dataSettings)
    {
        var settings = new DataAccessSettings();
        dataSettings.Invoke(settings);

        services.Configure(dataSettings);

        services.AddDbContext<CowboysDbContext>(
            options => options.UseNpgsql(settings.ConnectionString)
                .UseSnakeCaseNamingConvention());

        services.AddTransient<CowboyRepository>();
        services.AddTransient<GameEventsRepository>();
        services.AddTransient<GameRepository>();
        services.AddTransient<InGameCowboysRepository>();
        services.AddTransient<GameUnitOfWork>();
    }

    public static void RunDatabaseMigrations<TDbContext>(this WebApplication app) where TDbContext : DbContext
    {
        var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<TDbContext>();

        if (db.Database.IsRelational()) db.Database.Migrate();
    }
}