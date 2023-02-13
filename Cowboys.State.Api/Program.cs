using System.Reflection;
using Cowboys.DataAccess;
using Cowboys.Telemetry;
using Microsoft.OpenApi.Models;
using RMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetryTraces(builder.Configuration);
builder.Services.AddCowboysDataAccess(x => builder.Configuration.GetSection(DataAccessSettings.Position).Bind(x));
builder.Services.AddRabbitMq(x => builder.Configuration.GetSection("RMQ").Bind(x));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    //The generated Swagger JSON file will have these properties.
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cowboys API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json",
        "Cowboys API");
});

app.UseAuthorization();

app.MapControllers();

app.RunDatabaseMigrations<CowboysDbContext>();

app.Run();