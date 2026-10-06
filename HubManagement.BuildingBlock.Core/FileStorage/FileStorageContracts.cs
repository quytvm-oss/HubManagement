namespace HubManagement.BuildingBlock.Core.FileStorage;

public enum FileType { Image, Document, Video, Backup }

public sealed class FileDownloadResponse
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
    public long? ContentLength { get; init; }
}

public sealed class StreamUploadRequest
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required Stream Stream { get; init; }
}

public sealed class BufferedUploadRequest
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Data { get; init; }
}
