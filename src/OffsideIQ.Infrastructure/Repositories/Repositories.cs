using Microsoft.EntityFrameworkCore;
using OffsideIQ.Core.Entities;
using OffsideIQ.Core.Interfaces;
using OffsideIQ.Infrastructure.Data;

namespace OffsideIQ.Infrastructure.Repositories;

// ── Generic Base ──────────────────────────────────────────────────────────────

public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    public RepositoryBase(AppDbContext db) => _db = db;

    public virtual async Task<T?> GetByIdAsync(Guid id) => await _db.Set<T>().FindAsync(id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _db.Set<T>().ToListAsync();
    public virtual async Task<T> AddAsync(T entity) { _db.Set<T>().Add(entity); await _db.SaveChangesAsync(); return entity; }
    public virtual async Task UpdateAsync(T entity) { _db.Set<T>().Update(entity); await _db.SaveChangesAsync(); }
    public virtual async Task DeleteAsync(T entity) { _db.Set<T>().Remove(entity); await _db.SaveChangesAsync(); }
    public virtual async Task<bool> ExistsAsync(Guid id) => await _db.Set<T>().FindAsync(id) is not null;
}

// ── User ──────────────────────────────────────────────────────────────────────

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }
    public Task<User?> GetByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    public Task<bool> EmailExistsAsync(string email) =>
        _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
}

// ── Team ──────────────────────────────────────────────────────────────────────

public class TeamRepository : RepositoryBase<Team>, ITeamRepository
{
    public TeamRepository(AppDbContext db) : base(db) { }

    public Task<IEnumerable<Team>> GetByUserAsync(Guid userId) =>
        Task.FromResult<IEnumerable<Team>>(_db.Teams.Where(t => t.CreatedByUserId == userId).AsEnumerable());

    public Task<Team?> GetWithPlayersAsync(Guid id) =>
        _db.Teams.Include(t => t.Players).FirstOrDefaultAsync(t => t.Id == id);

    public Task<bool> ShortCodeExistsAsync(string shortCode, Guid? excludeId = null) =>
        _db.Teams.AnyAsync(t => t.ShortCode == shortCode && (excludeId == null || t.Id != excludeId));
}

// ── Match ─────────────────────────────────────────────────────────────────────

public class MatchRepository : RepositoryBase<Match>, IMatchRepository
{
    public MatchRepository(AppDbContext db) : base(db) { }

    public Task<Match?> GetWithDetailsAsync(Guid id) =>
        _db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Stats)
            .Include(m => m.Notes).ThenInclude(n => n.User)
            .Include(m => m.PlayerRatings).ThenInclude(r => r.Player)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<Match>> GetByTeamAsync(Guid teamId, int take = 10) =>
        await _db.Matches
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.Stats)
            .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
            .OrderByDescending(m => m.MatchDate)
            .Take(take)
            .ToListAsync();

    public async Task<IEnumerable<Match>> GetRecentAsync(int take = 10) =>
        await _db.Matches
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.Stats)
            .OrderByDescending(m => m.MatchDate)
            .Take(take)
            .ToListAsync();

    public async Task<IEnumerable<Match>> GetHeadToHeadAsync(Guid teamAId, Guid teamBId, int take = 10) =>
        await _db.Matches
            .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.Stats)
            .Where(m => (m.HomeTeamId == teamAId && m.AwayTeamId == teamBId) ||
                        (m.HomeTeamId == teamBId && m.AwayTeamId == teamAId))
            .OrderByDescending(m => m.MatchDate)
            .Take(take)
            .ToListAsync();

    public async Task<IEnumerable<Match>> GetPagedAsync(int page, int pageSize, string? competition = null)
    {
        var q = _db.Matches.Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.Stats).AsQueryable();
        if (!string.IsNullOrEmpty(competition)) q = q.Where(m => m.Competition == competition);
        return await q.OrderByDescending(m => m.MatchDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? competition = null)
    {
        var q = _db.Matches.AsQueryable();
        if (!string.IsNullOrEmpty(competition)) q = q.Where(m => m.Competition == competition);
        return await q.CountAsync();
    }

    public async Task<double> GetAverageGoalsAsync()
    {
        if (!await _db.Matches.AnyAsync()) return 0;
        return await _db.Matches.AverageAsync(m => m.HomeScore + m.AwayScore);
    }

    public Task<int> GetCountThisMonthAsync()
    {
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return _db.Matches.CountAsync(m => m.MatchDate >= start);
    }
}

// ── Player ────────────────────────────────────────────────────────────────────

public class PlayerRepository : RepositoryBase<Player>, IPlayerRepository
{
    public PlayerRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Player>> GetByTeamAsync(Guid teamId) =>
        await _db.Players.Where(p => p.TeamId == teamId).Include(p => p.Ratings).ToListAsync();

    public Task<Player?> GetWithRatingsAsync(Guid id) =>
        _db.Players.Include(p => p.Ratings).ThenInclude(r => r.Match).FirstOrDefaultAsync(p => p.Id == id);
}

// ── MatchStats ────────────────────────────────────────────────────────────────

public class MatchStatsRepository : RepositoryBase<MatchStats>, IMatchStatsRepository
{
    public MatchStatsRepository(AppDbContext db) : base(db) { }

    public Task<MatchStats?> GetByMatchAsync(Guid matchId) =>
        _db.MatchStats.FirstOrDefaultAsync(s => s.MatchId == matchId);

    public async Task UpsertAsync(MatchStats stats)
    {
        var existing = await GetByMatchAsync(stats.MatchId);
        if (existing is null) { _db.MatchStats.Add(stats); }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(stats);
            existing.Id = existing.Id; // keep original Id
        }
        await _db.SaveChangesAsync();
    }
}

// ── MatchNote ─────────────────────────────────────────────────────────────────

public class MatchNoteRepository : RepositoryBase<MatchNote>, IMatchNoteRepository
{
    public MatchNoteRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<MatchNote>> GetByMatchAsync(Guid matchId, Guid? userId = null)
    {
        var q = _db.MatchNotes.Include(n => n.User).Where(n => n.MatchId == matchId);
        if (userId.HasValue) q = q.Where(n => n.IsPublic || n.UserId == userId.Value);
        else q = q.Where(n => n.IsPublic);
        return await q.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }
}
