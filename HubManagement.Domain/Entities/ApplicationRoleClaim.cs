using Microsoft.AspNetCore.Identity;

namespace HubManagement.Domain.Entities;

public class  ApplicationRoleClaim: IdentityRoleClaim<string>
{
    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
}