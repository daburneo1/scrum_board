using Application.Services.Authentication;
using Application.Services.Boards;
using Application.Services.Projects;
using Application.Services.Reports;
using Application.Tasks.Ordering;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<LoginService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<BoardService>();
        services.AddScoped<TaskOrderCalculator>();
        services.AddScoped<BoardSubscriptionService>();
        services.AddScoped<ProjectReportService>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
