using System.Text.Json.Serialization;
using HubManagement.Application;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Infrastructure;
using HubManagement.BuildingBlock.Infrastructure.FileStorage;
using HubManagement.BuildingBlock.Infrastructure.Messaging;
using HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;
using HubManagement.Infrastructure.Installers;
using HubManagement.WebApi.Endpoints.V1.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("Configurations/appsettings.json", false, true)
    .AddJsonFile(
        $"Configurations/appsettings.{builder.Environment.EnvironmentName}.json",
        true,
        true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Configuration.AddEnvironmentVariables();

// Keep API enum values stable and readable for clients.
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
    o.EnableMailing = true;
});

builder.Services.AddHeroMessaging<IHubManagementApplicationMaker>(builder.Configuration);

builder.Services.AddBackgroundJob();

builder.Services.AddMinimalEndpoints(
    typeof(IdentityEndpointRegistration).Assembly
);

builder.Services.AddStorage(builder.Configuration);
builder.Services.ServicesRegisterExtensions();
builder.Services.AddPersistence(builder.Configuration);

builder.Services.ConfigureJwtAuth();

var app = builder.Build();

app.UsePlatform();

app.MapMinimalEndpoints();

var migrateOnStartup = app.Environment.IsDevelopment()
                       || app.Configuration.GetValue<bool>("Database:MigrateOnStartup");

if (migrateOnStartup)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.MigrateAsync(CancellationToken.None);
    await initializer.SeedAsync(CancellationToken.None);
}

await app.RunAsync();
