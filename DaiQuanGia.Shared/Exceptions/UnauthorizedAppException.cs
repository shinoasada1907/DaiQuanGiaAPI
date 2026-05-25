namespace DaiQuanGia.Shared.Exceptions;

public sealed class UnauthorizedAppException(string message) : AppException(message)
{
    public override int StatusCode => 401;
}
