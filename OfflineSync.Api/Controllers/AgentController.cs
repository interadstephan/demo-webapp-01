using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfflineSync.Api.Data;
using OfflineSync.Api.Models;

namespace OfflineSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AgentController> _logger;

    public AgentController(AppDbContext context, ILogger<AgentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllAgents()
    {
        var agents = await _context.Agents
            .Where(a => a.IsActive)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Email,
                a.CreatedAt,
                a.UpdatedAt
            })
            .ToListAsync();

        return Ok(agents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetAgent(Guid id)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null || !agent.IsActive)
        {
            return NotFound();
        }

        return Ok(new
        {
            agent.Id,
            agent.Name,
            agent.Email,
            agent.CreatedAt,
            agent.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult> CreateAgent([FromBody] CreateAgentRequest request)
    {
        try
        {
            var existingAgent = await _context.Agents
                .FirstOrDefaultAsync(a => a.Email == request.Email);

            if (existingAgent != null)
            {
                return Conflict("Agent with this email already exists");
            }

            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, new
            {
                agent.Id,
                agent.Name,
                agent.Email,
                agent.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent");
            return StatusCode(500, $"Error creating agent: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAgent(Guid id, [FromBody] UpdateAgentRequest request)
    {
        try
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
            {
                return NotFound();
            }

            agent.Name = request.Name;
            agent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                agent.Id,
                agent.Name,
                agent.Email,
                agent.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent {AgentId}", id);
            return StatusCode(500, $"Error updating agent: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAgent(Guid id)
    {
        try
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
            {
                return NotFound();
            }

            agent.IsActive = false;
            agent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting agent {AgentId}", id);
            return StatusCode(500, $"Error deleting agent: {ex.Message}");
        }
    }
}

public class CreateAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateAgentRequest
{
    public string Name { get; set; } = string.Empty;
}
