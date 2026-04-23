namespace OffsideIQ.Core.Entities;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Position { get; set; } // GK, DEF, MID, FWD
    public int? JerseyNumber { get; set; }
    public string? Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Team Team { get; set; } = null!;
    public ICollection<PlayerRating> Ratings { get; set; } = new List<PlayerRating>();
}

public class PlayerRating
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }
    public Guid PlayerId { get; set; }
    public decimal Rating { get; set; } // 1.0 - 10.0
    public string? Notes { get; set; }

    // Navigation
    public Match Match { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

public class MatchNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Match Match { get; set; } = null!;
    public User User { get; set; } = null!;
}
