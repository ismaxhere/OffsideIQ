using OffsideIQ.Core.Entities;

namespace OffsideIQ.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(Guid id);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}

public interface ITeamRepository : IRepository<Team>
{
    Task<IEnumerable<Team>> GetByUserAsync(Guid userId);
    Task<Team?> GetWithPlayersAsync(Guid id);
    Task<bool> ShortCodeExistsAsync(string shortCode, Guid? excludeId = null);
}

public interface IMatchRepository : IRepository<Match>
{
    Task<Match?> GetWithDetailsAsync(Guid id);
    Task<IEnumerable<Match>> GetByTeamAsync(Guid teamId, int take = 10);
    Task<IEnumerable<Match>> GetRecentAsync(int take = 10);
    Task<IEnumerable<Match>> GetHeadToHeadAsync(Guid teamAId, Guid teamBId, int take = 10);
    Task<IEnumerable<Match>> GetPagedAsync(int page, int pageSize, string? competition = null);
    Task<int> GetTotalCountAsync(string? competition = null);
    Task<double> GetAverageGoalsAsync();
    Task<int> GetCountThisMonthAsync();
}

public interface IPlayerRepository : IRepository<Player>
{
    Task<IEnumerable<Player>> GetByTeamAsync(Guid teamId);
    Task<Player?> GetWithRatingsAsync(Guid id);
}

public interface IMatchStatsRepository : IRepository<MatchStats>
{
    Task<MatchStats?> GetByMatchAsync(Guid matchId);
    Task UpsertAsync(MatchStats stats);
}

public interface IMatchNoteRepository : IRepository<MatchNote>
{
    Task<IEnumerable<MatchNote>> GetByMatchAsync(Guid matchId, Guid? userId = null);
}
