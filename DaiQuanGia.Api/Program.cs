using DaiQuanGia.Api.Middlewares;
using DaiQuanGia.Application.Auth;
using DaiQuanGia.Application.Auth.Services;
using DaiQuanGia.Application.Users.Services;
using DaiQuanGia.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(
    builder.Configuration.GetSection("Jwt").Get<AuthOptions>()
        ?? throw new InvalidOperationException("Jwt configuration is missing."));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
