namespace DaiQuanGia.Application.Abstractions.Authentication;

public interface IRefreshTokenHasher
{
    string Hash(string token);
}
