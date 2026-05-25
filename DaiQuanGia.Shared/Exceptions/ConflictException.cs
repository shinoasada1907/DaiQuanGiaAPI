namespace DaiQuanGia.Shared.Exceptions;

public sealed class ConflictException(string message) : AppException(message)
{
    public override int StatusCode => 409;
}
