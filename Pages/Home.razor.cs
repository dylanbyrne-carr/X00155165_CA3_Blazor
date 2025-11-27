using Microsoft.AspNetCore.Components;
using F1RaceAnalytics.Models;
using F1RaceAnalytics.Services;

namespace F1RaceAnalytics.Pages;

public partial class Home
{
    [Inject]
    private OpenF1Service OpenF1Service { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private Dictionary<int, List<TrackInfo>> tracksByYear = new();
    private HashSet<int> expandedYears = new() { 2025 };
    private bool isLoading = true;
    private string? errorMessage;
    private List<Session> allSessions = new();

    private string driverSearch = "";
    private bool isSearching;
    private string? searchError;
    private Driver? selectedDriver;

    private int selectedYear;
    private int selectedRaceKey;
    private List<Meeting> races = new();
    private bool isLoadingRaces;
    private string? raceError;

    protected override async Task OnInitializedAsync()
    {
        await LoadAllData();
    }

    private async Task LoadAllData()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            var allMeetings = new List<Meeting>();
            allSessions = new List<Session>();
            
            for (int year = 2022; year <= 2025; year++)
            {
                var meetings = await OpenF1Service.GetMeetingsAsync(year);
                var sessions = await OpenF1Service.GetSessionsAsync(year);
                
                // Filter out Sprint sessions
                var regularRaces = sessions.Where(s => 
                    s.SessionType == "Race" && 
                    (s.SessionName == null || !s.SessionName.Contains("Sprint", StringComparison.OrdinalIgnoreCase))
                ).ToList();
                
                allMeetings.AddRange(meetings);
                allSessions.AddRange(regularRaces);
            }

            tracksByYear = allMeetings
                .GroupBy(m => m.Year)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new TrackInfo
                    {
                        MeetingKey = m.MeetingKey,
                        MeetingOfficialName = m.MeetingOfficialName,
                        CircuitShortName = m.CircuitShortName,
                        CountryName = m.CountryName,
                        Year = m.Year,
                        FlagUrl = GetCountryFlagUrl(m.CountryName)
                    }).ToList()
                );
        }
        catch (HttpRequestException ex)
        {
            errorMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task SearchDriver()
    {
        isSearching = true;
        searchError = null;
        selectedDriver = null;

        try
        {
            var raceSessions = allSessions
                .Where(s => s.SessionType == "Race")
                .OrderByDescending(s => s.DateStart)
                .ToList();

            if (raceSessions.Count == 0)
            {
                searchError = "No race sessions available.";
                return;
            }

            Session? sessionWithDriver = null;
            Driver? foundDriver = null;

            foreach (var session in raceSessions.Take(5))
            {
                try
                {
                    var drivers = await OpenF1Service.GetDriversAsync(session.SessionKey);

                    foundDriver = drivers.FirstOrDefault(d =>
                        d.FullName.Contains(driverSearch, StringComparison.OrdinalIgnoreCase) ||
                        d.BroadcastName.Contains(driverSearch, StringComparison.OrdinalIgnoreCase) ||
                        d.NameAcronym.Contains(driverSearch, StringComparison.OrdinalIgnoreCase) ||
                        d.DriverNumber.ToString() == driverSearch);

                    if (foundDriver != null)
                    {
                        sessionWithDriver = session;
                        break;
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (foundDriver == null)
            {
                searchError = $"No driver found matching '{driverSearch}'.";
                return;
            }

            Navigation.NavigateTo($"/driver/{foundDriver.DriverNumber}");
        }
        catch (Exception ex)
        {
            searchError = $"Error searching driver: {ex.Message}";
        }
        finally
        {
            isSearching = false;
        }
    }

    private string GetSearchBorderClass()
    {
        if (string.IsNullOrWhiteSpace(driverSearch)) return "border-gray-600";
        return driverSearch.Length >= 1 ? "border-green-500" : "border-red-500";
    }

    private async Task OnYearChanged()
    {
        if (selectedYear == 0) return;

        isLoadingRaces = true;
        raceError = null;
        selectedRaceKey = 0;
        races = new();

        try
        {
            if (selectedYear < 2022 || selectedYear > 2025)
            {
                raceError = "Please select a year between 2022 and 2025.";
                return;
            }

            var meetings = await OpenF1Service.GetMeetingsAsync(selectedYear);
            races = meetings.ToList();
        }
        catch (Exception ex)
        {
            raceError = $"Error loading races: {ex.Message}";
        }
        finally
        {
            isLoadingRaces = false;
        }
    }

    private void LoadRace()
    {
        if (selectedYear == 0 || selectedRaceKey == 0)
        {
            raceError = "Please select both year and race.";
            return;
        }

        var raceSession = allSessions.FirstOrDefault(s =>
            s.MeetingKey == selectedRaceKey &&
            s.SessionType == "Race");

        if (raceSession != null)
        {
            Navigation.NavigateTo($"race/{raceSession.SessionKey}");
        }
        else
        {
            raceError = "Race session not found.";
        }
    }

    private void ToggleYear(int year)
    {
        if(!expandedYears.Remove(year))
            expandedYears.Add(year);
    }

    private void ViewRaceResults(TrackInfo track)
    {
        var raceSession = allSessions.FirstOrDefault(s => 
            s.MeetingKey == track.MeetingKey && 
            s.SessionType == "Race");

        if (raceSession != null)
        {
            Navigation.NavigateTo($"race/{raceSession.SessionKey}");
        }
        else
        {
            errorMessage = $"No race session found for {track.CircuitShortName}";
        }
    }

    private static string GetCountryFlagUrl(string countryName)
    {
        var countryCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Australia"] = "au",
            ["Austria"] = "at",
            ["Azerbaijan"] = "az",
            ["Bahrain"] = "bh",
            ["Belgium"] = "be",
            ["Brazil"] = "br",
            ["Canada"] = "ca",
            ["China"] = "cn",
            ["Netherlands"] = "nl",
            ["Emilia Romagna"] = "it",
            ["France"] = "fr",
            ["Great Britain"] = "gb",
            ["Hungary"] = "hu",
            ["Italy"] = "it",
            ["Japan"] = "jp",
            ["Mexico"] = "mx",
            ["Monaco"] = "mc",
            ["Qatar"] = "qa",
            ["Saudi Arabia"] = "sa",
            ["Singapore"] = "sg",
            ["Spain"] = "es",
            ["USA"] = "us",
            ["United States"] = "us",
            ["UAE"] = "ae",
            ["Abu Dhabi"] = "ae",
            ["Las Vegas"] = "us",
            ["Miami"] = "us"
        };

        var countryCode = countryCodeMap.FirstOrDefault(kvp =>
            countryName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)).Value;

        if (!string.IsNullOrEmpty(countryCode))
        {
            return $"https://flagcdn.com/w320/{countryCode}.png";
        }

        return "";
    }
}