using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace F1RaceAnalytics.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PlaywrightTests : PageTest
{
    private const string BaseUrl = "http://localhost:5171";

    [Test]
    public async Task Test1_HomePage_LoadsAndDisplaysRaces()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page).ToHaveTitleAsync("F1 Race Analytics");
        var yearSections = Page.Locator("text=2024 Season, text=2025 Season");
        await Expect(yearSections.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Test2_HomePage_CanExpandYearAccordion()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        var viewResultsButtons = Page.Locator("button:has-text('View Results')");
        await Expect(viewResultsButtons.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Test3_RaceResultsPage_DisplaysDriverStandings()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        await Page.Locator("button:has-text('View Results')").First.ClickAsync();
        await Page.WaitForURLAsync("**/race/**", new() { Timeout = 10000 });
        var raceHeading = Page.Locator("h1, h2").First;
        await Expect(raceHeading).ToBeVisibleAsync();
    }

    [Test]
    public async Task Test4_Navigation_CanNavigateBetweenHomeAndRacePage()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        await Page.Locator("button:has-text('View Results')").First.ClickAsync();
        await Page.WaitForURLAsync("**/race/**", new() { Timeout = 10000 });
        await Page.Locator("a:has-text('Home'), button:has-text('Home')").First.ClickAsync();
        await Page.WaitForURLAsync(BaseUrl, new() { Timeout = 5000 });
        await Expect(Page.Locator("text=Select a Race")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Test5_DirectUrlNavigation_RacePageLoadsCorrectly()
    {
        await Page.GotoAsync($"{BaseUrl}/race/9693");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var heading = Page.Locator("h1, h2").First;
        await Expect(heading).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task Test6_MultipleRaceSelection_LoadsDifferentRaces()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        var firstButton = Page.Locator("button:has-text('View Results')").First;
        await firstButton.ClickAsync();
        await Page.WaitForURLAsync("**/race/**", new() { Timeout = 10000 });
        var firstUrl = Page.Url;
        await Page.Locator("a:has-text('Home'), button:has-text('Home')").First.ClickAsync();
        await Page.WaitForURLAsync(BaseUrl, new() { Timeout = 5000 });
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        var secondButton = Page.Locator("button:has-text('View Results')").Nth(1);
        await secondButton.ClickAsync();
        await Page.WaitForURLAsync("**/race/**", new() { Timeout = 10000 });
        var secondUrl = Page.Url;
        Assert.That(firstUrl, Is.Not.EqualTo(secondUrl), "Different races should have different URLs");
    }

    [Test]
    public async Task Test7_YearAccordion_CanSwitchBetweenSeasons()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        var season2025Races = await Page.Locator("button:has-text('View Results')").CountAsync();
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.Locator("text=2024 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        var season2024Races = await Page.Locator("button:has-text('View Results')").CountAsync();
        Assert.That(season2025Races, Is.GreaterThan(0), "2025 season should have races");
        Assert.That(season2024Races, Is.GreaterThan(0), "2024 season should have races");
    }

    [Test]
    public async Task Test8_HomePage_DisplaysTrackImages()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        var trackImages = Page.Locator("img[alt*='Circuit']");
        await Expect(trackImages.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        var firstImageSrc = await trackImages.First.GetAttributeAsync("src");
        Assert.That(firstImageSrc, Is.Not.Null.And.Not.Empty, "Track images should have src attributes");
    }

    [Test]
    public async Task Test9_RacePage_DisplaysRaceInformation()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        await Page.Locator("button:has-text('View Results')").First.ClickAsync();
        await Page.WaitForURLAsync("**/race/**", new() { Timeout = 10000 });
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 15000 });
        var pageContent = await Page.Locator("body").TextContentAsync();
        Assert.That(pageContent, Is.Not.Null.And.Not.Empty, "Race page should display content");
    }

    [Test]
    public async Task Test10_Application_RespondsToWindowResize()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator("text=F1 Race Analytics")).ToBeVisibleAsync();
        await Page.SetViewportSizeAsync(768, 1024);
        await Page.ReloadAsync();
        await Expect(Page.Locator("text=F1 Race Analytics")).ToBeVisibleAsync();
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.ReloadAsync();
        await Expect(Page.Locator("text=F1 Race Analytics")).ToBeVisibleAsync();
        await Page.Locator("text=2025 Season").ClickAsync();
        await Page.WaitForSelectorAsync("button:has-text('View Results')", new() { Timeout = 5000 });
        var buttons = Page.Locator("button:has-text('View Results')");
        await Expect(buttons.First).ToBeVisibleAsync();
    }
}