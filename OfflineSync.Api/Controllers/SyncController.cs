using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfflineSync.Api.Data;
using OfflineSync.Api.DTOs;
using OfflineSync.Api.Models;

namespace OfflineSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SyncController> _logger;

    public SyncController(AppDbContext context, ILogger<SyncController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("sync")]
    public async Task<ActionResult<SyncResponse>> Sync([FromBody] SyncRequest request)
    {
        try
        {
            // Verify agent exists
            var agent = await _context.Agents.FindAsync(request.AgentId);
            if (agent == null)
            {
                return NotFound(new SyncResponse 
                { 
                    Success = false, 
                    Message = "Agent not found" 
                });
            }

            // Process pushed records from client
            foreach (var pushedRecord in request.PushedRecords)
            {
                var existingRecord = await _context.DataRecords
                    .FirstOrDefaultAsync(r => r.Id == pushedRecord.Id);

                if (existingRecord == null)
                {
                    // New record - use client version if provided, otherwise generate server version
                    var newRecord = new DataRecord
                    {
                        Id = pushedRecord.Id,
                        AgentId = pushedRecord.AgentId,
                        Title = pushedRecord.Title,
                        Description = pushedRecord.Description,
                        Data = pushedRecord.Data,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = pushedRecord.UpdatedAt.UtcDateTime,
                        IsDeleted = pushedRecord.IsDeleted,
                        Version = pushedRecord.Version > 0 ? pushedRecord.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    _context.DataRecords.Add(newRecord);
                }
                else if (existingRecord.UpdatedAt < pushedRecord.UpdatedAt.UtcDateTime)
                {
                    // Update existing record if client version is newer
                    existingRecord.Title = pushedRecord.Title;
                    existingRecord.Description = pushedRecord.Description;
                    existingRecord.Data = pushedRecord.Data;
                    existingRecord.UpdatedAt = pushedRecord.UpdatedAt.UtcDateTime;
                    existingRecord.IsDeleted = pushedRecord.IsDeleted;
                    existingRecord.Version = pushedRecord.Version > 0 ? pushedRecord.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }

            // Process pushed files from client
            foreach (var pushedFile in request.PushedFiles)
            {
                var existingFile = await _context.FileAttachments
                    .FirstOrDefaultAsync(f => f.Id == pushedFile.Id);

                if (existingFile == null)
                {
                    var newFile = new FileAttachment
                    {
                        Id = pushedFile.Id,
                        AgentId = pushedFile.AgentId,
                        DataRecordId = pushedFile.DataRecordId,
                        FileName = pushedFile.FileName,
                        ContentType = pushedFile.ContentType,
                        FileSize = pushedFile.FileSize,
                        BlobPath = pushedFile.BlobPath,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = pushedFile.UpdatedAt.UtcDateTime,
                        IsDeleted = pushedFile.IsDeleted,
                        Version = pushedFile.Version > 0 ? pushedFile.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    _context.FileAttachments.Add(newFile);
                }
                else if (existingFile.UpdatedAt < pushedFile.UpdatedAt.UtcDateTime)
                {
                    existingFile.FileName = pushedFile.FileName;
                    existingFile.ContentType = pushedFile.ContentType;
                    existingFile.FileSize = pushedFile.FileSize;
                    existingFile.BlobPath = pushedFile.BlobPath;
                    existingFile.UpdatedAt = pushedFile.UpdatedAt.UtcDateTime;
                    existingFile.IsDeleted = pushedFile.IsDeleted;
                    existingFile.Version = pushedFile.Version > 0 ? pushedFile.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }

            // Process pushed master data from client
            foreach (var pushedData in request.PushedMasterData)
            {
                var existingData = await _context.MasterData
                    .FirstOrDefaultAsync(m => m.Id == pushedData.Id);

                if (existingData == null)
                {
                    var newData = new MasterData
                    {
                        Id = pushedData.Id,
                        Key = pushedData.Key,
                        Value = pushedData.Value,
                        Category = pushedData.Category,
                        Description = pushedData.Description,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = pushedData.UpdatedAt.UtcDateTime,
                        IsDeleted = pushedData.IsDeleted,
                        Version = pushedData.Version > 0 ? pushedData.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    _context.MasterData.Add(newData);
                }
                else if (existingData.UpdatedAt < pushedData.UpdatedAt.UtcDateTime)
                {
                    existingData.Key = pushedData.Key;
                    existingData.Value = pushedData.Value;
                    existingData.Category = pushedData.Category;
                    existingData.Description = pushedData.Description;
                    existingData.UpdatedAt = pushedData.UpdatedAt.UtcDateTime;
                    existingData.IsDeleted = pushedData.IsDeleted;
                    existingData.Version = pushedData.Version > 0 ? pushedData.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }

            await _context.SaveChangesAsync();

            // Get updates for this agent since last sync
            var updatedRecords = await _context.DataRecords
                .Where(r => r.AgentId == request.AgentId && r.Version > request.LastSyncVersion)
                .Select(r => new DataRecordDto
                {
                    Id = r.Id,
                    AgentId = r.AgentId,
                    Title = r.Title,
                    Description = r.Description,
                    Data = r.Data,
                    UpdatedAt = new DateTimeOffset(DateTime.SpecifyKind(r.UpdatedAt, DateTimeKind.Utc)),
                    IsDeleted = r.IsDeleted,
                    Version = r.Version
                })
                .ToListAsync();

            var updatedFiles = await _context.FileAttachments
                .Where(f => f.AgentId == request.AgentId && f.Version > request.LastSyncVersion)
                .Select(f => new FileAttachmentDto
                {
                    Id = f.Id,
                    AgentId = f.AgentId,
                    DataRecordId = f.DataRecordId,
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    BlobPath = f.BlobPath,
                    UpdatedAt = new DateTimeOffset(DateTime.SpecifyKind(f.UpdatedAt, DateTimeKind.Utc)),
                    IsDeleted = f.IsDeleted,
                    Version = f.Version
                })
                .ToListAsync();

            // Get master data updates (global data for all agents)
            var updatedMasterData = await _context.MasterData
                .Where(m => m.Version > request.LastSyncVersion)
                .Select(m => new MasterDataDto
                {
                    Id = m.Id,
                    Key = m.Key,
                    Value = m.Value,
                    Category = m.Category,
                    Description = m.Description,
                    UpdatedAt = new DateTimeOffset(DateTime.SpecifyKind(m.UpdatedAt, DateTimeKind.Utc)),
                    IsDeleted = m.IsDeleted,
                    Version = m.Version
                })
                .ToListAsync();

            // Update sync metadata
            var syncMetadata = await _context.SyncMetadata
                .FirstOrDefaultAsync(s => s.AgentId == request.AgentId && s.DeviceId == request.DeviceId);

            var currentVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (syncMetadata == null)
            {
                syncMetadata = new SyncMetadata
                {
                    Id = Guid.NewGuid(),
                    AgentId = request.AgentId,
                    DeviceId = request.DeviceId,
                    LastSyncAt = DateTime.UtcNow,
                    LastSyncVersion = currentVersion,
                    SyncStatus = "Completed"
                };
                _context.SyncMetadata.Add(syncMetadata);
            }
            else
            {
                syncMetadata.LastSyncAt = DateTime.UtcNow;
                syncMetadata.LastSyncVersion = currentVersion;
                syncMetadata.SyncStatus = "Completed";
            }

            await _context.SaveChangesAsync();

            return Ok(new SyncResponse
            {
                CurrentVersion = currentVersion,
                UpdatedRecords = updatedRecords,
                UpdatedFiles = updatedFiles,
                UpdatedMasterData = updatedMasterData,
                Success = true,
                Message = "Sync completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sync for agent {AgentId}", request.AgentId);
            return StatusCode(500, new SyncResponse
            {
                Success = false,
                Message = $"Sync failed: {ex.Message}"
            });
        }
    }

    [HttpGet("agent/{agentId}/status")]
    public async Task<ActionResult> GetSyncStatus(Guid agentId)
    {
        var metadata = await _context.SyncMetadata
            .Where(s => s.AgentId == agentId)
            .OrderByDescending(s => s.LastSyncAt)
            .ToListAsync();

        return Ok(metadata);
    }
}
