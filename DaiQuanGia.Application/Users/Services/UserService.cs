using DaiQuanGia.Application.Abstractions.Authentication;
using DaiQuanGia.Application.Abstractions.Persistence;
using DaiQuanGia.Application.Users.Dtos;
using DaiQuanGia.Shared.Exceptions;

namespace DaiQuanGia.Application.Users.Services;

public sealed class UserService(
    IUserRepository userRepository,
    ICurrentUser currentUser) : IUserService
{
    public async Task<UserMeResponse> GetMeAsync(CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(currentUser.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        return new UserMeResponse(user.Id, user.Email, user.FullName, user.Timezone);
    }
}
