namespace OfflineSync.Api.Models;

public class DataRecord
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; }
    
    // Navigation property
    public Agent Agent { get; set; } = null!;
    public ICollection<FileAttachment> FileAttachments { get; set; } = new List<FileAttachment>();
}
