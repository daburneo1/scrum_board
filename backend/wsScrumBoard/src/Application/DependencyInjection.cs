using Application.Services.Authentication;
using Application.Services.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<LoginService>();
        services.AddScoped<ProjectService>();

        return services;
    }
}
