using HubManagement.Application.Features.Tokens.RefreshToken;
using HubManagement.Application.Features.Tokens.TokenGeneration;
using HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HubManagement.Application.Features.Tokens;

public class TokenEndpoints : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/identity")
            .WithTags("Identity");

        group.MapGenerateToken();
        group.MapRefreshTokenEndpoint();

        return builder;
    }
}