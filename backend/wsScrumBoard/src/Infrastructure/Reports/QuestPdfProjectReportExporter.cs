using System.Globalization;
using Application.Contracts.Reports;
using Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports;

internal sealed class QuestPdfProjectReportExporter
    : IProjectReportExporter
{
    static QuestPdfProjectReportExporter()
    {
        QuestPDF.Settings.License =
            LicenseType.Community;
    }

    public string Format => "pdf";

    public string ContentType =>
        "application/pdf";

    public string FileExtension => "pdf";

    public byte[] Export(
        ProjectReportDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var document = Document.Create(
            container =>
            {
                container.Page(page =>
                {
                    page.Size(
                        PageSizes.A4.Landscape());

                    page.Margin(24);

                    page.PageColor(
                        Colors.White);

                    page.DefaultTextStyle(
                        style =>
                            style.FontSize(9));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text(
                                    $"Reporte del proyecto: " +
                                    $"{report.ProjectName}")
                                .SemiBold()
                                .FontSize(18);

                            column.Item()
                                .PaddingTop(4)
                                .Text(
                                    $"Descripción: " +
                                    $"{report.Description}");

                            column.Item()
                                .PaddingTop(2)
                                .Text(
                                    $"Fecha de inicio: " +
                                    $"{FormatDate(report.StartDate)}");

                            column.Item()
                                .Text(
                                    $"Fecha esperada de finalización: " +
                                    $"{FormatDate(
                                        report.ExpectedEndDate)}");

                            column.Item()
                                .Text(
                                    $"Estado: {FormatStatus(report.Status)}");

                            column.Item()
                                .Text(
                                    "Generado en UTC: " +
                                    report.GeneratedAtUtc.ToString(
                                        "yyyy-MM-dd HH:mm:ss",
                                        CultureInfo.InvariantCulture));
                        });

                    page.Content()
                        .PaddingVertical(12)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(2.2f);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1.4f);
                                    columns.RelativeColumn(1.7f);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1.4f);
                                });

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text("Tarea");

                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text("Descripción");

                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text("Columna");

                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text("Responsable");

                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text("Prioridad");

                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text("Creada en UTC");
                            });

                            if (report.Tasks.Count == 0)
                            {
                                table.Cell()
                                    .ColumnSpan(6)
                                    .Element(BodyCell)
                                    .AlignCenter()
                                    .Text(
                                        "El proyecto no contiene tareas.");

                                return;
                            }

                            foreach (var task in report.Tasks)
                            {
                                table.Cell()
                                    .Element(BodyCell)
                                    .Text(task.Title);

                                table.Cell()
                                    .Element(BodyCell)
                                    .Text(
                                        task.Description);

                                table.Cell()
                                    .Element(BodyCell)
                                    .Text(
                                        task.ColumnName);

                                table.Cell()
                                    .Element(BodyCell)
                                    .Text(
                                        task.ResponsibleName
                                        ?? "Sin asignar");

                                table.Cell()
                                    .Element(BodyCell)
                                    .Text(
                                        FormatPriority(
                                            task.Priority));

                                table.Cell()
                                    .Element(BodyCell)
                                    .Text(
                                        task.CreatedAtUtc
                                            .ToString(
                                                "yyyy-MM-dd HH:mm",
                                                CultureInfo
                                                    .InvariantCulture));
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                });
            });

        return document.GeneratePdf();
    }

    private static string FormatDate(
        DateOnly date)
    {
        return date.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);
    }

    private static string FormatStatus(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Planned => "Planificado",
            ProjectStatus.Active => "Activo",
            ProjectStatus.Completed => "Completado",
            ProjectStatus.Cancelled => "Cancelado",
            _ => status.ToString()
        };
    }

    private static string FormatPriority(WorkItemPriority priority)
    {
        return priority switch
        {
            WorkItemPriority.Low => "Baja",
            WorkItemPriority.Medium => "Media",
            WorkItemPriority.High => "Alta",
            WorkItemPriority.Critical => "Crítica",
            _ => priority.ToString()
        };
    }

    private static IContainer HeaderCell(
        IContainer container)
    {
        return container
            .Background(
                Colors.Grey.Lighten2)
            .BorderBottom(1)
            .BorderColor(
                Colors.Grey.Medium)
            .Padding(5)
            .DefaultTextStyle(
                style => style.SemiBold());
    }

    private static IContainer BodyCell(
        IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(
                Colors.Grey.Lighten2)
            .Padding(5);
    }
}
