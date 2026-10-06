using System.Security.Claims;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Authorization;
using Microsoft.AspNetCore.Http;

namespace HubManagement.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? _user;

    private ClaimsPrincipal? User => _user ?? httpContextAccessor.HttpContext?.User;
    
    public string? Name => User?.Identity?.Name;
    
    private Guid _userId = Guid.Empty;
    
    public Guid GetUserId()
    {
        return IsAuthenticated() ?
            Guid.Parse(User?.GetUserId() ?? Guid.Empty.ToString()) : _userId;
    }

    public string? GetUserEmail()
        => IsAuthenticated() ? User!.GetEmail() : string.Empty;

    public bool IsAuthenticated()
        => User?.Identity?.IsAuthenticated is true;

    public bool IsInRole(string role)
        => User?.IsInRole(role) is true;

    public IEnumerable<Claim>? GetUserClaims()
        => User?.Claims;

    public void SetCurrentUser(ClaimsPrincipal user)
    {
        if (_user != null)
        {
            throw new CustomException("Method reserved for in-scope initialization");
        }

        _user = user;
    }

    public void SetCurrentUserId(string userId)
    {
        if (_userId != Guid.Empty)
        {
            throw new CustomException("Method reserved for in-scope initialization");
        }

        if (!string.IsNullOrEmpty(userId))
        {
            _userId = Guid.Parse(userId);
        }
    }
}
