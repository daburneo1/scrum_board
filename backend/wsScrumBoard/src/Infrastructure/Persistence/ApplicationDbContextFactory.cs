using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory :
    IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environmentFileVariables = ReadEnvironmentFile();

        var connectionString = PostgresConnectionStringFactory.Create(
            name => Environment.GetEnvironmentVariable(name)
                    ?? environmentFileVariables.GetValueOrDefault(name));

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IReadOnlyDictionary<string, string> ReadEnvironmentFile()
    {
        var environmentFilePath = FindEnvironmentFile();

        if (environmentFilePath is null)
        {
            return new Dictionary<string, string>();
        }

        var variables = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in File.ReadLines(environmentFilePath))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            const string exportPrefix = "export ";

            if (line.StartsWith(
                    exportPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                line = line[exportPrefix.Length..].TrimStart();
            }

            var separatorIndex = line.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var name = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') ||
                 (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            variables[name] = value;
        }

        return variables;
    }

    private static string? FindEnvironmentFile()
    {
        var searchRoots = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        foreach (var searchRoot in searchRoots.Distinct())
        {
            var directory = new DirectoryInfo(searchRoot);

            while (directory is not null)
            {
                var candidate = Path.Combine(
                    directory.FullName,
                    ".env");

                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }
}
