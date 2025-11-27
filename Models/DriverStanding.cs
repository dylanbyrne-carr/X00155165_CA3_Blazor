namespace F1RaceAnalytics.Models;

internal class DriverStanding
{
    public int Position { get; set; }
    public int DriverNumber { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string TeamColour { get; set; } = string.Empty;
    public string? HeadshotUrl { get; set; }
    public int PitStops { get; set; }
    public double BestLapTime { get; set; }
    public int PositionDelta { get; set; }
    public bool HasFastestLap { get; set; }
    public List<TireStint> TireStints { get; set; } = new();
}