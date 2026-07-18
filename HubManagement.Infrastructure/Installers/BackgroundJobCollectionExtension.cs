using HubManagement.Application.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;

namespace HubManagement.Infrastructure.Installers;

public static class BackgroundJobCollectionExtension
{
    public static IServiceCollection AddBackgroundJob(this IServiceCollection services)
    {
        services.AddHostedService<RebusSubscriptionHostedService>();
        return services;
    }
}