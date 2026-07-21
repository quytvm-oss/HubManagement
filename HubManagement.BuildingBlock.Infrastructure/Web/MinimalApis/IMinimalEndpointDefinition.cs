using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;

public interface IMinimalEndpointDefinition
{
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder builder);
}