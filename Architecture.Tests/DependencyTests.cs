using System.Reflection;
using HubManagement.Application;
using HubManagement.BuildingBlock.Core.Domain;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.DataContext;
using HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;
using HubManagement.WebApi.Endpoints.V1.Identity;

namespace Architecture.Tests;

public sealed class DependencyTests
{
    private static readonly Assembly Core = typeof(BaseEntity<>).Assembly;
    private static readonly Assembly Domain = typeof(ApplicationUser).Assembly;
    private static readonly Assembly Application = typeof(IHubManagementApplicationMaker).Assembly;
    private static readonly Assembly Infrastructure = typeof(HubDbContext).Assembly;

    [Fact]
    public void Core_must_not_depend_on_outer_layers()
        => AssertDoesNotReference(Core, "HubManagement.Domain", "HubManagement.Application",
            "HubManagement.Infrastructure", "HubManagement.WebApi");

    [Fact]
    public void Domain_must_not_depend_on_outer_layers()
        => AssertDoesNotReference(Domain, "HubManagement.Application", "HubManagement.Infrastructure",
            "HubManagement.BuildingBlock.Infrastructure", "HubManagement.WebApi");

    [Fact]
    public void Application_must_not_depend_on_infrastructure_or_host()
        => AssertDoesNotReference(Application, "HubManagement.Infrastructure",
            "HubManagement.BuildingBlock.Infrastructure", "HubManagement.WebApi");

    [Fact]
    public void Infrastructure_must_not_depend_on_host()
        => AssertDoesNotReference(Infrastructure, "HubManagement.WebApi");

    [Fact]
    public void WebApi_must_contain_minimal_endpoint_definitions()
    {
        var endpointTypes = typeof(IdentityEndpointRegistration).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && typeof(IMinimalEndpointDefinition).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(endpointTypes);
    }

    private static void AssertDoesNotReference(Assembly assembly, params string[] forbiddenAssemblies)
    {
        var references = assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToHashSet();
        var violations = forbiddenAssemblies.Where(references.Contains).ToArray();

        Assert.True(violations.Length == 0,
            $"{assembly.GetName().Name} references forbidden assemblies: {string.Join(", ", violations)}");
    }
}
