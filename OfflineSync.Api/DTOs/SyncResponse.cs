namespace OfflineSync.Api.DTOs;

public class SyncResponse
{
    public long CurrentVersion { get; set; }
    public List<DataRecordDto> UpdatedRecords { get; set; } = new();
    public List<FileAttachmentDto> UpdatedFiles { get; set; } = new();
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
