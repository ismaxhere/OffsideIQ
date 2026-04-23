using Microsoft.EntityFrameworkCore;
using OffsideIQ.Core.Entities;

namespace OffsideIQ.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchStats> MatchStats => Set<MatchStats>();
    public DbSet<MatchNote> MatchNotes => Set<MatchNote>();
    public DbSet<PlayerRating> PlayerRatings => Set<PlayerRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("User");
        });

        // ── Team ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Team>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ShortCode).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.ShortCode).HasMaxLength(5).IsRequired();
            e.Property(x => x.League).HasMaxLength(100);
            e.Property(x => x.Country).HasMaxLength(100);
            e.HasOne(x => x.CreatedByUser)
             .WithMany(x => x.Teams)
             .HasForeignKey(x => x.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Player ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Player>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Position).HasMaxLength(10);
            e.HasOne(x => x.Team)
             .WithMany(x => x.Players)
             .HasForeignKey(x => x.TeamId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Match ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Match>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Competition).HasMaxLength(100);
            e.Property(x => x.Venue).HasMaxLength(150);
            e.HasOne(x => x.HomeTeam)
             .WithMany(x => x.HomeMatches)
             .HasForeignKey(x => x.HomeTeamId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AwayTeam)
             .WithMany(x => x.AwayMatches)
             .HasForeignKey(x => x.AwayTeamId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CreatedByUser)
             .WithMany(x => x.Matches)
             .HasForeignKey(x => x.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── MatchStats ────────────────────────────────────────────────────────
        modelBuilder.Entity<MatchStats>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.HomePossession).HasPrecision(5, 2);
            e.Property(x => x.AwayPossession).HasPrecision(5, 2);
            e.Property(x => x.HomeXg).HasPrecision(4, 2);
            e.Property(x => x.AwayXg).HasPrecision(4, 2);
            e.HasOne(x => x.Match)
             .WithOne(x => x.Stats)
             .HasForeignKey<MatchStats>(x => x.MatchId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── MatchNote ─────────────────────────────────────────────────────────
        modelBuilder.Entity<MatchNote>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Content).HasMaxLength(2000).IsRequired();
            e.HasOne(x => x.Match)
             .WithMany(x => x.Notes)
             .HasForeignKey(x => x.MatchId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
             .WithMany(x => x.Notes)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PlayerRating ──────────────────────────────────────────────────────
        modelBuilder.Entity<PlayerRating>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.MatchId, x.PlayerId }).IsUnique();
            e.Property(x => x.Rating).HasPrecision(3, 1);
            e.HasOne(x => x.Match)
             .WithMany(x => x.PlayerRatings)
             .HasForeignKey(x => x.MatchId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Player)
             .WithMany(x => x.Ratings)
             .HasForeignKey(x => x.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
