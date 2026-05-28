using DaiQuanGia.Application.Abstractions.Authentication;
using DaiQuanGia.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace DaiQuanGia.Infrastructure.Authentication;

public sealed class PasswordService(IPasswordHasher<User> passwordHasher) : IPasswordService
{
    public string HashPassword(string password)
    {
        return passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(null!, passwordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
