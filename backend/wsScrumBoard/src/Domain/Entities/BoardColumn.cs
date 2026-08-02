using Domain.Common;

namespace Domain.Entities;

public sealed class BoardColumn : Entity
{
    private BoardColumn()
    {
    }

    public BoardColumn(
        string name,
        Guid projectId,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre de la columna es obligatorio.",
                nameof(name));
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Se requiere un identificador de proyecto válido.",
                nameof(projectId));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "El orden no puede ser negativo.");
        }

        Name = name.Trim();
        ProjectId = projectId;
        SortOrder = sortOrder;
    }

    public string Name { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public Guid ProjectId { get; private set; }

    public Project Project { get; private set; } = null!;

    public ICollection<BoardTask> Tasks { get; private set; } =
        new List<BoardTask>();
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
    }
    public void ChangeSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "El orden no puede ser negativo.");
        }

        SortOrder = sortOrder;
    }
}
