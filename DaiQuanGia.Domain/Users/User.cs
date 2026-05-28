using Microsoft.AspNetCore.Identity;

namespace DaiQuanGia.Domain.Users;

public sealed class User : IdentityUser<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
    }

    public User(string email, string fullName, string timezone)
    {
        Id = Guid.NewGuid();
        Email = email;
        UserName = email;
        NormalizedEmail = email.ToUpperInvariant();
        NormalizedUserName = email.ToUpperInvariant();
        FullName = fullName;
        Timezone = timezone;
    }

    public string FullName { get; set; } = string.Empty;

    public string Timezone { get; set; } = "Asia/Saigon";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _refreshTokens.Add(refreshToken);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
