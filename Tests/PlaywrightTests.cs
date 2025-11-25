using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;

namespace Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PlaywrightTests : PageTest
{
    private const string BaseUrl = "http://localhost:5171";

    [Test]
    public async Task HomePage_LoadsAndDisplaysRaces()
    {
        await Page.GotoAsync(BaseUrl);
        
        // Title is "F1 Race Analytics" not "Home"
        await Expect(Page).ToHaveTitleAsync("F1 Race Analytics");
        
        var header = Page.Locator("text=F1 Race Analytics");
        await Expect(header).ToBeVisibleAsync();
        
        var yearSection = Page.Locator("text=2024");
        await Expect(yearSection).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomePage_CanExpandYearAccordion()
    {
        await Page.GotoAsync(BaseUrl);
        
        // Click on 2024 to expand
        await Page.Locator("text=2024").First.ClickAsync();
        
        // Should see race locations
        await Page.WaitForSelectorAsync("text=View Results", new() { Timeout = 10000 });
        
        var viewResultsButton = Page.Locator("text=View Results").First;
        await Expect(viewResultsButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task RaceResultsPage_DisplaysDriverStandings()
    {
        await Page.GotoAsync($"{BaseUrl}/race/9625");
        
        await Page.WaitForSelectorAsync("text=Race Results", new() { Timeout = 15000 });
        
        var resultsHeader = Page.Locator("text=Race Results");
        await Expect(resultsHeader).ToBeVisibleAsync();
        
        var homeLink = Page.Locator("a:has-text('Home')");
        await Expect(homeLink).ToBeVisibleAsync();
    }
}