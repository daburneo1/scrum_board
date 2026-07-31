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
        string description,
        DateOnly startDate,
        DateOnly expectedEndDate,
        ProjectStatus status = ProjectStatus.Planned)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (expectedEndDate < startDate)
        {
            throw new ArgumentException(
                "La fecha final no puede ser menor a la fecha de inicio",
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
}