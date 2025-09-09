using System;
using System.Threading.Tasks;
using Accessor.Endpoints;
using Accessor.Models.Users;
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
    private readonly Mock<IUserManagementService> _mockService = new(MockBehavior.Strict);
    private readonly Mock<ILogger<UserManagementService>> _mockLogger = new();

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

        var ok = result.Should().BeOfType<Ok<UserData>>().Subject;
        ok.Value.Should().Be(user);
        _mockService.VerifyAll();
    }

    // ---- CREATE ----
    [Fact]
    public async Task CreateUser_Should_Return_Conflict_When_Duplicate()
    {
        var user = BuildUser("dup@test.com");
        _mockService.Setup(s => s.CreateUserAsync(user)).ReturnsAsync(false);

        var result = await Invoke("CreateUserAsync", user, _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<Conflict<string>>();
        _mockService.VerifyAll();
    }

    [Fact]
    public async Task CreateUser_Should_Return_Created_When_Success()
    {
        var user = BuildUser("ok@test.com");
        _mockService.Setup(s => s.CreateUserAsync(user)).ReturnsAsync(true);

        var result = await Invoke("CreateUserAsync", user, _mockService.Object, _mockLogger.Object);

        var created = result.Should().BeOfType<Created<UserModel>>().Subject;
        created.Value.Should().BeEquivalentTo(user);
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

        var result = await Invoke("UpdateUserAsync", id, new UpdateUserModel(), _mockService.Object, _mockLogger.Object);

        result.Should().BeOfType<NotFound<string>>();
        _mockService.VerifyAll();
    }

    [Fact]
    public async Task UpdateUser_Should_Return_Ok_When_Success()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.UpdateUserAsync(It.IsAny<UpdateUserModel>(), id)).ReturnsAsync(true);

        var result = await Invoke("UpdateUserAsync", id, new UpdateUserModel(), _mockService.Object, _mockLogger.Object);

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

        return await (Task<IResult>)method.Invoke(null, args)!;
    }

    private static UserModel BuildUser(string email, Guid? id = null) => new()
    {
        UserId = id ?? Guid.NewGuid(),
        Email = email,
        FirstName = "Test",
        LastName = "User",
        Password = "Pass123!",
        Role = Role.Student
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