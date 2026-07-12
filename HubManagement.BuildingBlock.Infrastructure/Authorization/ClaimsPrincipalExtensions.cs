using System.Security.Claims;

namespace HubManagement.BuildingBlock.Infrastructure.Authorization;

public static class ClaimsPrincipalExtensions
{
    // Retrieves the user's ID
    public static string? GetUserId(this ClaimsPrincipal principal) =>
        principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    
    public static string? GetEmail(this ClaimsPrincipal principal) =>
        principal?.FindFirstValue(ClaimTypes.Email);
}