using System.Security.Cryptography;
using System.Text;
using DaiQuanGia.Application.Abstractions.Authentication;

namespace DaiQuanGia.Infrastructure.Authentication;

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
