namespace DaiQuanGia.Application.Auth.Dtos;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
