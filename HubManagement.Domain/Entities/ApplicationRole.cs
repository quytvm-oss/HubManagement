using HubManagement.BuildingBlock.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace HubManagement.Domain.Entities;

public sealed class ApplicationRole : IdentityRole, IAuditableEntity, ISoftDeletable
{
    public string? Description { get; set; }
    
    public DateTimeOffset CreatedOnUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedOnUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedOnUtc { get; set; }

    public string? DeletedBy { get; set; }
    
    public ApplicationRole(string name, string? description = null)
        : base(name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Description = description;
        NormalizedName = name.ToUpperInvariant();
    }
}