using Npgsql;

namespace Infrastructure.Persistence;

internal static class PostgresConnectionStringFactory
{
    public static string CreateFromEnvironment()
    {
        return Create(Environment.GetEnvironmentVariable);
    }

    public static string Create(
        Func<string, string?> getVariable)
    {
        var portValue = GetRequiredVariable(
            "POSTGRES_PORT",
            getVariable);

        if (!int.TryParse(portValue, out var port) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "La variable de entorno POSTGRES_PORT debe contener un puerto TCP válido.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = GetRequiredVariable("POSTGRES_HOST", getVariable),
            Port = port,
            Database = GetRequiredVariable("POSTGRES_DB", getVariable),
            Username = GetRequiredVariable("POSTGRES_USER", getVariable),
            Password = GetRequiredVariable("POSTGRES_PASSWORD", getVariable),
            IncludeErrorDetail = true
        }.ConnectionString;
    }

    private static string GetRequiredVariable(
        string name,
        Func<string, string?> getVariable)
    {
        var value = getVariable(name);

        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"La variable de entorno {name} no está configurada.");
    }
}
