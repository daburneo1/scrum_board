using Application.Common.Exceptions;

namespace Application.Tasks.Ordering;

public sealed class TaskOrderCalculator
{
    public const int OrderSpacing = 1000;

    public TaskMovePlan Calculate(
        Guid taskId,
        Guid sourceColumnId,
        Guid targetColumnId,
        int targetIndex,
        IReadOnlyList<Guid> sourceOrderedTaskIds,
        IReadOnlyList<Guid> targetOrderedTaskIds)
    {
        if (taskId == Guid.Empty)
        {
            throw new ValidationException(
                "Se requiere un identificador de tarea válido.");
        }

        if (sourceColumnId == Guid.Empty ||
            targetColumnId == Guid.Empty)
        {
            throw new ValidationException(
                "Se requieren identificadores válidos para las columnas de origen y destino.");
        }

        EnsureUnique(
            sourceOrderedTaskIds,
            "La columna de origen contiene tareas duplicadas.");

        if (!sourceOrderedTaskIds.Contains(taskId))
        {
            throw new ValidationException(
                "La tarea no pertenece a la columna de origen.");
        }

        return sourceColumnId == targetColumnId
            ? CalculateWithinSameColumn(
                taskId,
                sourceColumnId,
                targetIndex,
                sourceOrderedTaskIds)
            : CalculateBetweenColumns(
                taskId,
                sourceColumnId,
                targetColumnId,
                targetIndex,
                sourceOrderedTaskIds,
                targetOrderedTaskIds);
    }

    private static TaskMovePlan CalculateWithinSameColumn(
        Guid taskId,
        Guid columnId,
        int targetIndex,
        IReadOnlyList<Guid> orderedTaskIds)
    {
        var reorderedIds = orderedTaskIds
            .Where(id => id != taskId)
            .ToList();

        ValidateTargetIndex(
            targetIndex,
            reorderedIds.Count);

        reorderedIds.Insert(
            targetIndex,
            taskId);

        return new TaskMovePlan(
            BuildChanges(
                reorderedIds,
                columnId));
    }

    private static TaskMovePlan CalculateBetweenColumns(
        Guid taskId,
        Guid sourceColumnId,
        Guid targetColumnId,
        int targetIndex,
        IReadOnlyList<Guid> sourceOrderedTaskIds,
        IReadOnlyList<Guid> targetOrderedTaskIds)
    {
        EnsureUnique(
            targetOrderedTaskIds,
            "La columna de destino contiene tareas duplicadas.");

        if (targetOrderedTaskIds.Contains(taskId))
        {
            throw new ValidationException(
                "La tarea ya se encuentra en la columna de destino.");
        }

        var duplicatedAcrossColumns = sourceOrderedTaskIds
            .Where(id => id != taskId)
            .Intersect(targetOrderedTaskIds)
            .Any();

        if (duplicatedAcrossColumns)
        {
            throw new ValidationException(
                "Una tarea no puede pertenecer a ambas columnas.");
        }

        var sourceResult = sourceOrderedTaskIds
            .Where(id => id != taskId)
            .ToList();

        var targetResult =
            targetOrderedTaskIds.ToList();

        ValidateTargetIndex(
            targetIndex,
            targetResult.Count);

        targetResult.Insert(
            targetIndex,
            taskId);

        var changes = BuildChanges(
                sourceResult,
                sourceColumnId)
            .Concat(
                BuildChanges(
                    targetResult,
                    targetColumnId))
            .ToList();

        return new TaskMovePlan(changes);
    }

    private static IReadOnlyCollection<TaskPositionChange>
        BuildChanges(
            IReadOnlyList<Guid> orderedIds,
            Guid columnId)
    {
        return orderedIds
            .Select((id, index) =>
                new TaskPositionChange(
                    id,
                    columnId,
                    checked(
                        (index + 1) *
                        OrderSpacing)))
            .ToList();
    }

    private static void ValidateTargetIndex(
        int targetIndex,
        int availablePositions)
    {
        if (
            targetIndex < 0 ||
            targetIndex > availablePositions
        )
        {
            throw new ValidationException(
                "La posición de destino de la tarea no es válida.");
        }
    }

    private static void EnsureUnique(
        IReadOnlyList<Guid> ids,
        string message)
    {
        if (ids.Distinct().Count() != ids.Count)
        {
            throw new ValidationException(message);
        }
    }
}
