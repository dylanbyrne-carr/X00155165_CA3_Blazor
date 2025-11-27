using Microsoft.AspNetCore.Components;
using F1RaceAnalytics.Models;
using F1RaceAnalytics.Services;

namespace F1RaceAnalytics.Pages;

public partial class DriverProfile : ComponentBase
{
    [Parameter]
    public int DriverNumber { get; set; }

    [Inject]
    private OpenF1Service OpenF1Service { get; set; } = default!;

    private Driver? driver;
    private DriverStats allStats = new();
    private Dictionary<int, DriverStats> seasonStats = new();
    private Dictionary<int, List<RaceResult>> seasonRaces = new();
    private HashSet<int> expandedSeasons = new();
    private bool isLoading = true;
    private bool isLoadingStats = false;
    private string? errorMessage;
    private int currentRaceIndex;
    private int totalRaces;
    private string currentRaceName = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadDriverData();
    }

    private async Task LoadDriverData()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            var sessions = await OpenF1Service.GetSessionsAsync(2023, 2025);
            var raceSessions = sessions.Where(s => s.SessionName == "Race").OrderByDescending(s => s.DateStart).ToList();

            // Check most recent session first (driver likely participated)
            Driver? foundDriver = null;
            Session? driverSession = null;
            
            foreach (var session in raceSessions.Take(5)) // Check last 5 races only
            {
                try
                {
                    var drivers = await OpenF1Service.GetDriversAsync(session.SessionKey);
                    foundDriver = drivers.FirstOrDefault(d => d.DriverNumber == DriverNumber);
                    if (foundDriver != null)
                    {
                        driverSession = session;
                        break;
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (foundDriver == null || driverSession == null)
            {
                errorMessage = "Driver not found in any recent race sessions.";
                return;
            }

            driver = foundDriver;

            isLoadingStats = true;
            await LoadDriverStatsAndRaces(raceSessions);
        }
        catch (Exception ex)
        {
            errorMessage = $"Error loading driver data: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            isLoadingStats = false;
        }
    }

    private async Task LoadDriverStatsAndRaces(List<Session> raceSessions)
    {
        totalRaces = raceSessions.Count;
        seasonStats.Clear();
        seasonRaces.Clear();
        allStats = new DriverStats();
        
        // Track positions for actual average calculation
        var seasonPositions = new Dictionary<int, List<int>>();
        var allPositions = new List<int>();

        foreach (var (session, index) in raceSessions.Select((s, i) => (s, i)))
        {
            currentRaceIndex = index + 1;
            currentRaceName = $"{session.CountryName} Grand Prix";
            StateHasChanged(); // Show progress

            try
            {
                await Task.Delay(100); // Prevent rate limiting
                var positions = await OpenF1Service.GetPositionsAsync(session.SessionKey);
                var driverPositions = positions.Where(p => p.DriverNumber == DriverNumber).ToList();

                if (!driverPositions.Any()) continue;

                var startPosition = driverPositions.OrderBy(p => p.Date).First().Position;
                var finalPosition = driverPositions.OrderBy(p => p.Date).Last().Position;
                var points = GetRacePoints(finalPosition);

                // Update season stats
                if (!seasonStats.ContainsKey(session.Year))
                {
                    seasonStats[session.Year] = new DriverStats();
                    seasonRaces[session.Year] = new();
                    seasonPositions[session.Year] = new();
                }

                var stats = seasonStats[session.Year];
                stats.TotalRaces++;
                allStats.TotalRaces++;
                
                // Track positions for average
                seasonPositions[session.Year].Add(finalPosition);
                allPositions.Add(finalPosition);

                if (finalPosition <= 3)
                {
                    stats.Podiums++;
                    allStats.Podiums++;
                }

                if (finalPosition < stats.BestPosition)
                    stats.BestPosition = finalPosition;
                if (finalPosition < allStats.BestPosition)
                    allStats.BestPosition = finalPosition;

                if (finalPosition > stats.WorstPosition)
                    stats.WorstPosition = finalPosition;
                if (finalPosition > allStats.WorstPosition)
                    allStats.WorstPosition = finalPosition;

                stats.Points += points;
                allStats.Points += points;

                // Add race result
                seasonRaces[session.Year].Add(new RaceResult
                {
                    RaceName = $"{session.CountryName} Grand Prix",
                    RaceDate = session.DateStart,
                    StartPosition = startPosition,
                    FinishPosition = finalPosition,
                    PositionChange = startPosition - finalPosition,
                    Points = points
                });
            }
            catch
            {
                // Skip races with errors
            }

            StateHasChanged();
        }


        foreach (var (year, positions) in seasonPositions)
        {
            if (positions.Count > 0)
                seasonStats[year].AveragePosition = positions.Average();
        }

        if (allPositions.Count > 0)
            allStats.AveragePosition = allPositions.Average();
    }

    private void ToggleSeason(int year)
    {
        if (expandedSeasons.Contains(year))
            expandedSeasons.Remove(year);
        else
            expandedSeasons.Add(year);
    }

    private static int GetRacePoints(int position)
    {
        return position switch
        {
            1 => 25,
            2 => 18,
            3 => 15,
            4 => 12,
            5 => 10,
            6 => 8,
            7 => 6,
            8 => 4,
            9 => 2,
            10 => 1,
            _ => 0
        };
    }

    private static string GetCountryFlag(string countryCode)
    {
        return $"https://flagcdn.com/w80/{countryCode.ToLowerInvariant()}.png";
    }
}

// Helper class for race-by-race results
