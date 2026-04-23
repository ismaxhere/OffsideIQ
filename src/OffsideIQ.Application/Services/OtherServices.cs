using OffsideIQ.Core.DTOs;
using OffsideIQ.Core.Entities;
using OffsideIQ.Core.Interfaces;

namespace OffsideIQ.Application.Services;

// ── Dashboard Service ─────────────────────────────────────────────────────────

public class DashboardService : IDashboardService
{
    private readonly IMatchRepository _matches;
    private readonly ITeamRepository _teams;
    private readonly ITeamService _teamService;
    private readonly IInsightService _insights;

    public DashboardService(
        IMatchRepository matches, ITeamRepository teams,
        ITeamService teamService, IInsightService insights)
    {
        _matches = matches;
        _teams = teams;
        _teamService = teamService;
        _insights = insights;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid userId)
    {
        var recentMatches = (await _matches.GetRecentAsync(10))
            .Select(MatchService.MapToDto).ToList();

        var globalInsights = (await _insights.GenerateGlobalAsync()).ToList();

        var teams = (await _teams.GetByUserAsync(userId)).Take(5).ToList();
        var forms = new List<TeamFormDto>();
        foreach (var t in teams)
            forms.Add(await _teamService.GetFormAsync(t.Id));

        var stats = new DashboardStatsDto(
            await _matches.GetTotalCountAsync(),
            (await _teams.GetAllAsync()).Count(),
            Math.Round(await _matches.GetAverageGoalsAsync(), 2),
            await _matches.GetCountThisMonthAsync());

        return new DashboardDto(recentMatches, globalInsights, forms, stats);
    }
}

// ── Player Service ────────────────────────────────────────────────────────────

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _players;
    private readonly ITeamRepository _teams;
    private readonly IMatchRepository _matches;

    public PlayerService(IPlayerRepository players, ITeamRepository teams, IMatchRepository matches)
    {
        _players = players;
        _teams = teams;
        _matches = matches;
    }

    public async Task<PlayerDto> CreateAsync(CreatePlayerRequest req)
    {
        if (!await _teams.ExistsAsync(req.TeamId)) throw new KeyNotFoundException("Team not found.");

        var player = new Player
        {
            TeamId = req.TeamId,
            Name = req.Name.Trim(),
            Position = req.Position,
            JerseyNumber = req.JerseyNumber,
            Nationality = req.Nationality,
            DateOfBirth = req.DateOfBirth,
        };

        await _players.AddAsync(player);
        var team = await _teams.GetByIdAsync(req.TeamId);
        return MapToDto(player, team!.Name);
    }

    public async Task<IEnumerable<PlayerDto>> GetByTeamAsync(Guid teamId)
    {
        var team = await _teams.GetByIdAsync(teamId) ?? throw new KeyNotFoundException("Team not found.");
        var players = await _players.GetByTeamAsync(teamId);
        return players.Select(p => MapToDto(p, team.Name, p.Ratings.Any()
            ? (double?)p.Ratings.Average(r => (double)r.Rating) : null));
    }

    public async Task<PlayerDto> GetByIdAsync(Guid id)
    {
        var player = await _players.GetWithRatingsAsync(id) ?? throw new KeyNotFoundException("Player not found.");
        var team = await _teams.GetByIdAsync(player.TeamId);
        double? avg = player.Ratings.Any() ? (double?)player.Ratings.Average(r => (double)r.Rating) : null;
        return MapToDto(player, team?.Name ?? "Unknown", avg);
    }

    public async Task UpsertRatingAsync(Guid matchId, UpsertPlayerRatingRequest req)
    {
        var player = await _players.GetWithRatingsAsync(req.PlayerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var existing = player.Ratings.FirstOrDefault(r => r.MatchId == matchId);
        if (existing is not null)
        {
            existing.Rating = req.Rating;
            existing.Notes = req.Notes;
            await _players.UpdateAsync(player);
        }
        else
        {
            player.Ratings.Add(new PlayerRating
            {
                MatchId = matchId,
                PlayerId = req.PlayerId,
                Rating = req.Rating,
                Notes = req.Notes,
            });
            await _players.UpdateAsync(player);
        }
    }

    private static PlayerDto MapToDto(Player p, string teamName, double? avgRating = null) =>
        new(p.Id, p.TeamId, teamName, p.Name, p.Position, p.JerseyNumber, p.Nationality, avgRating);
}

// ── Note Service ──────────────────────────────────────────────────────────────

public class NoteService : INoteService
{
    private readonly IMatchNoteRepository _notes;
    private readonly IMatchRepository _matches;

    public NoteService(IMatchNoteRepository notes, IMatchRepository matches)
    {
        _notes = notes;
        _matches = matches;
    }

    public async Task<NoteDto> CreateAsync(Guid matchId, CreateNoteRequest req, Guid userId)
    {
        if (!await _matches.ExistsAsync(matchId)) throw new KeyNotFoundException("Match not found.");

        var note = new MatchNote
        {
            MatchId = matchId,
            UserId = userId,
            Content = req.Content.Trim(),
            IsPublic = req.IsPublic,
        };

        await _notes.AddAsync(note);
        return new NoteDto(note.Id, matchId, "You", note.Content, note.IsPublic, note.CreatedAt);
    }

    public async Task DeleteAsync(Guid noteId, Guid userId)
    {
        var note = await _notes.GetByIdAsync(noteId) ?? throw new KeyNotFoundException("Note not found.");
        if (note.UserId != userId) throw new UnauthorizedAccessException("You cannot delete this note.");
        await _notes.DeleteAsync(note);
    }

    public async Task<IEnumerable<NoteDto>> GetByMatchAsync(Guid matchId, Guid userId)
    {
        var notes = await _notes.GetByMatchAsync(matchId, userId);
        return notes.Select(n => new NoteDto(
            n.Id, n.MatchId, n.User?.DisplayName ?? "Unknown",
            n.Content, n.IsPublic, n.CreatedAt));
    }
}
