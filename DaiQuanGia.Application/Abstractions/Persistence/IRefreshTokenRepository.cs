using DaiQuanGia.Domain.Users;

namespace DaiQuanGia.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    void Add(RefreshToken refreshToken);
}
