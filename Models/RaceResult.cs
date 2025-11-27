namespace F1RaceAnalytics.Models;

public class RaceResult
{
    public string RaceName { get; set; } = "";
    public DateTime RaceDate { get; set; }
    public int StartPosition { get; set; }
    public int FinishPosition { get; set; }
    public int PositionChange { get; set; }
    public int Points { get; set; }
}