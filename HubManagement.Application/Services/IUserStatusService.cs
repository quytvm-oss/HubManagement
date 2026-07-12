namespace HubManagement.Application.Services;

public interface IUserStatusService
{
    Task ToggleStatusAsync(bool activateUser, string userId, CancellationToken ct = default);
    
    Task DeleteAsync(string userId, CancellationToken ct = default);
}