namespace F1RaceAnalytics.Models;

internal class LapChartData
{
    public string DriverName { get; set; } = string.Empty;
    public double LapTime { get; set; }
    public double GapToFastest { get; set; }
    public string TeamColour { get; set; } = string.Empty;
}