using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class BoardTask : Entity
{
    private BoardTask()
    {
    }

    public BoardTask(
        string title,
        string description,
        WorkItemPriority priority,
        Guid columnId,
        int sortOrder,
        DateTimeOffset createdAtUtc,
        Guid? assignedUserId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (columnId == Guid.Empty)
        {
            throw new ArgumentException(
                "Se requiere un identificador de columna válido.",
                nameof(columnId));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "El orden de clasificación no puede ser negativo.");
        }

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Priority = priority;
        ColumnId = columnId;
        SortOrder = sortOrder;
        CreatedAtUtc = createdAtUtc;
        AssignedUserId = assignedUserId;
    }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public WorkItemPriority Priority { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid ColumnId { get; private set; }

    public BoardColumn Column { get; private set; } = null!;

    public Guid? AssignedUserId { get; private set; }

    public AppUser? AssignedUser { get; private set; }
}