namespace OffsideIQ.Core.Entities;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty; // e.g. "MCI", "ARS"
    public string? LogoUrl { get; set; }
    public string? Stadium { get; set; }
    public string? League { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }

    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
}
