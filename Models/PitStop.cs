namespace F1RaceAnalytics.Models;

public class PitStop
{
    public DateTime Date { get; set; }
    public int DriverNumber { get; set; }
    public int LapNumber { get; set; }
    public double PitDuration { get; set; }
}