namespace OffsideIQ.Core.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // "User" | "Admin"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<MatchNote> Notes { get; set; } = new List<MatchNote>();
}
