using System.Security.Claims;
using DaiQuanGia.Application.Abstractions.Authentication;
using DaiQuanGia.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace DaiQuanGia.Infrastructure.Authentication;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

            if (Guid.TryParse(value, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAppException("User is not authenticated.");
        }
    }
}
