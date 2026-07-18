namespace HubManagement.Application.Services;

public interface IUserService
{
    Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default);
    
    Task<bool> ExistsWithEmailAsync(string email, Guid? exceptId = null, CancellationToken ct = default);
    
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber,  Guid? exceptId = null, CancellationToken ct = default);
    
    // permisions
    Task<List<string>?> GetPermissionsAsync(string userId, CancellationToken ct = default);
    
    Task<bool> HasPermissionAsync(string userId, string permissionName, CancellationToken ct = default);
    
    Task InvalidatePermissionCacheAsync(string userId,CancellationToken ct = default);
}