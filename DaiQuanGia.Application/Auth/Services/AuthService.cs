using System.Security.Cryptography;
using DaiQuanGia.Application.Abstractions.Authentication;
using DaiQuanGia.Application.Abstractions.Persistence;
using DaiQuanGia.Application.Auth.Dtos;
using DaiQuanGia.Domain.Users;
using DaiQuanGia.Shared.Exceptions;

namespace DaiQuanGia.Application.Auth.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenHasher refreshTokenHasher,
    AuthOptions authOptions) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException("Email already exists.");
        }

        var user = new User(
            email,
            passwordService.HashPassword(request.Password),
            request.FullName.Trim(),
            NormalizeTimezone(request.Timezone));

        var refreshToken = CreateRefreshToken(user.Id);
        user.AddRefreshToken(refreshToken.Entity);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, refreshToken.PlainToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !passwordService.VerifyPassword(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAppException("Invalid credentials.");
        }

        var refreshToken = CreateRefreshToken(user.Id);
        user.AddRefreshToken(refreshToken.Entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, refreshToken.PlainToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenHasher.Hash(request.RefreshToken);
        var refreshToken = await refreshTokenRepository.GetByHashWithUserAsync(tokenHash, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (refreshToken is null || !refreshToken.IsActive(now))
        {
            throw new UnauthorizedAppException("Invalid refresh token.");
        }

        refreshToken.Revoke(now);

        var newRefreshToken = CreateRefreshToken(refreshToken.UserId);
        refreshTokenRepository.Add(newRefreshToken.Entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(refreshToken.User, newRefreshToken.PlainToken);
    }

    public async Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenHasher.Hash(request.RefreshToken);
        var refreshToken = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (refreshToken is null)
        {
            return;
        }

        refreshToken.Revoke(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private AuthResponse CreateAuthResponse(User user, string refreshToken)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(authOptions.AccessTokenMinutes);
        var accessToken = jwtTokenService.CreateAccessToken(user.Id, user.Email, user.FullName, expiresAt);

        return new AuthResponse(accessToken, refreshToken, expiresAt);
    }

    private RefreshTokenResult CreateRefreshToken(Guid userId)
    {
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = refreshTokenHasher.Hash(plainToken);
        var expiresAt = DateTimeOffset.UtcNow.AddDays(authOptions.RefreshTokenDays);

        return new RefreshTokenResult(plainToken, new RefreshToken(userId, tokenHash, expiresAt));
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeTimezone(string? timezone)
    {
        return string.IsNullOrWhiteSpace(timezone) ? "Asia/Saigon" : timezone.Trim();
    }

    private sealed record RefreshTokenResult(string PlainToken, RefreshToken Entity);
}
