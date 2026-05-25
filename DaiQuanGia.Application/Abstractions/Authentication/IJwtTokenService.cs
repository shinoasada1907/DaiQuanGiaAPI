namespace DaiQuanGia.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    string CreateAccessToken(Guid userId, string email, string fullName, DateTimeOffset expiresAt);
}
