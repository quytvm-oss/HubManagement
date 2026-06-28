using HubManagement.BuildingBlock.Infrastructure.Cache.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace HubManagement.BuildingBlock.Infrastructure.Cache;

public static class Extensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        services
            .AddOptions<CachingOptions>()
            .BindConfiguration(nameof(CachingOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        // Always and memory cache for L1
        services.AddMemoryCache();

        var cacheOptions = configuration.GetSection(nameof(CachingOptions)).Get<CachingOptions>();
        if (cacheOptions is null || string.IsNullOrEmpty(cacheOptions.Redis))
        {
            // if no redis, use memory cache for L2 as well
            services.AddDistributedMemoryCache();
            services.AddTransient<ICacheService, HubManagement.BuildingBlock.Infrastructure.Cache.DistributedCacheService>();
            return services;
        }
        
        // use redis for L2
        services.AddStackExchangeRedisCache(options =>
        {
            var config = ConfigurationOptions.Parse(cacheOptions.Redis);

            // Only override SSL if xplicitly configured
            if (cacheOptions.EnableSsl.HasValue)
            {
                config.Ssl = cacheOptions.EnableSsl.Value;
            }

            options.ConfigurationOptions = config;
        });
        
        // Register hybrid cache service
        services.AddTransient<ICacheService, HybridCacheService>();
        
        return services;
    }
}