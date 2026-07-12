using HubManagement.BuildingBlock.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace HubManagement.Domain.Entities;

public class ApplicationUser :  IdentityUser, IHasDomainEvents, IAuditableEntity, ISoftDeletable
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public Uri? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime RefreshTokenExpireTime { get; set; }

    public string? ObjectId { get; set; }

    public DateTime LastPasswordChangeDateTime { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;
    
    public DateTimeOffset CreatedOnUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedOnUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedOnUtc { get; set; }

    public string? DeletedBy { get; set; }
    
    //navigation
    public virtual ICollection<UserDeviceToken> UserDeviceTokens { get; set; }
    
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void ClearDomainEvents() => _domainEvents.Clear();
    
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

}