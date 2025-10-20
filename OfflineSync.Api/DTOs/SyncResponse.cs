namespace OfflineSync.Api.DTOs;

public class SyncResponse
{
    public long CurrentVersion { get; set; }
    public List<DataRecordDto> UpdatedRecords { get; set; } = new();
    public List<FileAttachmentDto> UpdatedFiles { get; set; } = new();
    public List<MasterDataDto> UpdatedMasterData { get; set; } = new();
    public List<ArticleDto> UpdatedArticles { get; set; } = new();
    public int TotalArticles { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasMoreArticles { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
