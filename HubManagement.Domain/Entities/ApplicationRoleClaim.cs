using Microsoft.AspNetCore.Identity;

namespace HubManagement.Domain.Entities;

public class  ApplicationRoleClaim: IdentityRoleClaim<Guid>
{
    public string? Description { get; set; }
    
    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
}