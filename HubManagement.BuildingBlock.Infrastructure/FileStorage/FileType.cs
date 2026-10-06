using HubManagement.BuildingBlock.Core.FileStorage;

namespace HubManagement.BuildingBlock.Infrastructure.FileStorage;

internal sealed record FileTypeRules(IReadOnlyList<string> AllowedExtensions, int MaxSizeInMb);

internal static class FileTypeMetadata
{
    private static readonly Dictionary<FileType, FileTypeRules> Rules = new()
    {
        [FileType.Image] = new([".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"], 10),
        [FileType.Document] = new([".pdf", ".docx", ".xlsx", ".pptx", ".txt"], 50),
        [FileType.Video] = new([".mp4", ".mov", ".avi", ".mkv", ".webm"], 500),
        [FileType.Backup] = new([".zip", ".tar", ".gz", ".7z"], 2048)
    };

    public static FileTypeRules GetRules(FileType fileType)
        => Rules.TryGetValue(fileType, out var rules)
            ? rules
            : throw new ArgumentOutOfRangeException(nameof(fileType), $"No rules defined for {fileType}.");
}
