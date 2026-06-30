using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;

public static class MinimalEndpointExtensions
{
    public static IServiceCollection AddMinimalEndpoints(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
        }

        var endpointTypes = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                !t.IsGenericTypeDefinition &&
                typeof(IMinimalEndpointDefinition).IsAssignableFrom(t))
            .ToList();

        foreach (var endpointType in endpointTypes)
        {
            services.AddSingleton(typeof(IMinimalEndpointDefinition), endpointType);
        }

        return services;
    }
    
    public static IEndpointRouteBuilder MapMinimalEndpoints(
        this IEndpointRouteBuilder builder)
    {
        var endpoints = builder.ServiceProvider
            .GetServices<IMinimalEndpointDefinition>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return builder;
    }
}