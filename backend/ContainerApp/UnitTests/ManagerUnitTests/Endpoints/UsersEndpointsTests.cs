using FluentAssertions;
using Manager.Endpoints;
using Manager.Models.Users;
using Manager.Services.Clients.Accessor.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Manager.Constants;
using Manager.Services.Clients.Accessor.Interfaces;

namespace ManagerUnitTests.Endpoints;

public class UsersEndpointsTests
{
    private readonly Mock<IUsersAccessorClient> _mockUsersAccessor = new(MockBehavior.Strict);
    private readonly Mock<ILogger<object>> _mockLogger = new();

    // ---- CREATE USER ----
    [Fact]
    public async Task CreateUser_Should_Return_BadRequest_When_InvalidRole()
    {
        var newUser = MakeCreateUserRequest("fail@test.com", role: "alien");
        var ctx = new DefaultHttpContext();

        var result = await Invoke("CreateUserAsync", newUser, _mockUsersAccessor.Object, _mockLogger.Object, ctx);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreateUser_Should_Return_Conflict_When_Duplicate()
    {
        var newUser = MakeCreateUserRequest("dupe@test.com");
        var ctx = new DefaultHttpContext();

        _mockUsersAccessor.Setup(a => a.CreateUserAsync(It.IsAny<CreateUserAccessorRequest>())).ReturnsAsync(false);

        var result = await Invoke("CreateUserAsync", newUser, _mockUsersAccessor.Object, _mockLogger.Object, ctx);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        _mockUsersAccessor.VerifyAll();
    }

    [Fact]
    public async Task CreateUser_Should_Return_Created_When_Valid()
    {
        var newUser = MakeCreateUserRequest("ok@test.com");
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Accept-Language"] = "he-IL";

        _mockUsersAccessor.Setup(a => a.CreateUserAsync(It.IsAny<CreateUserAccessorRequest>())).ReturnsAsync(true);

        var result = await Invoke("CreateUserAsync", newUser, _mockUsersAccessor.Object, _mockLogger.Object, ctx);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status201Created);

        _mockUsersAccessor.VerifyAll();
    }

    // ---- GET USER ----
    [Fact]
    public async Task GetUser_Should_Return_NotFound_When_NoUser()
    {
        var userId = Guid.NewGuid();
        _mockUsersAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync((GetUserAccessorResponse?)null);

        var result = await Invoke("GetUserAsync", userId, _mockUsersAccessor.Object, _mockLogger.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _mockUsersAccessor.VerifyAll();
    }

    [Fact]
    public async Task GetUser_Should_Return_Ok_When_UserExists()
    {
        var userId = Guid.NewGuid();
        _mockUsersAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync(MakeGetUserAccessorResponse(Role.Student, "found@test.com", userId));

        var result = await Invoke("GetUserAsync", userId, _mockUsersAccessor.Object, _mockLogger.Object);

        var ok = Assert.IsType<Ok<GetUserResponse>>(result);
        ok.Value.Should().NotBeNull();
        ok.Value!.Email.Should().Be("found@test.com");

        _mockUsersAccessor.VerifyAll();
    }

    // ---- UPDATE USER ----
    [Fact]
    public async Task UpdateUser_Should_Return_NotFound_When_UserMissing()
    {
        var userId = Guid.NewGuid();
        _mockUsersAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync((GetUserAccessorResponse?)null);

        var update = new UpdateUserRequest { PreferredLanguageCode = SupportedLanguage.he };

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
        new ClaimsIdentity([new Claim(AuthSettings.RoleClaimType, "Admin")], "TestAuth"))
        };

        var result = await Invoke("UpdateUserAsync", userId, update, _mockUsersAccessor.Object, _mockLogger.Object, httpContext);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _mockUsersAccessor.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_BadRequest_When_InvalidLanguage()
    {
        var userId = Guid.NewGuid();
        _mockUsersAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync(MakeGetUserAccessorResponse(Role.Student, "lang@test.com", userId));

        var update = new UpdateUserRequest { PreferredLanguageCode = (SupportedLanguage)999 };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
       new ClaimsIdentity([new Claim(AuthSettings.RoleClaimType, "Admin")], "TestAuth"))
        };

        var result = await Invoke("UpdateUserAsync", userId, update, _mockUsersAccessor.Object, _mockLogger.Object, httpContext);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        _mockUsersAccessor.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_BadRequest_When_NonStudentSetsHebrewLevel()
    {
        var userId = Guid.NewGuid();
        _mockUsersAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync(MakeGetUserAccessorResponse(Role.Teacher, "teach@test.com", userId));

        var update = new UpdateUserRequest { HebrewLevelValue = HebrewLevel.fluent };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
       new ClaimsIdentity([new Claim(AuthSettings.RoleClaimType, "Admin")], "TestAuth"))
        };

        var result = await Invoke("UpdateUserAsync", userId, update, _mockUsersAccessor.Object, _mockLogger.Object, httpContext);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        _mockUsersAccessor.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_Ok_When_Valid()
    {
        var userId = Guid.NewGuid();
        _mockUsersAccessor.Setup(a => a.GetUserAsync(userId)).ReturnsAsync(MakeGetUserAccessorResponse(Role.Student, "update@test.com", userId));
        _mockUsersAccessor.Setup(a => a.UpdateUserAsync(It.IsAny<UpdateUserAccessorRequest>(), userId)).ReturnsAsync(true);

        var update = new UpdateUserRequest { PreferredLanguageCode = SupportedLanguage.he };

        // Build fake HttpContext with an Admin role
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
       new ClaimsIdentity([new Claim(AuthSettings.RoleClaimType, "Admin")], "TestAuth"))
        };

        var result = await Invoke("UpdateUserAsync", userId, update, _mockUsersAccessor.Object, _mockLogger.Object, httpContext);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status200OK);

        _mockUsersAccessor.VerifyAll();
    }

    // ---- DELETE USER ----
    [Fact]
    public async Task DeleteUser_Should_Return_NotFound_When_Missing()
    {
        var userId = Guid.NewGuid();
        _mockUsersAccessor.Setup(a => a.DeleteUserAsync(userId)).ReturnsAsync(false);

        var result = await Invoke("DeleteUserAsync", userId, _mockUsersAccessor.Object, _mockLogger.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _mockUsersAccessor.VerifyAll();
    }

    [Fact]
    public async Task DeleteUser_Should_Return_Ok_When_Deleted()
    {
        var userId = Guid.NewGuid();
        _mockUsersAccessor.Setup(a => a.DeleteUserAsync(userId)).ReturnsAsync(true);

        var result = await Invoke("DeleteUserAsync", userId, _mockUsersAccessor.Object, _mockLogger.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        status.StatusCode.Should().Be(StatusCodes.Status200OK);

        _mockUsersAccessor.VerifyAll();
    }

    // ---- UTILS ----
    private static GetUserAccessorResponse MakeGetUserAccessorResponse(Role role, string email, Guid? id = null) => new()
    {
        UserId = id ?? Guid.NewGuid(),
        FirstName = "Existing",
        LastName = "User",
        Role = role,
        Email = email,
        PreferredLanguageCode = SupportedLanguage.en
    };

    private static CreateUserRequest MakeCreateUserRequest(string email, string role = "student") => new()
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