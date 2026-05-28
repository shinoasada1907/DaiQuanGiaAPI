using DaiQuanGia.Application.Abstractions.Authentication;
using DaiQuanGia.Application.Abstractions.Persistence;
using DaiQuanGia.Application.Auth;
using DaiQuanGia.Application.Auth.Dtos;
using DaiQuanGia.Application.Auth.Services;
using DaiQuanGia.Domain.Users;
using DaiQuanGia.Shared.Exceptions;

namespace DaiQuanGia.UnitTests.Application.Auth;

public sealed class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IRefreshTokenHasher _refreshTokenHasher = Substitute.For<IRefreshTokenHasher>();
    private readonly AuthOptions _authOptions = new() { AccessTokenMinutes = 30, RefreshTokenDays = 30 };

    private AuthService CreateSut() => new(
        _userRepository,
        _refreshTokenRepository,
        _unitOfWork,
        _passwordService,
        _jwtTokenService,
        _refreshTokenHasher,
        _authOptions);

    // ===========================================================================
    // RegisterAsync
    // ===========================================================================

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "Password123", "Test User", null);
        _userRepository.ExistsByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => CreateSut().RegisterAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email already exists.");

        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    [Fact]
    public async Task RegisterAsync_WhenValid_NormalizesEmailAndTimezoneAndReturnsAuthResponse()
    {
        // Arrange — email viết hoa + có khoảng trắng, không truyền timezone
        var request = new RegisterRequest("  Test@Example.COM  ", "Password123", "  Test User  ", null);
        _userRepository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _refreshTokenHasher.Hash(Arg.Any<string>()).Returns("hashed-token");
        _jwtTokenService.CreateAccessToken(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>())
            .Returns("access-jwt");

        // Act
        var result = await CreateSut().RegisterAsync(request, CancellationToken.None);

        // Assert — email lowercased + trimmed
        await _userRepository.Received(1).ExistsByEmailAsync("test@example.com", Arg.Any<CancellationToken>());

        // CreateAsync nhận user với email đã normalize và full_name đã trim
        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.Email == "test@example.com"
                              && u.FullName == "Test User"
                              && u.Timezone == "Asia/Saigon"),
            "Password123");

        // RefreshToken được add
        _refreshTokenRepository.Received(1).Add(Arg.Any<RefreshToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        result.AccessToken.Should().Be("access-jwt");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterAsync_WhenTimezoneProvided_UsesProvidedTimezone()
    {
        var request = new RegisterRequest("a@b.com", "Password123", "X", "Europe/London");
        _userRepository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _refreshTokenHasher.Hash(Arg.Any<string>()).Returns("h");

        await CreateSut().RegisterAsync(request, CancellationToken.None);

        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.Timezone == "Europe/London"),
            Arg.Any<string>());
    }

    // ===========================================================================
    // LoginAsync
    // ===========================================================================

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorized()
    {
        var request = new LoginRequest("missing@example.com", "Password123");
        _userRepository.GetByEmailAsync("missing@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => CreateSut().LoginAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordWrong_ThrowsUnauthorized()
    {
        var user = new User("test@example.com", "Test", "Asia/Saigon");
        user.PasswordHash = "stored-hash";
        var request = new LoginRequest("test@example.com", "WrongPassword");

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.VerifyPassword("stored-hash", "WrongPassword").Returns(false);

        var act = () => CreateSut().LoginAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>()
            .WithMessage("Invalid credentials.");

        _refreshTokenRepository.DidNotReceive().Add(Arg.Any<RefreshToken>());
    }

    [Fact]
    public async Task LoginAsync_WhenValid_IssuesAccessAndRefreshTokens()
    {
        var user = new User("test@example.com", "Test", "Asia/Saigon");
        user.PasswordHash = "stored-hash";
        var request = new LoginRequest("Test@Example.com", "Password123");

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.VerifyPassword("stored-hash", "Password123").Returns(true);
        _refreshTokenHasher.Hash(Arg.Any<string>()).Returns("hashed-refresh");
        _jwtTokenService.CreateAccessToken(
            user.Id, user.Email!, user.FullName, Arg.Any<DateTimeOffset>())
            .Returns("jwt-access");

        var result = await CreateSut().LoginAsync(request, CancellationToken.None);

        result.AccessToken.Should().Be("jwt-access");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _refreshTokenRepository.Received(1).Add(Arg.Is<RefreshToken>(
            t => t.UserId == user.Id && t.TokenHash == "hashed-refresh"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ===========================================================================
    // RefreshAsync
    // ===========================================================================

    [Fact]
    public async Task RefreshAsync_WhenTokenNotFound_ThrowsUnauthorized()
    {
        var request = new RefreshTokenRequest("raw-token");
        _refreshTokenHasher.Hash("raw-token").Returns("hash");
        _refreshTokenRepository.GetByHashWithUserAsync("hash", Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var act = () => CreateSut().RefreshAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>()
            .WithMessage("Invalid refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenRevoked_ThrowsUnauthorized()
    {
        var user = new User("a@b.com", "A", "Asia/Saigon");
        var token = new RefreshToken(user.Id, "hash", DateTimeOffset.UtcNow.AddDays(10));
        token.Revoke(DateTimeOffset.UtcNow.AddMinutes(-5));

        _refreshTokenHasher.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByHashWithUserAsync("hash", Arg.Any<CancellationToken>()).Returns(token);

        var act = () => CreateSut().RefreshAsync(new RefreshTokenRequest("raw"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenExpired_ThrowsUnauthorized()
    {
        var user = new User("a@b.com", "A", "Asia/Saigon");
        var token = new RefreshToken(user.Id, "hash", DateTimeOffset.UtcNow.AddDays(-1));

        _refreshTokenHasher.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByHashWithUserAsync("hash", Arg.Any<CancellationToken>()).Returns(token);

        var act = () => CreateSut().RefreshAsync(new RefreshTokenRequest("raw"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task RefreshAsync_WhenValid_RevokesOldAndIssuesNewToken()
    {
        var user = new User("a@b.com", "A", "Asia/Saigon");
        var oldToken = new RefreshToken(user.Id, "old-hash", DateTimeOffset.UtcNow.AddDays(10));
        // Set User navigation property via reflection vì RefreshToken.User là private set
        typeof(RefreshToken).GetProperty(nameof(RefreshToken.User))!.SetValue(oldToken, user);

        _refreshTokenHasher.Hash("raw").Returns("old-hash");
        _refreshTokenHasher.Hash(Arg.Is<string>(s => s != "raw")).Returns("new-hash");
        _refreshTokenRepository.GetByHashWithUserAsync("old-hash", Arg.Any<CancellationToken>()).Returns(oldToken);
        _jwtTokenService.CreateAccessToken(
            user.Id, user.Email!, user.FullName, Arg.Any<DateTimeOffset>())
            .Returns("new-jwt");

        var result = await CreateSut().RefreshAsync(new RefreshTokenRequest("raw"), CancellationToken.None);

        oldToken.RevokedAt.Should().NotBeNull();
        result.AccessToken.Should().Be("new-jwt");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _refreshTokenRepository.Received(1).Add(Arg.Is<RefreshToken>(
            t => t.UserId == user.Id && t.TokenHash == "new-hash"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ===========================================================================
    // LogoutAsync
    // ===========================================================================

    [Fact]
    public async Task LogoutAsync_WhenTokenNotFound_DoesNothing()
    {
        _refreshTokenHasher.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByHashAsync("hash", Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        await CreateSut().LogoutAsync(new RefreshTokenRequest("raw"), CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_WhenTokenExists_RevokesAndSaves()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddDays(10));
        _refreshTokenHasher.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(token);

        await CreateSut().LogoutAsync(new RefreshTokenRequest("raw"), CancellationToken.None);

        token.RevokedAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_WhenTokenAlreadyRevoked_StillCallsSave()
    {
        // Document hành vi hiện tại: code không kiểm tra IsActive trước khi revoke + save lại
        var token = new RefreshToken(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddDays(10));
        token.Revoke(DateTimeOffset.UtcNow.AddDays(-1));
        _refreshTokenHasher.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(token);

        await CreateSut().LogoutAsync(new RefreshTokenRequest("raw"), CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
