namespace Application.Contracts.Reports;

public sealed record GeneratedReportFile(
    byte[] Content,
    string ContentType,
    string FileName);