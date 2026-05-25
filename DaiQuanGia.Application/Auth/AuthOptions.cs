namespace DaiQuanGia.Application.Auth;

public sealed class AuthOptions
{
    public int AccessTokenMinutes { get; set; } = 30;

    public int RefreshTokenDays { get; set; } = 30;
}
