using System.Text;
using Application.Ports.Persistence;
using Application.Ports.Security;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = BuildPostgresConnectionString();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IProjectRepository, ProjectRepository>();

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "El emisor del JWT es obligatorio.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "La audiencia del JWT es obligatoria.")
            .Validate(
                options => options.Key.Length >= 32,
                "La clave JWT debe contener al menos 32 caracteres.")
            .ValidateOnStart();

        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "No se encontró la configuración de JWT.");

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtOptions.Key)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
            });

        services.AddAuthorization();

        return services;
    }

    private static string BuildPostgresConnectionString()
    {
        var portValue = GetRequiredEnvironmentVariable("POSTGRES_PORT");

        if (!int.TryParse(portValue, out var port) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "La variable de entorno POSTGRES_PORT debe contener un puerto TCP válido.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = GetRequiredEnvironmentVariable("POSTGRES_HOST"),
            Port = port,
            Database = GetRequiredEnvironmentVariable("POSTGRES_DB"),
            Username = GetRequiredEnvironmentVariable("POSTGRES_USER"),
            Password = GetRequiredEnvironmentVariable("POSTGRES_PASSWORD"),
            IncludeErrorDetail = true
        }.ConnectionString;
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"La variable de entorno {name} no está configurada.");
    }
}
