using HubManagement.BuildingBlock.Core.DbSettings;
using HubManagement.BuildingBlock.Core.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace HubManagement.BuildingBlock.Infrastructure.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddHeroMessaging<TMarker>(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbSettings = configuration
            .GetSection(nameof(PostGreSqlSetting))
            .Get<PostGreSqlSetting>();

        var options = configuration
            .GetSection(nameof(RebusOptions))
            .Get<RebusOptions>() ?? new RebusOptions();

        services.AddRebus(config => config
            .Transport(t => t.UsePostgreSql(
                connectionString: dbSettings?.ConnectionString,
                tableName: options.MessagesTableName,
                inputQueueName: options.QueueName))
            .Subscriptions(s => s.StoreInPostgres(
                connectionString: dbSettings?.ConnectionString,
                tableName: options.SubscriptionsTableName,
                isCentralized: true))
            .Routing(r => r.TypeBased()
                .MapFallback(options.QueueName)) 
            // .Routing(r =>
            // {
            //     var routing = r.TypeBased();
            //
            //     var eventTypes = typeof(TMarker).Assembly
            //         .GetTypes()
            //         .Where(t =>
            //             t.IsClass &&
            //             !t.IsAbstract &&
            //             !t.IsGenericTypeDefinition &&
            //             typeof(IIntegrationEvent).IsAssignableFrom(t));
            //
            //     foreach (var eventType in eventTypes)
            //     {
            //         routing.Map(eventType, options.QueueName);
            //     }
            // })
            .Options(o =>
            {
                o.SetNumberOfWorkers(options.NumberOfWorkers);
                o.SetMaxParallelism(options.MaxParallelism);
            })
            .Logging(l => l.Serilog()));

        services.AutoRegisterHandlersFromAssemblyOf<TMarker>();

        return services;
    }
}