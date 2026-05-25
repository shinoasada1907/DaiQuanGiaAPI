namespace DaiQuanGia.Application.Users.Dtos;

public sealed record UserMeResponse(
    Guid Id,
    string Email,
    string FullName,
    string Timezone
);
