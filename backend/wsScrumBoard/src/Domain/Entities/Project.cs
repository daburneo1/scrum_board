using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Project : Entity
{
    private Project()
    {
    }

    public Project(
        string name,
        string? description,
        DateOnly startDate,
        DateOnly expectedEndDate,
        ProjectStatus status = ProjectStatus.Planned)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del proyecto es obligatorio.",
                nameof(name));
        }

        if (expectedEndDate < startDate)
        {
            throw new ArgumentException(
                "La fecha final no puede ser menor que la fecha de inicio.",
                nameof(expectedEndDate));
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        StartDate = startDate;
        ExpectedEndDate = expectedEndDate;
        Status = status;
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateOnly StartDate { get; private set; }

    public DateOnly ExpectedEndDate { get; private set; }

    public ProjectStatus Status { get; private set; }

    public ICollection<BoardColumn> Columns { get; private set; } =
        new List<BoardColumn>();

    public void Update(
        string name,
        string? description,
        DateOnly startDate,
        DateOnly expectedEndDate,
        ProjectStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del proyecto es obligatorio.",
                nameof(name));
        }

        if (expectedEndDate < startDate)
        {
            throw new ArgumentException(
                "La fecha final no puede ser menor que la fecha de inicio.",
                nameof(expectedEndDate));
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        StartDate = startDate;
        ExpectedEndDate = expectedEndDate;
        Status = status;
    }
}
