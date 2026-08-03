using System.Drawing;
using Application.Contracts.Reports;
using Domain.Enums;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Infrastructure.Reports;

internal sealed class EpPlusProjectReportExporter :
    IProjectReportExporter
{
    private const int TaskHeaderRow = 10;
    private const int ColumnCount = 6;

    static EpPlusProjectReportExporter()
    {
        ExcelPackage.License.SetNonCommercialOrganization(
            "ScrumBoard");
    }

    public string Format => "xlsx";

    public string ContentType =>
        "application/vnd.openxmlformats-" +
        "officedocument.spreadsheetml.sheet";

    public string FileExtension => "xlsx";

    public byte[] Export(ProjectReportDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        using var package = new ExcelPackage();

        var worksheet = package.Workbook.Worksheets.Add(
            "Reporte del proyecto");

        ConfigureDocumentProperties(package, report);
        AddProjectHeader(worksheet, report);
        AddTaskTable(worksheet, report);
        ConfigureWorksheet(worksheet);

        return package.GetAsByteArray();
    }

    private static void ConfigureDocumentProperties(
        ExcelPackage package,
        ProjectReportDto report)
    {
        package.Workbook.Properties.Title =
            $"Reporte del proyecto - {report.ProjectName}";

        package.Workbook.Properties.Subject =
            "Reporte de tareas del proyecto";

        package.Workbook.Properties.Author = "ScrumBoard";
    }

    private static void AddProjectHeader(
        ExcelWorksheet worksheet,
        ProjectReportDto report)
    {
        var titleRange = worksheet.Cells[1, 1, 1, ColumnCount];

        titleRange.Merge = true;
        titleRange.Value = $"Reporte del proyecto: {report.ProjectName}";
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.Size = 16;
        titleRange.Style.HorizontalAlignment =
            ExcelHorizontalAlignment.Center;

        AddMetadataRow(
            worksheet,
            3,
            "Proyecto",
            report.ProjectName);

        AddMetadataRow(
            worksheet,
            4,
            "Descripción",
            report.Description);

        AddMetadataRow(
            worksheet,
            5,
            "Fecha de inicio",
            report.StartDate.ToString("yyyy-MM-dd"));

        AddMetadataRow(
            worksheet,
            6,
            "Fecha esperada de finalización",
            report.ExpectedEndDate.ToString("yyyy-MM-dd"));

        AddMetadataRow(
            worksheet,
            7,
            "Estado",
            FormatStatus(report.Status));

        AddMetadataRow(
            worksheet,
            8,
            "Generado en UTC",
            report.GeneratedAtUtc.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    private static void AddMetadataRow(
        ExcelWorksheet worksheet,
        int row,
        string label,
        string value)
    {
        var labelCell = worksheet.Cells[row, 1];

        labelCell.Value = label;
        labelCell.Style.Font.Bold = true;

        var valueRange = worksheet.Cells[row, 2, row, ColumnCount];

        valueRange.Merge = true;
        valueRange.Value = value;
        valueRange.Style.WrapText = true;
    }

    private static void AddTaskTable(
        ExcelWorksheet worksheet,
        ProjectReportDto report)
    {
        var headers = new[]
        {
            "Tarea",
            "Descripción",
            "Columna",
            "Responsable",
            "Prioridad",
            "Creada en UTC"
        };

        for (var index = 0; index < headers.Length; index++)
        {
            worksheet.Cells[TaskHeaderRow, index + 1].Value =
                headers[index];
        }

        var headerRange = worksheet.Cells[
            TaskHeaderRow,
            1,
            TaskHeaderRow,
            headers.Length];

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
        headerRange.Style.Fill.BackgroundColor.SetColor(
            Color.FromArgb(217, 234, 211));
        headerRange.Style.HorizontalAlignment =
            ExcelHorizontalAlignment.Center;
        headerRange.AutoFilter = true;

        var currentRow = TaskHeaderRow + 1;

        if (report.Tasks.Count == 0)
        {
            var emptyRange = worksheet.Cells[
                currentRow,
                1,
                currentRow,
                headers.Length];

            emptyRange.Merge = true;
            emptyRange.Value = "El proyecto no contiene tareas.";
            emptyRange.Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            return;
        }

        foreach (var task in report.Tasks)
        {
            worksheet.Cells[currentRow, 1].Value = task.Title;
            worksheet.Cells[currentRow, 2].Value = task.Description;
            worksheet.Cells[currentRow, 3].Value = task.ColumnName;
            worksheet.Cells[currentRow, 4].Value =
                task.ResponsibleName ?? "Sin asignar";
            worksheet.Cells[currentRow, 5].Value =
                FormatPriority(task.Priority);

            var createdAtCell = worksheet.Cells[currentRow, 6];

            createdAtCell.Value = task.CreatedAtUtc.UtcDateTime;
            createdAtCell.Style.Numberformat.Format =
                "yyyy-mm-dd hh:mm";

            currentRow++;
        }

        worksheet.Cells[
            TaskHeaderRow,
            1,
            currentRow - 1,
            headers.Length].AutoFilter = true;
    }

    private static void ConfigureWorksheet(
        ExcelWorksheet worksheet)
    {
        worksheet.View.FreezePanes(TaskHeaderRow + 1, 1);

        worksheet.Column(1).Width = 28;
        worksheet.Column(2).Width = 45;
        worksheet.Column(3).Width = 20;
        worksheet.Column(4).Width = 28;
        worksheet.Column(5).Width = 14;
        worksheet.Column(6).Width = 20;

        worksheet.Column(2).Style.WrapText = true;

        if (worksheet.Dimension is not null)
        {
            worksheet.Cells[worksheet.Dimension.Address]
                .Style.VerticalAlignment =
                ExcelVerticalAlignment.Top;
        }

        worksheet.PrinterSettings.Orientation =
            eOrientation.Landscape;
        worksheet.PrinterSettings.FitToPage = true;
        worksheet.PrinterSettings.FitToWidth = 1;
        worksheet.PrinterSettings.FitToHeight = 0;
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
}
