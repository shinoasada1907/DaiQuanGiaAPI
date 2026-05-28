using DaiQuanGia.Application.Auth;
using DaiQuanGia.Application.Auth.Services;
using DaiQuanGia.Application.Users.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DaiQuanGia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(
            configuration.GetSection("Jwt").Get<AuthOptions>()
                ?? throw new InvalidOperationException("Jwt configuration is missing."));

        // Auth module
        services.AddScoped<IAuthService, AuthService>();

        // Users module
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
