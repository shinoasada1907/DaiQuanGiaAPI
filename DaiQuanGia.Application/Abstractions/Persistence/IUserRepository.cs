using DaiQuanGia.Domain.Users;

namespace DaiQuanGia.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Tạo user mới và hash password thông qua UserManager.
    /// Tự động commit — không cần gọi IUnitOfWork.SaveChangesAsync sau đó.
    /// </summary>
    Task CreateAsync(User user, string password);

    /// <summary>
    /// Không dùng trực tiếp — dùng CreateAsync(user, password) thay thế.
    /// </summary>
    void Add(User user);
}
