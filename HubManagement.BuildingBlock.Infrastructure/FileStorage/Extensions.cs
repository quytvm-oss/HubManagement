using HubManagement.BuildingBlock.Infrastructure.FileStorage.Local;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HubManagement.BuildingBlock.Infrastructure.FileStorage;

public static class Extensions
{
    public static IServiceCollection AddHeroStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StorageOptions>().BindConfiguration(nameof(StorageOptions))
            .ValidateDataAnnotations().ValidateOnStart();
        
        var storageOptions = configuration.GetSection(nameof(StorageOptions)).Get<StorageOptions>() ?? new StorageOptions();

        switch (storageOptions.Provider?.Trim().ToLowerInvariant())
        {
            case "local":
                services.AddHeroLocalFileStorage(configuration);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported storage provider: '{storageOptions.Provider}'. Allowed: Local, S3.");
        }
        return services;
    }
    
    private static IServiceCollection AddHeroLocalFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<LocalStorageOptions>().BindConfiguration("Storage:Local")
            .Validate(o => !string.IsNullOrWhiteSpace(o.StorageRoot),
                "Storage:Local:StorageRoot is required when using Local storage.")
            .ValidateDataAnnotations().ValidateOnStart();
        
        services.AddScoped<IStorageService, LocalStorageService>();
        return services;
    }
}