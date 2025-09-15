using FluentAssertions;
using Manager.Endpoints;
using Manager.Models.Users;
using Manager.Services.Clients.Accessor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Manager.Constants;

namespace ManagerUnitTests.Endpoints;

public class UsersEndpointsTests
{
    private readonly Mock<IAccessorClient> _mockAccessor = new(MockBehavior.Strict);
    private readonly Mock<ILogger<object>> _mockLogger = new();

    // ---- CREATE USER ----
    [Fact]
    public async Task CreateUser_Should_Return_BadRequest_When_InvalidRole()
    {
        var newUser = MakeCreateUser("fail@test.com", role: "alien");
        var ctx = new DefaultHttpContext();

        var result = await Invoke("CreateUserAsync", newUser, _mockAccessor.Object, _mockLogger.Object, ctx);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreateUser_Should_Return_Conflict_When_Duplicate()
    {
        var newUser = MakeCreateUser("dupe@test.com");
        var ctx = new DefaultHttpContext();

        _mockAccessor.Setup(a => a.CreateUserAsync(It.IsAny<UserModel>())).ReturnsAsync(false);

        var result = await Invoke("CreateUserAsync", newUser, _mockAccessor.Object, _mockLogger.Object, ctx);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        _mockAccessor.VerifyAll();
    }

    [Fact]
    public async Task CreateUser_Should_Return_Created_When_Valid()
    {
        var newUser = MakeCreateUser("ok@test.com");
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Accept-Language"] = "he-IL";

        _mockAccessor.Setup(a => a.CreateUserAsync(It.IsAny<UserModel>())).ReturnsAsync(true);

        var result = await Invoke("CreateUserAsync", newUser, _mockAccessor.Object, _mockLogger.Object, ctx);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status201Created);

        _mockAccessor.VerifyAll();
    }

    // ---- GET USER ----
    [Fact]
    public async Task GetUser_Should_Return_NotFound_When_NoUser()
    {
        var userId = Guid.NewGuid();
        _mockAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync((UserData?)null);

        var result = await Invoke("GetUserAsync", userId, _mockAccessor.Object, _mockLogger.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _mockAccessor.VerifyAll();
    }

    [Fact]
    public async Task GetUser_Should_Return_Ok_When_UserExists()
    {
        var userId = Guid.NewGuid();
        _mockAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync(MakeUserData(Role.Student, "found@test.com", userId));

        var result = await Invoke("GetUserAsync", userId, _mockAccessor.Object, _mockLogger.Object);

        var ok = Assert.IsType<Ok<UserData>>(result);
        ok.Value.Should().NotBeNull();
        ok.Value!.Email.Should().Be("found@test.com");

        _mockAccessor.VerifyAll();
    }

    // ---- UPDATE USER ----
    [Fact]
    public async Task UpdateUser_Should_Return_NotFound_When_UserMissing()
    {
        var userId = Guid.NewGuid();
        _mockAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync((UserData?)null);

        var update = new UpdateUserModel { PreferredLanguageCode = SupportedLanguage.he };

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
        new ClaimsIdentity([new Claim(AuthSettings.RoleClaimType, "Admin")], "TestAuth"))
        };

        var result = await Invoke("UpdateUserAsync", userId, update, _mockAccessor.Object, _mockLogger.Object, httpContext);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _mockAccessor.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_BadRequest_When_InvalidLanguage()
    {
        var userId = Guid.NewGuid();
        _mockAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync(MakeUserData(Role.Student, "lang@test.com", userId));

        var update = new UpdateUserModel { PreferredLanguageCode = (SupportedLanguage)999 };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
       new ClaimsIdentity([new Claim(AuthSettings.RoleClaimType, "Admin")], "TestAuth"))
        };

        var result = await Invoke("UpdateUserAsync", userId, update, _mockAccessor.Object, _mockLogger.Object, httpContext);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        _mockAccessor.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_BadRequest_When_NonStudentSetsHebrewLevel()
    {
        var userId = Guid.NewGuid();
        _mockAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync(MakeUserData(Role.Teacher, "teach@test.com", userId));

        var update = new UpdateUserModel { HebrewLevelValue = HebrewLevel.fluent };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
       new ClaimsIdentity([new Claim(AuthSettings.RoleClaimType, "Admin")], "TestAuth"))
        };

        var result = await Invoke("UpdateUserAsync", userId, update, _mockAccessor.Object, _mockLogger.Object, httpContext);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        _mockAccessor.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_Ok_When_Valid()
    {
        var userId = Guid.NewGuid();
        _mockAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync(MakeUserData(Role.Student, "update@test.com", userId));
        _mockAccessor.Setup(a => a.UpdateUserAsync(It.IsAny<UpdateUserModel>(), userId)).ReturnsAsync(true);

        var update = new UpdateUserModel { PreferredLanguageCode = SupportedLanguage.he };

        // Build fake HttpContext with an Admin role
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
       new ClaimsIdentity([new Claim(AuthSettings.RoleClaimType, "Admin")], "TestAuth"))
        };

        var result = await Invoke("UpdateUserAsync", userId, update, _mockAccessor.Object, _mockLogger.Object, httpContext);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status200OK);

        _mockAccessor.VerifyAll();
    }

    // ---- DELETE USER ----
    [Fact]
    public async Task DeleteUser_Should_Return_NotFound_When_Missing()
    {
        var userId = Guid.NewGuid();
        _mockAccessor.Setup(a => a.DeleteUserAsync(userId)).ReturnsAsync(false);

        var result = await Invoke("DeleteUserAsync", userId, _mockAccessor.Object, _mockLogger.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _mockAccessor.VerifyAll();
    }

    [Fact]
    public async Task DeleteUser_Should_Return_Ok_When_Deleted()
    {
        var userId = Guid.NewGuid();
        _mockAccessor.Setup(a => a.DeleteUserAsync(userId)).ReturnsAsync(true);

        var result = await Invoke("DeleteUserAsync", userId, _mockAccessor.Object, _mockLogger.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status200OK);

        _mockAccessor.VerifyAll();
    }

    // ---- UTILS ----
    private static UserModel MakeUserModel(Role role, string email, Guid? id = null) => new()
    {
        UserId = id ?? Guid.NewGuid(),
        FirstName = "New",
        LastName = "User",
        Password = "pw",
        Role = role,
        Email = email
    };

    private static UserData MakeUserData(Role role, string email, Guid? id = null) => new()
    {
        UserId = id ?? Guid.NewGuid(),
        FirstName = "Existing",
        LastName = "User",
        Role = role,
        Email = email,
        PreferredLanguageCode = SupportedLanguage.en
    };

    private static CreateUser MakeCreateUser(string email, string role = "student") => new()
    {
        FirstName = "New",
        LastName = "User",
        Password = "pw",
        Email = email,
        Role = role
    };

    private static Task<IResult> Invoke(string methodName, params object[] args) =>
        PrivateInvoker.InvokePrivateEndpointAsync(typeof(UsersEndpoints), methodName, args);

}