using System.Text.RegularExpressions;
using Application.Common.Exceptions;
using Application.Contracts.Reports;
using Application.Ports.Reports;

namespace Application.Services.Reports;

public sealed class ProjectReportService
{
    private readonly IProjectReportRepository
        _reportRepository;

    private readonly IReadOnlyDictionary<
        string,
        IProjectReportExporter> _exporters;

    private readonly TimeProvider _timeProvider;

    public ProjectReportService(
        IProjectReportRepository reportRepository,
        IEnumerable<IProjectReportExporter> exporters,
        TimeProvider timeProvider)
    {
        _reportRepository = reportRepository;
        _timeProvider = timeProvider;

        var exporterList = exporters.ToList();

        if (exporterList.Count == 0)
        {
            throw new InvalidOperationException(
                "Debe haber al menos un exportador de informes de proyectos registrado.");
        }

        var duplicatedFormat = exporterList
            .GroupBy(
                exporter => NormalizeFormat(
                    exporter.Format),
                StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicatedFormat is not null)
        {
            throw new InvalidOperationException(
                $"Más de un exportador estaba registrado para " +
                $"el formato '{duplicatedFormat.Key}'.");
        }

        _exporters = exporterList.ToDictionary(
            exporter => NormalizeFormat(
                exporter.Format),
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task<GeneratedReportFile> GenerateAsync(
        Guid projectId,
        string format,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "Se requiere un id de proyecto válido.");
        }

        var normalizedFormat =
            NormalizeFormat(format);

        if (!_exporters.TryGetValue(
            normalizedFormat,
            out var exporter))
        {
            throw new ValidationException(
                $"El formato '{format}' no está soportado.");
        }

        var generatedAtUtc =
            _timeProvider.GetUtcNow();

        /*
         * Esta es la única consulta a PostgreSQL
         * realizada durante la generación.
         */
        var report =
            await _reportRepository.GetAsync(
                projectId,
                generatedAtUtc,
                cancellationToken)
            ?? throw new NotFoundException(
                "No se encontró el proyecto.");

        var content = exporter.Export(report);

        if (content.Length == 0)
        {
            throw new InvalidOperationException(
                "El proyectó retornó un archivo vacio.");
        }

        var fileName = BuildFileName(
            report.ProjectName,
            generatedAtUtc,
            exporter.FileExtension);

        return new GeneratedReportFile(
            content,
            exporter.ContentType,
            fileName);
    }

    private static string NormalizeFormat(
        string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return string.Empty;
        }

        return format
            .Trim()
            .TrimStart('.')
            .ToLowerInvariant();
    }

    private static string BuildFileName(
        string projectName,
        DateTimeOffset generatedAtUtc,
        string extension)
    {
        var normalizedName = Regex.Replace(
                projectName.Trim(),
                @"[^\p{L}\p{Nd}-]+",
                "-")
            .Trim('-');

        if (string.IsNullOrWhiteSpace(
            normalizedName))
        {
            normalizedName = "project";
        }

        if (normalizedName.Length > 60)
        {
            normalizedName =
                normalizedName[..60]
                    .TrimEnd('-');
        }

        var normalizedExtension =
            extension.Trim().TrimStart('.');

        return
            $"{normalizedName}-report-" +
            $"{generatedAtUtc:yyyyMMdd-HHmmss}." +
            $"{normalizedExtension}";
    }
}