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
                        UpdatedAt = pushedRecord.UpdatedAt,
                        IsDeleted = pushedRecord.IsDeleted,
                        Version = pushedRecord.Version > 0 ? pushedRecord.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    _context.DataRecords.Add(newRecord);
                }
                else if (existingRecord.UpdatedAt < pushedRecord.UpdatedAt)
                {
                    // Update existing record if client version is newer
                    existingRecord.Title = pushedRecord.Title;
                    existingRecord.Description = pushedRecord.Description;
                    existingRecord.Data = pushedRecord.Data;
                    existingRecord.UpdatedAt = pushedRecord.UpdatedAt;
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
                        UpdatedAt = pushedFile.UpdatedAt,
                        IsDeleted = pushedFile.IsDeleted,
                        Version = pushedFile.Version > 0 ? pushedFile.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    _context.FileAttachments.Add(newFile);
                }
                else if (existingFile.UpdatedAt < pushedFile.UpdatedAt)
                {
                    existingFile.FileName = pushedFile.FileName;
                    existingFile.ContentType = pushedFile.ContentType;
                    existingFile.FileSize = pushedFile.FileSize;
                    existingFile.BlobPath = pushedFile.BlobPath;
                    existingFile.UpdatedAt = pushedFile.UpdatedAt;
                    existingFile.IsDeleted = pushedFile.IsDeleted;
                    existingFile.Version = pushedFile.Version > 0 ? pushedFile.Version : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
                    UpdatedAt = r.UpdatedAt,
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
                    UpdatedAt = f.UpdatedAt,
                    IsDeleted = f.IsDeleted,
                    Version = f.Version
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
