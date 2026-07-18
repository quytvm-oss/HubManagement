namespace HubManagement.Application.DTOs;

public class RoleDto
{
    public Guid Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public IReadOnlyCollection<string>? Permissions { get; set; }
}