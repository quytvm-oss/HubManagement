using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
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
        var apiVersionSet = builder.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var apiGroup = builder
            .MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        var endpoints = builder.ServiceProvider
            .GetServices<IMinimalEndpointDefinition>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(apiGroup);
        }

        return builder;
    }
}