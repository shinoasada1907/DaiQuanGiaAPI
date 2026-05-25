using DaiQuanGia.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Identity;

namespace DaiQuanGia.Infrastructure.Authentication;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
