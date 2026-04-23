namespace OffsideIQ.Core.Entities;

public class MatchStats
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }

    // Possession
    public decimal HomePossession { get; set; }  // 0-100
    public decimal AwayPossession { get; set; }

    // Shots
    public int HomeShotsTotal { get; set; }
    public int HomeShotsOnTarget { get; set; }
    public int AwayShotsTotal { get; set; }
    public int AwayShotsOnTarget { get; set; }

    // Passes
    public int HomePasses { get; set; }
    public int HomePassAccuracy { get; set; } // 0-100
    public int AwayPasses { get; set; }
    public int AwayPassAccuracy { get; set; }

    // Discipline
    public int HomeYellowCards { get; set; }
    public int HomeRedCards { get; set; }
    public int AwayYellowCards { get; set; }
    public int AwayRedCards { get; set; }

    // Corners & Fouls
    public int HomeCorners { get; set; }
    public int AwayCorners { get; set; }
    public int HomeFouls { get; set; }
    public int AwayFouls { get; set; }

    // xG (expected goals) - optional
    public decimal? HomeXg { get; set; }
    public decimal? AwayXg { get; set; }

    // Navigation
    public Match Match { get; set; } = null!;
}
