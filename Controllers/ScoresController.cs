using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoreTracker.Api.Data;
using ScoreTracker.Api.Models.DTOs;
using ScoreTracker.Api.Models.Entities;
using System.Security.Claims;

namespace ScoreTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ScoresController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ScoresController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/scores - Gets scores for the currently logged-in user
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScoreDto>>> GetMyScores()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var scores = await _context.Scores
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.DateRecorded)
            .Select(s => new ScoreDto(s.Id, s.Value, s.DateRecorded))
            .ToListAsync();

        return Ok(scores);
    }
    
    // POST: api/scores - Adds a new score for the currently logged-in user
    [HttpPost]
    public async Task<ActionResult<ScoreDto>> AddScore(CreateScoreDto createScoreDto)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();
        
        var score = new Score
        {
            Value = createScoreDto.Value,
            DateRecorded = DateTime.UtcNow,
            UserId = userId
        };

        _context.Scores.Add(score);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyScores), new { id = score.Id }, new ScoreDto(score.Id, score.Value, score.DateRecorded));
    }

    // DELETE: api/scores/5 - Deletes a score
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScore(int id)
    {
        var userId = _userManager.GetUserId(User);
        var score = await _context.Scores.FindAsync(id);

        if (score == null)
        {
            return NotFound();
        }

        // Security Check: Ensure the user owns this score OR is an Admin
        var isAdmin = User.IsInRole("Admin");
        if (score.UserId != userId && !isAdmin)
        {
            return Forbid(); // User is not authorized to delete this specific resource
        }

        _context.Scores.Remove(score);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    // --- ADMIN ENDPOINTS ---

    // GET: api/scores/user/{userId} - Gets all scores for a specific user (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ScoreDto>>> GetScoresForUser(string userId)
    {
        var scores = await _context.Scores
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.DateRecorded)
            .Select(s => new ScoreDto(s.Id, s.Value, s.DateRecorded))
            .ToListAsync();

        return Ok(scores);
    }
}
