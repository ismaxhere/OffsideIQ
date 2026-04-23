using OffsideIQ.Core.DTOs;
using OffsideIQ.Core.Entities;
using OffsideIQ.Core.Enums;
using OffsideIQ.Core.Interfaces;

namespace OffsideIQ.Application.Services;

// ── Team Service ──────────────────────────────────────────────────────────────

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teams;
    private readonly IMatchRepository _matches;

    public TeamService(ITeamRepository teams, IMatchRepository matches)
    {
        _teams = teams;
        _matches = matches;
    }

    public async Task<TeamDto> CreateAsync(CreateTeamRequest req, Guid userId)
    {
        if (await _teams.ShortCodeExistsAsync(req.ShortCode))
            throw new InvalidOperationException($"Short code '{req.ShortCode}' is already taken.");

        var team = new Team
        {
            Name = req.Name.Trim(),
            ShortCode = req.ShortCode.ToUpper().Trim(),
            LogoUrl = req.LogoUrl,
            Stadium = req.Stadium,
            League = req.League,
            Country = req.Country,
            CreatedByUserId = userId,
        };

        await _teams.AddAsync(team);
        return MapToDto(team);
    }

    public async Task<TeamDto> UpdateAsync(Guid id, UpdateTeamRequest req, Guid userId)
    {
        var team = await _teams.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Team not found.");

        if (req.Name is not null) team.Name = req.Name.Trim();
        if (req.ShortCode is not null)
        {
            if (await _teams.ShortCodeExistsAsync(req.ShortCode, id))
                throw new InvalidOperationException($"Short code '{req.ShortCode}' is already taken.");
            team.ShortCode = req.ShortCode.ToUpper().Trim();
        }
        if (req.LogoUrl is not null) team.LogoUrl = req.LogoUrl;
        if (req.Stadium is not null) team.Stadium = req.Stadium;
        if (req.League is not null) team.League = req.League;
        if (req.Country is not null) team.Country = req.Country;

        await _teams.UpdateAsync(team);
        return MapToDto(team);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var team = await _teams.GetByIdAsync(id) ?? throw new KeyNotFoundException("Team not found.");
        await _teams.DeleteAsync(team);
    }

    public async Task<TeamDto> GetByIdAsync(Guid id)
    {
        var team = await _teams.GetByIdAsync(id) ?? throw new KeyNotFoundException("Team not found.");
        return MapToDto(team);
    }

    public async Task<IEnumerable<TeamDto>> GetAllForUserAsync(Guid userId)
    {
        var teams = await _teams.GetByUserAsync(userId);
        return teams.Select(MapToDto);
    }

    public async Task<TeamFormDto> GetFormAsync(Guid teamId)
    {
        var team = await _teams.GetByIdAsync(teamId) ?? throw new KeyNotFoundException("Team not found.");
        var matches = (await _matches.GetByTeamAsync(teamId, 5)).ToList();

        var results = matches.Select(m => BuildMatchResult(m, teamId)).ToList();
        int wins = results.Count(r => r.Result == MatchResult.Win);
        int draws = results.Count(r => r.Result == MatchResult.Draw);
        int losses = results.Count(r => r.Result == MatchResult.Loss);
        var formStr = string.Join("", results.Select(r => r.Result switch
        {
            MatchResult.Win => "W",
            MatchResult.Draw => "D",
            _ => "L"
        }));

        double avgFor = results.Any() ? results.Average(r => r.GoalsFor) : 0;
        double avgAgainst = results.Any() ? results.Average(r => r.GoalsAgainst) : 0;

        return new TeamFormDto(
            teamId, team.Name, results, wins, draws, losses,
            results.Any() ? (decimal)wins / results.Count * 100 : 0,
            Math.Round(avgFor, 2), Math.Round(avgAgainst, 2), formStr);
    }

    private static MatchResultDto BuildMatchResult(Match m, Guid teamId)
    {
        bool isHome = m.HomeTeamId == teamId;
        int goalsFor = isHome ? m.HomeScore : m.AwayScore;
        int goalsAgainst = isHome ? m.AwayScore : m.HomeScore;
        string opponent = isHome ? m.AwayTeam?.Name ?? "Unknown" : m.HomeTeam?.Name ?? "Unknown";

        var result = goalsFor > goalsAgainst ? MatchResult.Win
            : goalsFor < goalsAgainst ? MatchResult.Loss
            : MatchResult.Draw;

        return new MatchResultDto(m.Id, m.MatchDate, opponent, goalsFor, goalsAgainst, result);
    }

    internal static TeamDto MapToDto(Team t) =>
        new(t.Id, t.Name, t.ShortCode, t.LogoUrl, t.Stadium, t.League, t.Country);
}

// ── Match Service ─────────────────────────────────────────────────────────────

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matches;
    private readonly IMatchStatsRepository _stats;
    private readonly ITeamRepository _teams;

    public MatchService(IMatchRepository matches, IMatchStatsRepository stats, ITeamRepository teams)
    {
        _matches = matches;
        _stats = stats;
        _teams = teams;
    }

    public async Task<MatchDto> CreateAsync(CreateMatchRequest req, Guid userId)
    {
        if (!await _teams.ExistsAsync(req.HomeTeamId)) throw new KeyNotFoundException("Home team not found.");
        if (!await _teams.ExistsAsync(req.AwayTeamId)) throw new KeyNotFoundException("Away team not found.");
        if (req.HomeTeamId == req.AwayTeamId) throw new InvalidOperationException("Home and away teams must be different.");

        var match = new Match
        {
            HomeTeamId = req.HomeTeamId,
            AwayTeamId = req.AwayTeamId,
            HomeScore = req.HomeScore,
            AwayScore = req.AwayScore,
            MatchDate = DateTime.SpecifyKind(req.MatchDate, DateTimeKind.Utc),
            Competition = req.Competition,
            Venue = req.Venue,
            Status = req.Status,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        await _matches.AddAsync(match);

        if (req.Stats is not null)
            await UpsertStatsAsync(match.Id, req.Stats, userId);

        var full = await _matches.GetWithDetailsAsync(match.Id);
        return MapToDto(full!);
    }

    public async Task<MatchDto> UpdateAsync(Guid id, UpdateMatchRequest req, Guid userId)
    {
        var match = await _matches.GetByIdAsync(id) ?? throw new KeyNotFoundException("Match not found.");

        if (req.HomeScore.HasValue) match.HomeScore = req.HomeScore.Value;
        if (req.AwayScore.HasValue) match.AwayScore = req.AwayScore.Value;
        if (req.MatchDate.HasValue)
            match.MatchDate = DateTime.SpecifyKind(req.MatchDate.Value, DateTimeKind.Utc);
        if (req.Competition is not null) match.Competition = req.Competition;
        if (req.Venue is not null) match.Venue = req.Venue;
        if (req.Status.HasValue) match.Status = req.Status.Value;

        await _matches.UpdateAsync(match);
        var full = await _matches.GetWithDetailsAsync(id);
        return MapToDto(full!);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var match = await _matches.GetByIdAsync(id) ?? throw new KeyNotFoundException("Match not found.");
        await _matches.DeleteAsync(match);
    }

    public async Task<MatchDto> GetByIdAsync(Guid id)
    {
        var match = await _matches.GetWithDetailsAsync(id) ?? throw new KeyNotFoundException("Match not found.");
        return MapToDto(match);
    }

    public async Task<PagedResult<MatchDto>> GetPagedAsync(int page, int pageSize, string? competition = null)
    {
        var items = await _matches.GetPagedAsync(page, pageSize, competition);
        var total = await _matches.GetTotalCountAsync(competition);
        return new PagedResult<MatchDto>(items.Select(MapToDto).ToList(), total, page, pageSize);
    }

    public async Task<IEnumerable<MatchDto>> GetRecentAsync(int take = 10)
    {
        var matches = await _matches.GetRecentAsync(take);
        return matches.Select(MapToDto);
    }

    public async Task<HeadToHeadDto> GetHeadToHeadAsync(Guid teamAId, Guid teamBId)
    {
        var teamA = await _teams.GetByIdAsync(teamAId) ?? throw new KeyNotFoundException("Team A not found.");
        var teamB = await _teams.GetByIdAsync(teamBId) ?? throw new KeyNotFoundException("Team B not found.");

        var matches = (await _matches.GetHeadToHeadAsync(teamAId, teamBId)).ToList();

        int teamAWins = matches.Count(m => (m.HomeTeamId == teamAId && m.HomeScore > m.AwayScore) ||
                                           (m.AwayTeamId == teamAId && m.AwayScore > m.HomeScore));
        int teamBWins = matches.Count(m => (m.HomeTeamId == teamBId && m.HomeScore > m.AwayScore) ||
                                           (m.AwayTeamId == teamBId && m.AwayScore > m.HomeScore));
        int draws = matches.Count(m => m.HomeScore == m.AwayScore);

        int teamAGoals = matches.Sum(m => m.HomeTeamId == teamAId ? m.HomeScore : m.AwayScore);
        int teamBGoals = matches.Sum(m => m.HomeTeamId == teamBId ? m.HomeScore : m.AwayScore);

        return new HeadToHeadDto(
            TeamService.MapToDto(teamA), TeamService.MapToDto(teamB),
            teamAWins, teamBWins, draws, matches.Count,
            teamAGoals, teamBGoals,
            matches.Take(5).Select(MapToDto).ToList());
    }

    public async Task UpsertStatsAsync(Guid matchId, CreateMatchStatsRequest req, Guid userId)
    {
        var stats = new MatchStats
        {
            MatchId = matchId,
            HomePossession = req.HomePossession,
            AwayPossession = req.AwayPossession,
            HomeShotsTotal = req.HomeShotsTotal,
            HomeShotsOnTarget = req.HomeShotsOnTarget,
            AwayShotsTotal = req.AwayShotsTotal,
            AwayShotsOnTarget = req.AwayShotsOnTarget,
            HomePasses = req.HomePasses,
            HomePassAccuracy = req.HomePassAccuracy,
            AwayPasses = req.AwayPasses,
            AwayPassAccuracy = req.AwayPassAccuracy,
            HomeYellowCards = req.HomeYellowCards,
            HomeRedCards = req.HomeRedCards,
            AwayYellowCards = req.AwayYellowCards,
            AwayRedCards = req.AwayRedCards,
            HomeCorners = req.HomeCorners,
            AwayCorners = req.AwayCorners,
            HomeFouls = req.HomeFouls,
            AwayFouls = req.AwayFouls,
            HomeXg = req.HomeXg,
            AwayXg = req.AwayXg,
        };
        await _stats.UpsertAsync(stats);
    }

    internal static MatchDto MapToDto(Match m) => new(
        m.Id,
        TeamService.MapToDto(m.HomeTeam),
        TeamService.MapToDto(m.AwayTeam),
        m.HomeScore, m.AwayScore, m.MatchDate,
        m.Competition, m.Venue, m.Status,
        m.Stats is null ? null : MapStatsDto(m.Stats),
        m.CreatedAt);

    private static MatchStatsDto MapStatsDto(MatchStats s) => new(
        s.HomePossession, s.AwayPossession,
        s.HomeShotsTotal, s.HomeShotsOnTarget, s.AwayShotsTotal, s.AwayShotsOnTarget,
        s.HomePasses, s.HomePassAccuracy, s.AwayPasses, s.AwayPassAccuracy,
        s.HomeYellowCards, s.HomeRedCards, s.AwayYellowCards, s.AwayRedCards,
        s.HomeCorners, s.AwayCorners, s.HomeFouls, s.AwayFouls,
        s.HomeXg, s.AwayXg);
}
