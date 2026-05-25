using DaiQuanGia.Application.Users.Dtos;
using DaiQuanGia.Application.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DaiQuanGia.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserMeResponse>> GetMe(CancellationToken cancellationToken)
    {
        return Ok(await userService.GetMeAsync(cancellationToken));
    }
}
