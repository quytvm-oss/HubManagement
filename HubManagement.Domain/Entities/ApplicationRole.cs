using Microsoft.AspNetCore.Identity;

namespace HubManagement.Domain.Entities;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    
    public ApplicationRole(string name, string? description = null)
        : base(name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Description = description;
        NormalizedName = name.ToUpperInvariant();
    }
}