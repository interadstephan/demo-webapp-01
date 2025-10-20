namespace OfflineSync.Api.DTOs;

public class SyncRequest
{
    public Guid AgentId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public long LastSyncVersion { get; set; }
    public int PageSize { get; set; } = 10; // For pagination of large data
    public int PageNumber { get; set; } = 1;
    public List<DataRecordDto> PushedRecords { get; set; } = new();
    public List<FileAttachmentDto> PushedFiles { get; set; } = new();
    public List<MasterDataDto> PushedMasterData { get; set; } = new();
    public List<ArticleDto> PushedArticles { get; set; } = new();
}

public class DataRecordDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
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
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; }
}

public class MasterDataDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; }
}

public class ArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ImageData { get; set; } = string.Empty;
    public string ImageContentType { get; set; } = string.Empty;
    public DateTimeOffset PublishedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; }
}
