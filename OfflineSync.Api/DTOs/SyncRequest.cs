namespace OfflineSync.Api.DTOs;

public class SyncRequest
{
    public Guid AgentId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public long LastSyncVersion { get; set; }
    public List<DataRecordDto> PushedRecords { get; set; } = new();
    public List<FileAttachmentDto> PushedFiles { get; set; } = new();
}

public class DataRecordDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; }
}

public class FileAttachmentDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public Guid? DataRecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string BlobPath { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; }
}
