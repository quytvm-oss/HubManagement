using System.Text.Json.Serialization;
using HubManagement.Application;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Infrastructure;
using HubManagement.BuildingBlock.Infrastructure.Messaging;
using HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;
using HubManagement.Infrastructure.Installers;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("Configurations/appsettings.json", false, true)
    .AddJsonFile(
        $"Configurations/appsettings.{builder.Environment.EnvironmentName}.json",
        true,
        true)
    .AddEnvironmentVariables();

// Serialize enums as string names (reads still accept names or integers). [Flags] enums (AuditTag, BodyCapture)
// opt back to numeric via their own NumericEnumConverter since comma-joined flag strings break bitwise consumers. Frontends mirror this as string unions.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (builder.Environment.IsProduction())
{
    static void Require(IConfiguration config, string key)
    {
        if (string.IsNullOrWhiteSpace(config[key]))
        {
            throw new InvalidOperationException($"Missing required configuration '{key}' in Production.");
        }
    }

    var config = builder.Configuration;
    Require(config, "PostGreSqlSetting:ConnectionString");
    Require(config, "JwtOptions:SigningKey");
}

builder.Services.AddMediator(o =>
{
    o.ServiceLifetime = ServiceLifetime.Scoped;
    o.Assemblies =
    [
        typeof(IHubManagementApplicationMaker),
    ];
});

builder.AddPlatform(o =>
{
    o.EnableCaching = true;
});

builder.Services.AddHeroMessaging<IHubManagementApplicationMaker>(builder.Configuration);

builder.Services.AddMinimalEndpoints(
    typeof(IHubManagementApplicationMaker).Assembly
);

builder.Services.ServicesRegisterExtensions();
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

app.UsePlatform();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.MigrateAsync(CancellationToken.None);
    await initializer.SeedAsync(CancellationToken.None);
}

app.MapGet("/", () => Results.Ok(new { message = "hello world!" }))
    .WithTags("PlayGround")
    .AllowAnonymous();

await app.RunAsync();