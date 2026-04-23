using OffsideIQ.Core.DTOs;

namespace OffsideIQ.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    string GenerateJwtToken(UserDto user);
}

public interface ITeamService
{
    Task<TeamDto> CreateAsync(CreateTeamRequest request, Guid userId);
    Task<TeamDto> UpdateAsync(Guid id, UpdateTeamRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<TeamDto> GetByIdAsync(Guid id);
    Task<IEnumerable<TeamDto>> GetAllForUserAsync(Guid userId);
    Task<TeamFormDto> GetFormAsync(Guid teamId);
}

public interface IMatchService
{
    Task<MatchDto> CreateAsync(CreateMatchRequest request, Guid userId);
    Task<MatchDto> UpdateAsync(Guid id, UpdateMatchRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<MatchDto> GetByIdAsync(Guid id);
    Task<PagedResult<MatchDto>> GetPagedAsync(int page, int pageSize, string? competition = null);
    Task<IEnumerable<MatchDto>> GetRecentAsync(int take = 10);
    Task<HeadToHeadDto> GetHeadToHeadAsync(Guid teamAId, Guid teamBId);
    Task UpsertStatsAsync(Guid matchId, CreateMatchStatsRequest request, Guid userId);
}

public interface IInsightService
{
    Task<IEnumerable<InsightDto>> GenerateForMatchAsync(Guid matchId);
    Task<IEnumerable<InsightDto>> GenerateForTeamAsync(Guid teamId);
    Task<IEnumerable<InsightDto>> GenerateGlobalAsync();
    Task<PredictionDto> PredictMatchAsync(Guid homeTeamId, Guid awayTeamId);
}

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid userId);
}

public interface IPlayerService
{
    Task<PlayerDto> CreateAsync(CreatePlayerRequest request);
    Task<IEnumerable<PlayerDto>> GetByTeamAsync(Guid teamId);
    Task<PlayerDto> GetByIdAsync(Guid id);
    Task UpsertRatingAsync(Guid matchId, UpsertPlayerRatingRequest request);
}

public interface INoteService
{
    Task<NoteDto> CreateAsync(Guid matchId, CreateNoteRequest request, Guid userId);
    Task DeleteAsync(Guid noteId, Guid userId);
    Task<IEnumerable<NoteDto>> GetByMatchAsync(Guid matchId, Guid userId);
}
