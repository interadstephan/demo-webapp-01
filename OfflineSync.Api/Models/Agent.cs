namespace OfflineSync.Api.Models;

public class Agent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<DataRecord> DataRecords { get; set; } = new List<DataRecord>();
    public ICollection<FileAttachment> FileAttachments { get; set; } = new List<FileAttachment>();
}
