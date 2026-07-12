using HubManagement.Application.DTOs;

namespace HubManagement.Application.Services;

public interface IUserRoleService
{
    Task<string> AssignRoleAsync(string userId, List<UserRoleDto> userRoles, CancellationToken ct = default);
    
    Task<List<UserRoleDto>> GetUserRolesAsync(string userId, CancellationToken ct = default);
}