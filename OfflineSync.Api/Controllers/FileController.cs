using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfflineSync.Api.Data;
using OfflineSync.Api.Models;

namespace OfflineSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<FileController> _logger;
    private readonly string _uploadPath;

    public FileController(AppDbContext context, ILogger<FileController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _uploadPath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(_uploadPath);
    }

    [HttpPost("upload")]
    public async Task<ActionResult> UploadFile(IFormFile file, [FromForm] Guid agentId, [FromForm] Guid? dataRecordId)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            // Verify agent exists
            var agent = await _context.Agents.FindAsync(agentId);
            if (agent == null)
            {
                return NotFound("Agent not found");
            }

            var fileId = Guid.NewGuid();
            var fileName = $"{fileId}_{file.FileName}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileAttachment = new FileAttachment
            {
                Id = fileId,
                AgentId = agentId,
                DataRecordId = dataRecordId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                BlobPath = filePath,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                Version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            _context.FileAttachments.Add(fileAttachment);
            await _context.SaveChangesAsync();

            return Ok(new { id = fileId, fileName = file.FileName, size = file.Length });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for agent {AgentId}", agentId);
            return StatusCode(500, $"Error uploading file: {ex.Message}");
        }
    }

    [HttpGet("download/{fileId}")]
    public async Task<ActionResult> DownloadFile(Guid fileId)
    {
        try
        {
            var fileAttachment = await _context.FileAttachments.FindAsync(fileId);
            if (fileAttachment == null || fileAttachment.IsDeleted)
            {
                return NotFound("File not found");
            }

            if (!System.IO.File.Exists(fileAttachment.BlobPath))
            {
                return NotFound("Physical file not found");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fileAttachment.BlobPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, fileAttachment.ContentType, fileAttachment.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", fileId);
            return StatusCode(500, $"Error downloading file: {ex.Message}");
        }
    }

    [HttpGet("agent/{agentId}")]
    public async Task<ActionResult> GetAgentFiles(Guid agentId)
    {
        var files = await _context.FileAttachments
            .Where(f => f.AgentId == agentId && !f.IsDeleted)
            .Select(f => new
            {
                f.Id,
                f.FileName,
                f.ContentType,
                f.FileSize,
                f.CreatedAt,
                f.DataRecordId
            })
            .ToListAsync();

        return Ok(files);
    }
}
