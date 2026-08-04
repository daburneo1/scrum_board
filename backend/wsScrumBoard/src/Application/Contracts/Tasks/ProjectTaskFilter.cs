using Application.Common.Exceptions;
using Domain.Enums;

namespace Application.Contracts.Tasks;

public sealed record ProjectTaskFilter(
    Guid? AssigneeId,
    WorkItemPriority? Priority,
    string? Search)
{
    public const int MaximumSearchLength = 100;

    public static readonly ProjectTaskFilter Empty = new(
        null,
        null,
        null);

    public bool HasActiveFilters =>
        AssigneeId.HasValue ||
        Priority.HasValue ||
        Search is not null;

    public static ProjectTaskFilter Create(
        Guid? assigneeId,
        WorkItemPriority? priority,
        string? search)
    {
        if (assigneeId == Guid.Empty)
        {
            throw new ValidationException(
                "Se requiere un responsable válido.");
        }

        if (priority.HasValue &&
            !Enum.IsDefined(priority.Value))
        {
            throw new ValidationException(
                "La prioridad seleccionada no es válida.");
        }

        var normalizedSearch = NormalizeSearch(search);

        return new ProjectTaskFilter(
            assigneeId,
            priority,
            normalizedSearch);
    }

    private static string? NormalizeSearch(
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        var normalized = search.Trim();

        if (normalized.Length > MaximumSearchLength)
        {
            normalized = normalized[..MaximumSearchLength];
        }

        return normalized.Length == 0
            ? null
            : normalized;
    }
}
