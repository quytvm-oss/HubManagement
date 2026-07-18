using HubManagement.BuildingBlock.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace HubManagement.Domain.Entities;

public class ApplicationUser :  IdentityUser<Guid>, IHasDomainEvents
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public Uri? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime RefreshTokenExpireTime { get; set; }

    public string? ObjectId { get; set; }

    public DateTime LastPasswordChangeDateTime { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;
    
    //navigation
    public virtual ICollection<UserDeviceToken> UserDeviceTokens { get; set; }
    
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void ClearDomainEvents() => _domainEvents.Clear();
    
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    
    public void Activate(string? activityBy = null)
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate(string? deactivatedBy = null, string? reason = null)
    {
        if (!IsActive) return;
        IsActive = false;
    }

}