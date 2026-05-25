using DaiQuanGia.Domain.Common;

namespace DaiQuanGia.Domain.Users;

public sealed class User : AuditableEntity
{
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
    }

    public User(string email, string passwordHash, string fullName, string timezone)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Timezone = timezone;
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string Timezone { get; private set; } = "Asia/Saigon";

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _refreshTokens.Add(refreshToken);
        Touch();
    }
}
