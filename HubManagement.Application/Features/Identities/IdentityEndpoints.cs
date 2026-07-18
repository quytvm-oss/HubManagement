using HubManagement.Application.Features.Identities.Tokens.RefreshToken;
using HubManagement.Application.Features.Identities.Tokens.TokenGeneration;
using HubManagement.Application.Features.Identities.Users.RegisterUser;
using HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HubManagement.Application.Features.Identities;

public class IdentityEndpoints : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/identity")
            .WithTags("Identity");

        group.MapGenerateToken();
        group.MapRefreshTokenEndpoint();
        group.MapRegisterUserEndpoint();

        return builder;
    }
}