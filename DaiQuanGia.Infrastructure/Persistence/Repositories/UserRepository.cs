using DaiQuanGia.Application.Abstractions.Persistence;
using DaiQuanGia.Domain.Users;
using DaiQuanGia.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DaiQuanGia.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(UserManager<User> userManager) : IUserRepository
{
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await userManager.FindByEmailAsync(email) is not null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await userManager.FindByIdAsync(id.ToString());
    }

    public async Task CreateAsync(User user, string password)
    {
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ConflictException(errors);
        }
    }

    // Giữ lại để không break interface — không dùng trực tiếp
    public void Add(User user)
    {
        throw new NotSupportedException("Dùng CreateAsync(user, password) thay thế.");
    }
}
