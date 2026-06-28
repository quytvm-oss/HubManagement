using System.ComponentModel.DataAnnotations;

namespace HubManagement.BuildingBlock.Infrastructure.FileStorage.Local;

public class LocalStorageOptions
{
    [Required]
    public string? StorageRoot { get; set; } = string.Empty;
}