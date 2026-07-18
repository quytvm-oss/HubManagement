using HubManagement.Application.DTOs;
using HubManagement.BuildingBlock.Core.Common;

namespace HubManagement.Application.Services;

public interface IRoleService
{
    Task<PagedResponse<RoleDto>> GetRolesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken cancellationToken = default);
    
    Task<RoleDto?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<RoleDto> CreateOrUpdateRoleAsync(Guid roleId, string name, string description, CancellationToken cancellationToken = default);
    
    Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<RoleDto> GetWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<string> UpdatePermissionsAsync(Guid roleId, List<string> permissions, CancellationToken cancellationToken = default);
}