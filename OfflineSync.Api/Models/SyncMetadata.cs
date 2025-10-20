namespace OfflineSync.Api.Models;

public class SyncMetadata
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTime LastSyncAt { get; set; }
    public long LastSyncVersion { get; set; }
    public string SyncStatus { get; set; } = string.Empty;
}
