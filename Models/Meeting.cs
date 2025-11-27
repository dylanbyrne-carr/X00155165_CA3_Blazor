namespace F1RaceAnalytics.Models;

public class Meeting
{
    public int MeetingKey { get; set; }
    public string MeetingName { get; set; } = string.Empty;
    public string MeetingOfficialName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string CircuitShortName { get; set; } = string.Empty;
    public DateTime DateStart { get; set; }
    public int Year { get; set; }
}