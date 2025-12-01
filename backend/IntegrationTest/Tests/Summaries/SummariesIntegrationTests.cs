using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using Manager.Models.Users;
using System.Net;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Summaries;

[Collection("IntegrationTests")]
public class SummariesIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : SummariesTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    #region Period Overview Tests

    [Fact(DisplayName = "Get period overview - Student can retrieve their own overview")]
    public async Task GetPeriodOverview_AsStudent_ShouldSucceed()
    {
        var student =  ClientFixture.GetUserInfo(Role.Student);

        // Act
        var overview = await GetPeriodOverviewAsync(student.UserId);

        // Assert
        overview.Should().NotBeNull();
        overview!.Period.Should().NotBeNull();
        overview.Period.StartDate.Should().BeBefore(overview.Period.EndDate);
        overview.Summary.Should().NotBeNull();
        overview.Summary.TotalAttempts.Should().BeGreaterThanOrEqualTo(0);
        overview.Summary.WordsLearned.Should().BeGreaterThanOrEqualTo(0);
        overview.Summary.AchievementsUnlocked.Should().BeGreaterThanOrEqualTo(0);
        overview.Summary.PracticeDays.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact(DisplayName = "Get period overview - Teacher can retrieve student overview")]
    public async Task GetPeriodOverview_TeacherForStudent_ShouldSucceed()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);

        // Act
        var overview = await GetPeriodOverviewAsync(studentInfo.UserId);

        // Assert
        overview.Should().NotBeNull();
        overview!.Period.Should().NotBeNull();
        overview.Summary.Should().NotBeNull();
    }

    [Fact(DisplayName = "Get period overview - Admin can retrieve any user overview")]
    public async Task GetPeriodOverview_AdminForAnyUser_ShouldSucceed()
    {
        // Arrange
        var admin = await LoginAsAsync(Role.Admin);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);

        // Act
        var overview = await GetPeriodOverviewAsync(studentInfo.UserId);

        // Assert
        overview.Should().NotBeNull();
        overview!.Period.Should().NotBeNull();
        overview.Summary.Should().NotBeNull();
    }

    [Fact(DisplayName = "Get period overview - With custom date range")]
    public async Task GetPeriodOverview_WithCustomDateRange_ShouldSucceed()
    {
        // Arrange
        var student = await LoginAsAsync(Role.Student);
        var startDate = DateTime.UtcNow.AddDays(-14);
        var endDate = DateTime.UtcNow.AddDays(-7);

        // Act
        var overview = await GetPeriodOverviewAsync(student.UserId, startDate, endDate);

        // Assert
        overview.Should().NotBeNull();
        overview!.Period.StartDate.Should().BeCloseTo(startDate, TimeSpan.FromMinutes(1));
        overview.Period.EndDate.Should().BeCloseTo(endDate, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Game Practice Tests

    [Fact(DisplayName = "Get game practice - Student can retrieve their own game practice")]
    public async Task GetGamePractice_AsStudent_ShouldSucceed()
    {
        // Arrange
        var student = await LoginAsAsync(Role.Student);

        // Act
        var gamePractice = await GetPeriodGamePracticeAsync(student.UserId);

        // Assert
        gamePractice.Should().NotBeNull();
        gamePractice!.Summary.Should().NotBeNull();
        gamePractice.Summary.TotalAttempts.Should().BeGreaterThanOrEqualTo(0);
        gamePractice.Summary.SuccessRate.Should().BeInRange(0, 100);
        gamePractice.Summary.AverageAccuracy.Should().BeInRange(0, 100);
        gamePractice.ByGameType.Should().NotBeNull();
        gamePractice.Daily.Should().NotBeNull();
        gamePractice.Mistakes.Should().NotBeNull();
        gamePractice.Mistakes.Summary.Should().NotBeNull();
        gamePractice.Mistakes.Patterns.Should().NotBeNull();
        gamePractice.Mistakes.UncorrectedExamples.Should().NotBeNull();
    }

    [Fact(DisplayName = "Get game practice - Teacher can retrieve student game practice")]
    public async Task GetGamePractice_TeacherForStudent_ShouldSucceed()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);

        // Act
        var gamePractice = await GetPeriodGamePracticeAsync(studentInfo.UserId);

        // Assert
        gamePractice.Should().NotBeNull();
        gamePractice!.Summary.Should().NotBeNull();
        gamePractice.Mistakes.Should().NotBeNull();
    }

    [Fact(DisplayName = "Get game practice - With custom date range")]
    public async Task GetGamePractice_WithCustomDateRange_ShouldSucceed()
    {
        // Arrange
        var student = await LoginAsAsync(Role.Student);
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Act
        var gamePractice = await GetPeriodGamePracticeAsync(student.UserId, startDate, endDate);

        // Assert
        gamePractice.Should().NotBeNull();
        gamePractice!.Summary.Should().NotBeNull();
    }

    #endregion

    #region Word Cards Tests

    [Fact(DisplayName = "Get word cards - Student can retrieve their own word cards")]
    public async Task GetWordCards_AsStudent_ShouldSucceed()
    {
        // Arrange
        var student = await LoginAsAsync(Role.Student);

        // Act
        var wordCards = await GetPeriodWordCardsAsync(student.UserId);

        // Assert
        wordCards.Should().NotBeNull();
        wordCards!.Summary.Should().NotBeNull();
        wordCards.Summary.TotalCards.Should().BeGreaterThanOrEqualTo(0);
        wordCards.Summary.NewInPeriod.Should().BeGreaterThanOrEqualTo(0);
        wordCards.Summary.LearnedInPeriod.Should().BeGreaterThanOrEqualTo(0);
        wordCards.Summary.TotalLearned.Should().BeGreaterThanOrEqualTo(0);
        wordCards.RecentLearned.Should().NotBeNull();
        wordCards.NewCards.Should().NotBeNull();
    }

    [Fact(DisplayName = "Get word cards - Teacher can retrieve student word cards")]
    public async Task GetWordCards_TeacherForStudent_ShouldSucceed()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);

        // Act
        var wordCards = await GetPeriodWordCardsAsync(studentInfo.UserId);

        // Assert
        wordCards.Should().NotBeNull();
        wordCards!.Summary.Should().NotBeNull();
    }

    #endregion

    #region Achievements Tests

    [Fact(DisplayName = "Get achievements - Student can retrieve their own achievements")]
    public async Task GetAchievements_AsStudent_ShouldSucceed()
    {
        // Arrange
        var student = await LoginAsAsync(Role.Student);

        // Act
        var achievements = await GetPeriodAchievementsAsync(student.UserId);

        // Assert
        achievements.Should().NotBeNull();
        achievements!.UnlockedInPeriod.Should().NotBeNull();
    }

    [Fact(DisplayName = "Get achievements - Teacher can retrieve student achievements")]
    public async Task GetAchievements_TeacherForStudent_ShouldSucceed()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);

        // Act
        var achievements = await GetPeriodAchievementsAsync(studentInfo.UserId);

        // Assert
        achievements.Should().NotBeNull();
        achievements!.UnlockedInPeriod.Should().NotBeNull();
    }

    #endregion

    #region Authorization Tests

    [Fact(DisplayName = "Authentication required - All endpoints return 401 when unauthenticated")]
    public async Task AuthenticationRequired_Unauthenticated_ShouldReturn401()
    {
        // Arrange
        ClientFixture.ClearSession();
        var userId = Guid.NewGuid();

        // Act & Assert - Overview
        var overviewResponse = await Client.GetAsync(SummaryRoutes.GetPeriodOverview(userId));
        overviewResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - Game Practice
        var gamePracticeResponse = await Client.GetAsync(SummaryRoutes.GetPeriodGamePractice(userId));
        gamePracticeResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - Word Cards
        var wordCardsResponse = await Client.GetAsync(SummaryRoutes.GetPeriodWordCards(userId));
        wordCardsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - Achievements
        var achievementsResponse = await Client.GetAsync(SummaryRoutes.GetPeriodAchievements(userId));
        achievementsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Invalid userId - Returns 400 for empty guid")]
    public async Task InvalidUserId_EmptyGuid_ShouldReturn400()
    {
        // Arrange
        await LoginAsAsync(Role.Teacher);
        var emptyUserId = Guid.Empty;

        // Act & Assert - Overview
        var overviewResponse = await Client.GetAsync(SummaryRoutes.GetPeriodOverview(emptyUserId));
        overviewResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act & Assert - Game Practice
        var gamePracticeResponse = await Client.GetAsync(SummaryRoutes.GetPeriodGamePractice(emptyUserId));
        gamePracticeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act & Assert - Word Cards
        var wordCardsResponse = await Client.GetAsync(SummaryRoutes.GetPeriodWordCards(emptyUserId));
        wordCardsResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act & Assert - Achievements
        var achievementsResponse = await Client.GetAsync(SummaryRoutes.GetPeriodAchievements(emptyUserId));
        achievementsResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
