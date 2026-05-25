namespace DaiQuanGia.Shared.Exceptions;

public sealed class NotFoundException(string message) : AppException(message)
{
    public override int StatusCode => 404;
}
