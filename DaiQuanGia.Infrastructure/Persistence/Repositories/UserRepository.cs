using DaiQuanGia.Application.Abstractions.Persistence;
using DaiQuanGia.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace DaiQuanGia.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }
}
