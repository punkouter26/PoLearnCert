using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Po.LearnCert.Api.Features.Authentication.Infrastructure;
using Po.LearnCert.Api.Features.Authentication.Services;
using Po.LearnCert.Shared.Contracts;
using Xunit;

namespace Po.LearnCert.UnitTests.Features.Authentication;

public class AuthenticationServiceTests
{
    private readonly Mock<UserManager<UserEntity>> _mockUserManager;
    private readonly Mock<SignInManager<UserEntity>> _mockSignInManager;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        // Setup UserManager mock
        var userStore = new Mock<IUserStore<UserEntity>>();
        _mockUserManager = new Mock<UserManager<UserEntity>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup SignInManager mock
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<UserEntity>>();
        _mockSignInManager = new Mock<SignInManager<UserEntity>>(
            _mockUserManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null!, null!, null!, null!);

        _mockLogger = new Mock<ILogger<AuthenticationService>>();
        _sut = new AuthenticationService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidCredentials_ShouldCreateUserAndReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("testuser", result.Username);
        _mockUserManager.Verify(x => x.CreateAsync(
            It.Is<UserEntity>(u => u.UserName == "testuser" && u.Email == "test@example.com"),
            "Password123!"), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordMismatch_ShouldReturnFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Passwords do not match", result.Errors);
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithWeakPassword_ShouldReturnIdentityErrors()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "weak",
            ConfirmPassword = "weak"
        };

        var identityErrors = new[]
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Password must be at least 6 characters" }
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Password must be at least 6 characters", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldSignInAndReturnSuccess()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var user = new UserEntity { UserName = "testuser", Email = "test@example.com", Id = "user-id-123" };

        _mockUserManager
            .Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(user);

        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(user, "Password123!", false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("testuser", result.Username);
        _mockSignInManager.Verify(x => x.PasswordSignInAsync(user, "Password123!", false, false), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "Password123!"
        };

        _mockUserManager
            .Setup(x => x.FindByNameAsync("nonexistent"))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid username or password", result.Errors);
        _mockSignInManager.Verify(x => x.PasswordSignInAsync(
            It.IsAny<UserEntity>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword123!"
        };

        var user = new UserEntity { UserName = "testuser", Email = "test@example.com", Id = "user-id-123" };

        _mockUserManager
            .Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(user);

        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(user, "WrongPassword123!", false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid username or password", result.Errors);
    }

    [Fact]
    public async Task LogoutAsync_ShouldSignOutUser()
    {
        // Arrange
        _mockSignInManager
            .Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogoutAsync();

        // Assert
        _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
    }
}
