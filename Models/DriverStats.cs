namespace F1RaceAnalytics.Models;

internal class DriverStats
{
    public int TotalRaces { get; set; }
    public int Podiums { get; set; }
    public int BestPosition { get; set; } = 999;
    public int WorstPosition { get; set; }
    public double AveragePosition { get; set; }
    public int Points { get; set; }
}