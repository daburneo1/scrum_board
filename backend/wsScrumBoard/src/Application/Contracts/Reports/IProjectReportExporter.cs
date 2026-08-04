namespace Application.Contracts.Reports;

public interface IProjectReportExporter
{
    string Format { get; }

    string ContentType { get; }

    string FileExtension { get; }

    byte[] Export(ProjectReportDto report);
}