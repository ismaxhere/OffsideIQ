using OffsideIQ.Core.Enums;

namespace OffsideIQ.Core.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string RefreshToken, UserDto User);

public record UserDto(Guid Id, string Email, string DisplayName, string Role);

// ── Teams ─────────────────────────────────────────────────────────────────────

public record CreateTeamRequest(
    string Name,
    string ShortCode,
    string? LogoUrl,
    string? Stadium,
    string? League,
    string? Country);

public record UpdateTeamRequest(
    string? Name,
    string? ShortCode,
    string? LogoUrl,
    string? Stadium,
    string? League,
    string? Country);

public record TeamDto(
    Guid Id,
    string Name,
    string ShortCode,
    string? LogoUrl,
    string? Stadium,
    string? League,
    string? Country);

public record TeamFormDto(
    Guid TeamId,
    string TeamName,
    List<MatchResultDto> Last5,
    int Wins,
    int Draws,
    int Losses,
    decimal WinRate,
    double AvgGoalsScored,
    double AvgGoalsConceded,
    string FormString); // e.g. "WWDLW"

// ── Matches ───────────────────────────────────────────────────────────────────

public record CreateMatchRequest(
    Guid HomeTeamId,
    Guid AwayTeamId,
    int HomeScore,
    int AwayScore,
    DateTime MatchDate,
    string? Competition,
    string? Venue,
    MatchStatus Status,
    CreateMatchStatsRequest? Stats);

public record UpdateMatchRequest(
    int? HomeScore,
    int? AwayScore,
    DateTime? MatchDate,
    string? Competition,
    string? Venue,
    MatchStatus? Status);

public record MatchDto(
    Guid Id,
    TeamDto HomeTeam,
    TeamDto AwayTeam,
    int HomeScore,
    int AwayScore,
    DateTime MatchDate,
    string? Competition,
    string? Venue,
    MatchStatus Status,
    MatchStatsDto? Stats,
    DateTime CreatedAt);

public record MatchResultDto(
    Guid MatchId,
    DateTime Date,
    string Opponent,
    int GoalsFor,
    int GoalsAgainst,
    MatchResult Result);

// ── Match Stats ───────────────────────────────────────────────────────────────

public record CreateMatchStatsRequest(
    decimal HomePossession,
    decimal AwayPossession,
    int HomeShotsTotal,
    int HomeShotsOnTarget,
    int AwayShotsTotal,
    int AwayShotsOnTarget,
    int HomePasses,
    int HomePassAccuracy,
    int AwayPasses,
    int AwayPassAccuracy,
    int HomeYellowCards,
    int HomeRedCards,
    int AwayYellowCards,
    int AwayRedCards,
    int HomeCorners,
    int AwayCorners,
    int HomeFouls,
    int AwayFouls,
    decimal? HomeXg,
    decimal? AwayXg);

public record MatchStatsDto(
    decimal HomePossession,
    decimal AwayPossession,
    int HomeShotsTotal,
    int HomeShotsOnTarget,
    int AwayShotsTotal,
    int AwayShotsOnTarget,
    int HomePasses,
    int HomePassAccuracy,
    int AwayPasses,
    int AwayPassAccuracy,
    int HomeYellowCards,
    int HomeRedCards,
    int AwayYellowCards,
    int AwayRedCards,
    int HomeCorners,
    int AwayCorners,
    int HomeFouls,
    int AwayFouls,
    decimal? HomeXg,
    decimal? AwayXg);

// ── Head-to-Head ──────────────────────────────────────────────────────────────

public record HeadToHeadDto(
    TeamDto TeamA,
    TeamDto TeamB,
    int TeamAWins,
    int TeamBWins,
    int Draws,
    int TotalMatches,
    int TeamAGoals,
    int TeamBGoals,
    List<MatchDto> RecentMatches);

// ── Insights ──────────────────────────────────────────────────────────────────

public record InsightDto(
    string Type,        // "form" | "scoring" | "defense" | "prediction" | "streak"
    string Level,       // "info" | "warning" | "positive" | "negative"
    string Title,
    string Message,
    Guid? RelatedTeamId);

public record DashboardDto(
    List<MatchDto> RecentMatches,
    List<InsightDto> Insights,
    List<TeamFormDto> TeamForms,
    DashboardStatsDto Stats);

public record DashboardStatsDto(
    int TotalMatches,
    int TotalTeams,
    double AvgGoalsPerMatch,
    int MatchesThisMonth);

// ── Players ───────────────────────────────────────────────────────────────────

public record CreatePlayerRequest(
    Guid TeamId,
    string Name,
    string? Position,
    int? JerseyNumber,
    string? Nationality,
    DateTime? DateOfBirth);

public record PlayerDto(
    Guid Id,
    Guid TeamId,
    string TeamName,
    string Name,
    string? Position,
    int? JerseyNumber,
    string? Nationality,
    double? AverageRating);

public record UpsertPlayerRatingRequest(Guid PlayerId, decimal Rating, string? Notes);

// ── Notes ─────────────────────────────────────────────────────────────────────

public record CreateNoteRequest(string Content, bool IsPublic);
public record NoteDto(Guid Id, Guid MatchId, string AuthorName, string Content, bool IsPublic, DateTime CreatedAt);

// ── Predictions ───────────────────────────────────────────────────────────────

public record PredictionDto(
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    string PredictedOutcome,   // "Home Win" | "Draw" | "Away Win"
    decimal HomeWinProbability,
    decimal DrawProbability,
    decimal AwayWinProbability,
    List<string> Factors);

// ── Pagination ────────────────────────────────────────────────────────────────

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}
