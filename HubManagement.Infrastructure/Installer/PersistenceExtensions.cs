using HubManagement.BuildingBlock.Core.DbSettings;
using HubManagement.Infrastructure.Inteceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HubManagement.Infrastructure.Installer;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<PostGreSqlSetting>().Bind(configuration.GetSection(nameof(PostGreSqlSetting)))
            .ValidateDataAnnotations().Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), 
                "PostGreSqlSetting.ConnectionString is required.")
            .ValidateOnStart();
        
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ISaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DomainEventsInterceptor>();
        return services;
    }
}