using OffsideIQ.Core.DTOs;
using OffsideIQ.Core.Enums;
using OffsideIQ.Core.Interfaces;

namespace OffsideIQ.Application.Services;

/// <summary>
/// Rule-based insight engine. Generates contextual football analytics 
/// from match data without machine learning.
/// </summary>
public class InsightService : IInsightService
{
    private readonly IMatchRepository _matches;
    private readonly ITeamRepository _teams;

    public InsightService(IMatchRepository matches, ITeamRepository teams)
    {
        _matches = matches;
        _teams = teams;
    }

    public async Task<IEnumerable<InsightDto>> GenerateForMatchAsync(Guid matchId)
    {
        var insights = new List<InsightDto>();
        var match = await _matches.GetWithDetailsAsync(matchId);
        if (match is null || match.Status != MatchStatus.Completed) return insights;

        var stats = match.Stats;

        // Dominant possession
        if (stats is not null)
        {
            if (stats.HomePossession >= 65)
                insights.Add(new InsightDto("possession", "info",
                    "High Possession Control",
                    $"{match.HomeTeam.Name} dominated possession at {stats.HomePossession}%.",
                    match.HomeTeamId));
            else if (stats.AwayPossession >= 65)
                insights.Add(new InsightDto("possession", "info",
                    "High Possession Control",
                    $"{match.AwayTeam.Name} dominated possession at {stats.AwayPossession}%.",
                    match.AwayTeamId));

            // Shooting efficiency
            if (stats.HomeShotsTotal > 0)
            {
                var homeEff = (double)stats.HomeShotsOnTarget / stats.HomeShotsTotal * 100;
                if (homeEff < 30 && stats.HomeShotsTotal >= 8)
                    insights.Add(new InsightDto("shooting", "negative",
                        "Poor Shot Accuracy",
                        $"{match.HomeTeam.Name} had {stats.HomeShotsOnTarget} shots on target from {stats.HomeShotsTotal} attempts ({homeEff:F0}%).",
                        match.HomeTeamId));
            }

            // Discipline issues
            if (stats.HomeRedCards > 0 || stats.AwayRedCards > 0)
                insights.Add(new InsightDto("discipline", "warning",
                    "Red Card Incident",
                    $"This match saw {stats.HomeRedCards + stats.AwayRedCards} red card(s). Discipline was a factor.",
                    null));

            if (stats.HomeYellowCards + stats.AwayYellowCards >= 6)
                insights.Add(new InsightDto("discipline", "warning",
                    "Feisty Encounter",
                    $"A combined {stats.HomeYellowCards + stats.AwayYellowCards} yellow cards indicates a heated contest.",
                    null));

            // High xG vs goals scored
            if (stats.HomeXg.HasValue && stats.HomeXg > 2.5m && match.HomeScore <= 1)
                insights.Add(new InsightDto("xg", "info",
                    "Underperformed xG",
                    $"{match.HomeTeam.Name} had an xG of {stats.HomeXg:F2} but scored only {match.HomeScore}.",
                    match.HomeTeamId));
        }

        // Goal patterns
        int total = match.HomeScore + match.AwayScore;
        if (total == 0)
            insights.Add(new InsightDto("scoring", "info", "Goalless Draw",
                "Neither side could find the net. Both defenses were on top.", null));
        else if (total >= 5)
            insights.Add(new InsightDto("scoring", "positive", "Goal Fest",
                $"An entertaining {total}-goal thriller between {match.HomeTeam.Name} and {match.AwayTeam.Name}.", null));

        // Comeback
        if (match.AwayScore > match.HomeScore + 1)
            insights.Add(new InsightDto("form", "positive", "Away Dominance",
                $"{match.AwayTeam.Name} won convincingly on the road — a strong away performance.",
                match.AwayTeamId));

        return insights;
    }

    public async Task<IEnumerable<InsightDto>> GenerateForTeamAsync(Guid teamId)
    {
        var insights = new List<InsightDto>();
        var team = await _teams.GetByIdAsync(teamId);
        if (team is null) return insights;

        var matches = (await _matches.GetByTeamAsync(teamId, 5)).ToList();
        if (!matches.Any()) return insights;

        var completedMatches = matches.Where(m => m.Status == MatchStatus.Completed).ToList();
        if (!completedMatches.Any()) return insights;

        var results = completedMatches.Select(m =>
        {
            bool isHome = m.HomeTeamId == teamId;
            int gf = isHome ? m.HomeScore : m.AwayScore;
            int ga = isHome ? m.AwayScore : m.HomeScore;
            return (gf, ga, result: gf > ga ? MatchResult.Win : gf < ga ? MatchResult.Loss : MatchResult.Draw);
        }).ToList();

        int wins = results.Count(r => r.result == MatchResult.Win);
        int losses = results.Count(r => r.result == MatchResult.Loss);
        double avgGoals = results.Average(r => r.gf);
        double avgConceded = results.Average(r => r.ga);

        // Win streak
        int streak = 0;
        foreach (var r in results)
        {
            if (r.result == MatchResult.Win) streak++;
            else break;
        }

        if (streak >= 3)
            insights.Add(new InsightDto("streak", "positive", "On Fire 🔥",
                $"{team.Name} is on a {streak}-match winning streak.", teamId));
        else if (losses >= 4)
            insights.Add(new InsightDto("form", "negative", "Concerning Form",
                $"{team.Name} has lost {losses} of their last {completedMatches.Count} matches.", teamId));
        else if (wins >= 4)
            insights.Add(new InsightDto("form", "positive", "Excellent Form",
                $"{team.Name} has won {wins} of their last {completedMatches.Count} matches.", teamId));

        // Scoring
        if (avgGoals < 0.8)
            insights.Add(new InsightDto("scoring", "negative", "Attacking Struggles",
                $"{team.Name} averages only {avgGoals:F1} goals per game in recent matches.", teamId));
        else if (avgGoals >= 2.5)
            insights.Add(new InsightDto("scoring", "positive", "Clinical Attack",
                $"{team.Name} is averaging {avgGoals:F1} goals per game — one of the best attacking forms.", teamId));

        // Defense
        if (avgConceded >= 2.5)
            insights.Add(new InsightDto("defense", "negative", "Defensive Vulnerability",
                $"{team.Name} is conceding {avgConceded:F1} goals per game on average.", teamId));
        else if (avgConceded < 0.5 && completedMatches.Count >= 3)
            insights.Add(new InsightDto("defense", "positive", "Solid Backline",
                $"{team.Name} has kept a near-clean-sheet record, conceding just {avgConceded:F1} goals per game.", teamId));

        return insights;
    }

    public async Task<IEnumerable<InsightDto>> GenerateGlobalAsync()
    {
        var insights = new List<InsightDto>();
        var recentMatches = (await _matches.GetRecentAsync(20))
            .Where(m => m.Status == MatchStatus.Completed).ToList();

        if (!recentMatches.Any()) return insights;

        double avgGoals = recentMatches.Average(m => m.HomeScore + m.AwayScore);
        int draws = recentMatches.Count(m => m.HomeScore == m.AwayScore);
        double drawRate = (double)draws / recentMatches.Count * 100;
        int homeWins = recentMatches.Count(m => m.HomeScore > m.AwayScore);
        double homeWinRate = (double)homeWins / recentMatches.Count * 100;

        if (avgGoals < 1.5)
            insights.Add(new InsightDto("scoring", "info", "Low Scoring Period",
                $"Recent matches average only {avgGoals:F1} goals. Defenses are on top.", null));
        else if (avgGoals >= 3.0)
            insights.Add(new InsightDto("scoring", "positive", "High Scoring Period",
                $"Games are averaging {avgGoals:F1} goals — attackers are in fine form.", null));

        if (drawRate >= 40)
            insights.Add(new InsightDto("trend", "info", "Draw-Heavy Trend",
                $"{drawRate:F0}% of recent matches have ended in draws — tight contests across the board.", null));

        if (homeWinRate >= 60)
            insights.Add(new InsightDto("trend", "info", "Home Advantage Pronounced",
                $"Home sides are winning {homeWinRate:F0}% of games — home advantage is significant right now.", null));

        return insights;
    }

    public async Task<PredictionDto> PredictMatchAsync(Guid homeTeamId, Guid awayTeamId)
    {
        var homeTeam = await _teams.GetByIdAsync(homeTeamId) ?? throw new KeyNotFoundException("Home team not found.");
        var awayTeam = await _teams.GetByIdAsync(awayTeamId) ?? throw new KeyNotFoundException("Away team not found.");

        var homeMatches = (await _matches.GetByTeamAsync(homeTeamId, 5))
            .Where(m => m.Status == MatchStatus.Completed).ToList();
        var awayMatches = (await _matches.GetByTeamAsync(awayTeamId, 5))
            .Where(m => m.Status == MatchStatus.Completed).ToList();

        double homePoints = CalculateFormPoints(homeMatches, homeTeamId);
        double awayPoints = CalculateFormPoints(awayMatches, awayTeamId);

        // Home advantage boost (well-established in football analytics)
        double homeBoost = 0.15;
        double totalPoints = homePoints + homeBoost + awayPoints;
        if (totalPoints == 0) totalPoints = 1;

        decimal homeWinProb = (decimal)((homePoints + homeBoost) / (totalPoints + 0.3));
        decimal awayWinProb = (decimal)(awayPoints / (totalPoints + 0.3));
        decimal drawProb = Math.Max(0.15m, 1m - homeWinProb - awayWinProb);

        // Normalize
        decimal sum = homeWinProb + awayWinProb + drawProb;
        homeWinProb = Math.Round(homeWinProb / sum * 100, 1);
        awayWinProb = Math.Round(awayWinProb / sum * 100, 1);
        drawProb = Math.Round(100 - homeWinProb - awayWinProb, 1);

        string outcome = homeWinProb > awayWinProb && homeWinProb > drawProb ? "Home Win"
            : awayWinProb > homeWinProb && awayWinProb > drawProb ? "Away Win"
            : "Draw";

        var factors = BuildPredictionFactors(homeTeam.Name, awayTeam.Name,
            homeMatches, awayMatches, homeTeamId, awayTeamId);

        return new PredictionDto(homeTeamId, homeTeam.Name, awayTeamId, awayTeam.Name,
            outcome, homeWinProb, drawProb, awayWinProb, factors);
    }

    private static double CalculateFormPoints(List<Core.Entities.Match> matches, Guid teamId)
    {
        if (!matches.Any()) return 0.33;
        double pts = 0;
        int weight = matches.Count;
        foreach (var m in matches)
        {
            bool isHome = m.HomeTeamId == teamId;
            int gf = isHome ? m.HomeScore : m.AwayScore;
            int ga = isHome ? m.AwayScore : m.HomeScore;
            pts += weight * (gf > ga ? 3 : gf == ga ? 1 : 0);
            weight--;
        }
        return pts;
    }

    private static List<string> BuildPredictionFactors(
        string homeName, string awayName,
        List<Core.Entities.Match> homeMatches,
        List<Core.Entities.Match> awayMatches,
        Guid homeTeamId, Guid awayTeamId)
    {
        var factors = new List<string> { "Home advantage applied (+15% boost)" };

        int homeWins = homeMatches.Count(m =>
            (m.HomeTeamId == homeTeamId && m.HomeScore > m.AwayScore) ||
            (m.AwayTeamId == homeTeamId && m.AwayScore > m.HomeScore));
        int awayWins = awayMatches.Count(m =>
            (m.HomeTeamId == awayTeamId && m.HomeScore > m.AwayScore) ||
            (m.AwayTeamId == awayTeamId && m.AwayScore > m.HomeScore));

        factors.Add($"{homeName}: {homeWins}W in last {homeMatches.Count} games");
        factors.Add($"{awayName}: {awayWins}W in last {awayMatches.Count} games");

        if (!homeMatches.Any()) factors.Add($"{homeName}: Insufficient match data — result uncertain");
        if (!awayMatches.Any()) factors.Add($"{awayName}: Insufficient match data — result uncertain");

        return factors;
    }
}
