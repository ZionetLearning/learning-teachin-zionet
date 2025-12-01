using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Models.Summaries;
using Manager.Models.Users;
using System.Data;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Summaries;

[Collection("IntegrationTests")]
public abstract class SummariesTestBase(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    public override async Task InitializeAsync()
    {
        await ClientFixture.LoginAsync(Role.Student);

        SignalRFixture.ClearReceivedMessages();
    }

    /// <summary>
    /// Logs in as a predefined test user of the specified role.
    /// Returns UserInfo for the logged-in user.
    /// </summary>
    protected async Task<UserInfo> LoginAsAsync(Role role)
    {
        await ClientFixture.LoginAsync(role);
        await EnsureSignalRStartedAsync();
        return ClientFixture.GetUserInfo(role);
    }

    /// <summary>
    /// Gets period overview for a user.
    /// </summary>
    protected async Task<GetPeriodOverviewResponse?> GetPeriodOverviewAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var url = startDate.HasValue && endDate.HasValue
            ? SummaryRoutes.GetPeriodOverviewWithDates(userId, startDate.Value, endDate.Value)
            : SummaryRoutes.GetPeriodOverview(userId);

        var response = await Client.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await ReadAsJsonAsync<GetPeriodOverviewResponse>(response);
    }

    /// <summary>
    /// Gets period game practice for a user.
    /// </summary>
    protected async Task<GetGamePracticeSummaryResponse?> GetPeriodGamePracticeAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var url = startDate.HasValue && endDate.HasValue
            ? SummaryRoutes.GetPeriodGamePracticeWithDates(userId, startDate.Value, endDate.Value)
            : SummaryRoutes.GetPeriodGamePractice(userId);

        var response = await Client.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await ReadAsJsonAsync<GetGamePracticeSummaryResponse>(response);
    }

    /// <summary>
    /// Gets period word cards for a user.
    /// </summary>
    protected async Task<GetPeriodWordCardsResponse?> GetPeriodWordCardsAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var url = startDate.HasValue && endDate.HasValue
            ? SummaryRoutes.GetPeriodWordCardsWithDates(userId, startDate.Value, endDate.Value)
            : SummaryRoutes.GetPeriodWordCards(userId);

        var response = await Client.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await ReadAsJsonAsync<GetPeriodWordCardsResponse>(response);
    }

    /// <summary>
    /// Gets period achievements for a user.
    /// </summary>
    protected async Task<GetPeriodAchievementsResponse?> GetPeriodAchievementsAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var url = startDate.HasValue && endDate.HasValue
            ? SummaryRoutes.GetPeriodAchievementsWithDates(userId, startDate.Value, endDate.Value)
            : SummaryRoutes.GetPeriodAchievements(userId);

        var response = await Client.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await ReadAsJsonAsync<GetPeriodAchievementsResponse>(response);
    }
}
