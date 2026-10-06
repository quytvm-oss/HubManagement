namespace HubManagement.Domain.Entities;

public class UserDeviceToken
{
    public Guid Id { get; private set; }
    
    public long UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;
    
    public ApplicationUser ApplicationUser { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public static UserDeviceToken Create(long userId, string token, string platform)
    {

        return new UserDeviceToken
        {
            Id = Guid.CreateVersion7(),
            Token = token,
            UserId = userId,
            Platform = platform
        };
    }
}
