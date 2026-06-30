using System.ComponentModel.DataAnnotations;
using HubManagement.BuildingBlock.Infrastructure.FileStorage.Local;

namespace HubManagement.BuildingBlock.Infrastructure.FileStorage;

public class StorageOptions
{
    [Required]
    public string? Provider { get; set; } = "Local";
    
    public LocalStorageOptions? Local { get; set; }
}