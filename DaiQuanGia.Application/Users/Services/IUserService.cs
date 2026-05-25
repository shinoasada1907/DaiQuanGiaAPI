using DaiQuanGia.Application.Users.Dtos;

namespace DaiQuanGia.Application.Users.Services;

public interface IUserService
{
    Task<UserMeResponse> GetMeAsync(CancellationToken cancellationToken);
}
