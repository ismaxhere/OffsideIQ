using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OffsideIQ.Core.DTOs;
using OffsideIQ.Core.Interfaces;

namespace OffsideIQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated."));
}

// ── Auth ──────────────────────────────────────────────────────────────────────

[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req);
        return CreatedAtAction(nameof(Register), result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
        => Ok(await _auth.LoginAsync(req));
}

// ── Teams ─────────────────────────────────────────────────────────────────────

[Route("api/teams")]
[Authorize]
public class TeamsController : BaseController
{
    private readonly ITeamService _teams;
    public TeamsController(ITeamService teams) => _teams = teams;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetAll()
        => Ok(await _teams.GetAllForUserAsync(CurrentUserId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamDto>> GetById(Guid id)
        => Ok(await _teams.GetByIdAsync(id));

    [HttpGet("{id:guid}/form")]
    public async Task<ActionResult<TeamFormDto>> GetForm(Guid id)
        => Ok(await _teams.GetFormAsync(id));

    [HttpPost]
    public async Task<ActionResult<TeamDto>> Create([FromBody] CreateTeamRequest req)
    {
        var team = await _teams.CreateAsync(req, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, team);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TeamDto>> Update(Guid id, [FromBody] UpdateTeamRequest req)
        => Ok(await _teams.UpdateAsync(id, req, CurrentUserId));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _teams.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }
}

// ── Matches ───────────────────────────────────────────────────────────────────

[Route("api/matches")]
[Authorize]
public class MatchesController : BaseController
{
    private readonly IMatchService _matches;
    private readonly IInsightService _insights;
    private readonly INoteService _notes;

    public MatchesController(IMatchService matches, IInsightService insights, INoteService notes)
    {
        _matches = matches;
        _insights = insights;
        _notes = notes;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MatchDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? competition = null)
        => Ok(await _matches.GetPagedAsync(page, pageSize, competition));

    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetRecent([FromQuery] int take = 10)
        => Ok(await _matches.GetRecentAsync(take));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MatchDto>> GetById(Guid id)
        => Ok(await _matches.GetByIdAsync(id));

    [HttpGet("{id:guid}/insights")]
    public async Task<ActionResult<IEnumerable<InsightDto>>> GetInsights(Guid id)
        => Ok(await _insights.GenerateForMatchAsync(id));

    [HttpPost]
    public async Task<ActionResult<MatchDto>> Create([FromBody] CreateMatchRequest req)
    {
        var match = await _matches.CreateAsync(req, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = match.Id }, match);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MatchDto>> Update(Guid id, [FromBody] UpdateMatchRequest req)
        => Ok(await _matches.UpdateAsync(id, req, CurrentUserId));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _matches.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpPut("{id:guid}/stats")]
    public async Task<IActionResult> UpsertStats(Guid id, [FromBody] CreateMatchStatsRequest req)
    {
        await _matches.UpsertStatsAsync(id, req, CurrentUserId);
        return NoContent();
    }

    [HttpGet("{id:guid}/notes")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotes(Guid id)
        => Ok(await _notes.GetByMatchAsync(id, CurrentUserId));

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<NoteDto>> CreateNote(Guid id, [FromBody] CreateNoteRequest req)
        => Ok(await _notes.CreateAsync(id, req, CurrentUserId));

    [HttpDelete("{matchId:guid}/notes/{noteId:guid}")]
    public async Task<IActionResult> DeleteNote(Guid matchId, Guid noteId)
    {
        await _notes.DeleteAsync(noteId, CurrentUserId);
        return NoContent();
    }
}

// ── Head-to-Head ──────────────────────────────────────────────────────────────

[Route("api/h2h")]
[Authorize]
public class HeadToHeadController : BaseController
{
    private readonly IMatchService _matches;
    public HeadToHeadController(IMatchService matches) => _matches = matches;

    [HttpGet("{teamAId:guid}/{teamBId:guid}")]
    public async Task<ActionResult<HeadToHeadDto>> GetH2H(Guid teamAId, Guid teamBId)
        => Ok(await _matches.GetHeadToHeadAsync(teamAId, teamBId));
}

// ── Insights ──────────────────────────────────────────────────────────────────

[Route("api/insights")]
[Authorize]
public class InsightsController : BaseController
{
    private readonly IInsightService _insights;
    public InsightsController(IInsightService insights) => _insights = insights;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InsightDto>>> GetGlobal()
        => Ok(await _insights.GenerateGlobalAsync());

    [HttpGet("teams/{teamId:guid}")]
    public async Task<ActionResult<IEnumerable<InsightDto>>> GetForTeam(Guid teamId)
        => Ok(await _insights.GenerateForTeamAsync(teamId));

    [HttpGet("predict")]
    public async Task<ActionResult<PredictionDto>> Predict(
        [FromQuery] Guid homeTeamId,
        [FromQuery] Guid awayTeamId)
        => Ok(await _insights.PredictMatchAsync(homeTeamId, awayTeamId));
}

// ── Dashboard ─────────────────────────────────────────────────────────────────

[Route("api/dashboard")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboard;
    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
        => Ok(await _dashboard.GetDashboardAsync(CurrentUserId));
}

// ── Players ───────────────────────────────────────────────────────────────────

[Route("api/players")]
[Authorize]
public class PlayersController : BaseController
{
    private readonly IPlayerService _players;
    public PlayersController(IPlayerService players) => _players = players;

    [HttpGet("team/{teamId:guid}")]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetByTeam(Guid teamId)
        => Ok(await _players.GetByTeamAsync(teamId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlayerDto>> GetById(Guid id)
        => Ok(await _players.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<PlayerDto>> Create([FromBody] CreatePlayerRequest req)
    {
        var player = await _players.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = player.Id }, player);
    }

    [HttpPost("matches/{matchId:guid}/ratings")]
    public async Task<IActionResult> UpsertRating(Guid matchId, [FromBody] UpsertPlayerRatingRequest req)
    {
        await _players.UpsertRatingAsync(matchId, req);
        return NoContent();
    }
}
