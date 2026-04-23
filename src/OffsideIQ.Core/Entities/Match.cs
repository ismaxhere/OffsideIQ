using OffsideIQ.Core.Enums;

namespace OffsideIQ.Core.Entities;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public DateTime MatchDate { get; set; }
    public string? Competition { get; set; }
    public string? Venue { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }

    // Navigation
    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public MatchStats? Stats { get; set; }
    public ICollection<MatchNote> Notes { get; set; } = new List<MatchNote>();
    public ICollection<PlayerRating> PlayerRatings { get; set; } = new List<PlayerRating>();
}
