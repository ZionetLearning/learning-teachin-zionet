using System;
using System.Threading.Tasks;
using Accessor.Endpoints;
using Accessor.Models.Users;
using Accessor.Models.Users.Requests;
using Accessor.Models.Users.Responses;
using Accessor.Services;
using Accessor.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AccessorUnitTests.Endpoints;

public class UsersEndpointsTests
{
    private readonly Mock<IUserService> _mockService = new(MockBehavior.Strict);
    private readonly Mock<ILogger<UserService>> _mockLogger = new();

    // ---- GET ----
    [Fact]
    public async Task GetUser_Should_Return_BadRequest_When_EmptyId()
    {
        var result = await Invoke("GetUserAsync", Guid.Empty, _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<BadRequest<string>>();
        _mockService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetUser_Should_Return_NotFound_When_Missing()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetUserAsync(id)).ReturnsAsync((UserData?)null);

        var result = await Invoke("GetUserAsync", id, _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<NotFound>();
        _mockService.VerifyAll();
    }

    [Fact]
    public async Task GetUser_Should_Return_Ok_When_Exists()
    {
        var id = Guid.NewGuid();
        var user = BuildUserData("ok@test.com", id);
        _mockService.Setup(s => s.GetUserAsync(id)).ReturnsAsync(user);

        var result = await Invoke("GetUserAsync", id, _mockService.Object, _mockLogger.Object);

        var ok = result.Should().BeOfType<Ok<GetUserResponse>>().Subject;
        ok.Value.Should().NotBeNull();
        ok.Value!.UserId.Should().Be(user.UserId);
        ok.Value!.Email.Should().Be(user.Email);
        _mockService.VerifyAll();
    }

    // ---- CREATE ----
    [Fact]
    public async Task CreateUser_Should_Return_Conflict_When_Duplicate()
    {
        var request = BuildCreateUserRequest("dup@test.com");
        _mockService.Setup(s => s.CreateUserAsync(It.IsAny<UserModel>())).ReturnsAsync(false);

        var result = await Invoke("CreateUserAsync", request, _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<Conflict<string>>();
        _mockService.VerifyAll();
    }

    [Fact]
    public async Task CreateUser_Should_Return_Created_When_Success()
    {
        var request = BuildCreateUserRequest("ok@test.com");
        _mockService.Setup(s => s.CreateUserAsync(It.IsAny<UserModel>())).ReturnsAsync(true);

        var result = await Invoke("CreateUserAsync", request, _mockService.Object, _mockLogger.Object);

        var created = result.Should().BeOfType<Created<CreateUserResponse>>().Subject;
        created.Value.Should().NotBeNull();
        created.Value!.Email.Should().Be(request.Email);
        created.Value!.UserId.Should().Be(request.UserId);
        _mockService.VerifyAll();
    }

    // ---- UPDATE ----
    [Fact]
    public async Task UpdateUser_Should_Return_BadRequest_When_Null()
    {
        var id = Guid.NewGuid();

        var result = await Invoke("UpdateUserAsync", id, null!, _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<BadRequest<string>>();
        _mockService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_NotFound_When_Missing()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.UpdateUserAsync(It.IsAny<UpdateUserModel>(), id)).ReturnsAsync(false);

        var result = await Invoke("UpdateUserAsync", id, new UpdateUserRequest(), _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<NotFound<string>>();
        _mockService.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_Ok_When_Success()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.UpdateUserAsync(It.IsAny<UpdateUserModel>(), id)).ReturnsAsync(true);

        var result = await Invoke("UpdateUserAsync", id, new UpdateUserRequest(), _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<Ok<string>>();
        _mockService.VerifyAll();
    }

    // ---- DELETE ----
    [Fact]
    public async Task DeleteUser_Should_Return_BadRequest_When_EmptyId()
    {
        var result = await Invoke("DeleteUserAsync", Guid.Empty, _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<BadRequest<string>>();
        _mockService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteUser_Should_Return_NotFound_When_Missing()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(false);

        var result = await Invoke("DeleteUserAsync", id, _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<NotFound<string>>();
        _mockService.VerifyAll();
    }

    [Fact]
    public async Task DeleteUser_Should_Return_Ok_When_Success()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(true);

        var result = await Invoke("DeleteUserAsync", id, _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<Ok<string>>();
        _mockService.VerifyAll();
    }

    // ---- UTILS ----
    private static async Task<IResult> Invoke(string methodName, params object[] args)
    {
        var method = typeof(UsersEndpoints).GetMethod(
            methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
            throw new InvalidOperationException($"Method {methodName} not found on UsersEndpoints.");

        var result = method.Invoke(null, args);
        if (result == null)
            throw new InvalidOperationException($"Method {methodName} returned null.");

        return await (Task<IResult>)result;
    }

    private static CreateUserRequest BuildCreateUserRequest(string email, Guid? id = null) => new()
    {
        UserId = id ?? Guid.NewGuid(),
        Email = email,
        FirstName = "Test",
        LastName = "User",
        Password = "Pass123!",
        Role = Role.Student,
        PreferredLanguageCode = SupportedLanguage.en,
        Interests = []
    };

    private static UserData BuildUserData(string email, Guid? id = null) => new()
    {
        UserId = id ?? Guid.NewGuid(),
        Email = email,
        FirstName = "Test",
        LastName = "User",
        Role = Role.Student,
        PreferredLanguageCode = SupportedLanguage.en
    };
}