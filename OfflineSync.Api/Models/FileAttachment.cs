namespace OfflineSync.Api.Models;

public class FileAttachment
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public Guid? DataRecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string BlobPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; }
    
    // Navigation properties
    public Agent Agent { get; set; } = null!;
    public DataRecord? DataRecord { get; set; }
}
