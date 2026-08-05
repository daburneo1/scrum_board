using Application.Common.Exceptions;
using Application.Contracts.Boards;
using Application.Contracts.Tasks;
using Application.Ports.Persistence;
using Application.RealTime.Boards;
using Application.Tasks.Ordering;
using Domain.Entities;

namespace Application.Services.Boards;

public sealed class BoardService(
    IBoardRepository boardRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    TaskOrderCalculator taskOrderCalculator,
    IBoardRealtimeNotifier realtimeNotifier)
{
    private const int OrderSpacing = 1000;

    public async Task<ProjectBoardDto> GetBoardAsync(
        Guid projectId,
        ProjectTaskFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await boardRepository.GetBoardAsync(
                   projectId,
                   filter ?? ProjectTaskFilter.Empty,
                   cancellationToken)
               ?? throw new NotFoundException(
                   "The requested project was not found.");
    }

    public Task<IReadOnlyCollection<UserOptionDto>>
        GetUsersAsync(
            CancellationToken cancellationToken = default)
    {
        return userRepository.GetOptionsAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<BoardColumnDto>> GetColumnsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var board = await GetBoardAsync(
            projectId,
            ProjectTaskFilter.Empty,
            cancellationToken);

        return board.Columns;
    }

    public async Task<BoardColumnDto> GetColumnAsync(
        Guid projectId,
        Guid columnId,
        CancellationToken cancellationToken = default)
    {
        var columns = await GetColumnsAsync(
            projectId,
            cancellationToken);

        return columns.SingleOrDefault(column => column.Id == columnId)
               ?? throw new NotFoundException(
                   "No se encontró la columna solicitada.");
    }

    public async Task<IReadOnlyCollection<BoardTaskDto>> GetTasksAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var columns = await GetColumnsAsync(
            projectId,
            cancellationToken);

        return columns
            .SelectMany(column => column.Tasks)
            .ToArray();
    }

    public async Task<BoardTaskDto> GetTaskAsync(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var tasks = await GetTasksAsync(
            projectId,
            cancellationToken);

        return tasks.SingleOrDefault(task => task.Id == taskId)
               ?? throw new NotFoundException(
                   "No se encontró la tarea.");
    }

    public async Task<BoardColumnDto> CreateColumnAsync(
        Guid projectId,
        CreateColumnRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateColumnName(request.Name);

        var project = await projectRepository.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(
                "No se encontró el proyecto solicitado..");
        }

        var sortOrder =
            await boardRepository.GetNextColumnSortOrderAsync(
                projectId,
                cancellationToken);

        var column = new BoardColumn(
            request.Name,
            projectId,
            sortOrder);

        boardRepository.AddColumn(column);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return MapColumn(column);
    }

    public async Task<BoardColumnDto> UpdateColumnAsync(
        Guid projectId,
        Guid columnId,
        UpdateColumnRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateColumnName(request.Name);

        var column = await GetColumnOrThrowAsync(
            projectId,
            columnId,
            cancellationToken);

        column.Rename(request.Name);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return MapColumn(column);
    }

    public async Task DeleteColumnAsync(
        Guid projectId,
        Guid columnId,
        CancellationToken cancellationToken = default)
    {
        var column = await GetColumnOrThrowAsync(
            projectId,
            columnId,
            cancellationToken);

        if (await boardRepository.ColumnHasTasksAsync(
                columnId,
                cancellationToken))
        {
            throw new ConflictException(
                "No se puede eliminar una columna que contiene tareas.");
        }

        boardRepository.RemoveColumn(column);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ReorderColumnsAsync(
        Guid projectId,
        ReorderColumnsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OrderedColumnIds is null ||
            request.OrderedColumnIds.Count == 0)
        {
            throw new ValidationException(
                "Se requieren los identificadores de columna ordenados.");
        }

        var orderedIds =
            request.OrderedColumnIds.ToList();

        if (orderedIds.Distinct().Count() != orderedIds.Count)
        {
            throw new ValidationException(
                "Los identificadores de columna ordenados contienen duplicados.");
        }

        var columns = (
                await boardRepository.GetColumnsAsync(
                    projectId,
                    cancellationToken))
            .ToList();

        if (columns.Count == 0)
        {
            throw new NotFoundException(
                "El proyecto no contiene columnas.");
        }

        var existingIds = columns
            .Select(column => column.Id)
            .ToHashSet();

        if (orderedIds.Count != existingIds.Count ||
            orderedIds.Any(id => !existingIds.Contains(id)))
        {
            throw new ValidationException(
                "La solicitud debe contener todas las columnas del proyecto exactamente una vez.");
        }

        var columnsById = columns.ToDictionary(column => column.Id);

        for (var index = 0; index < orderedIds.Count; index++)
        {
            var newOrder = checked(
                (index + 1) * OrderSpacing);

            columnsById[orderedIds[index]]
                .ChangeSortOrder(newOrder);
        }

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<BoardTaskDto> CreateTaskAsync(
        Guid projectId,
        CreateBoardTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateTask(request.Title);

        var column = await GetColumnOrThrowAsync(
            projectId,
            request.ColumnId,
            cancellationToken);

        await ValidateAssignedUserAsync(
            request.AssignedUserId,
            cancellationToken);

        var sortOrder =
            await boardRepository.GetNextTaskSortOrderAsync(
                column.Id,
                cancellationToken);

        var task = new BoardTask(
            request.Title,
            request.Description,
            request.Priority,
            column.Id,
            sortOrder,
            DateTimeOffset.UtcNow,
            request.AssignedUserId);

        boardRepository.AddTask(task);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        var createdTask = await GetTaskDtoAsync(
            projectId,
            task.Id,
            cancellationToken);

        await NotifyTaskChangeAsync(
            projectId,
            task.Id,
            BoardChangeType.TaskCreated);

        return createdTask;
    }

    public async Task<BoardTaskDto> UpdateTaskAsync(
        Guid projectId,
        Guid taskId,
        UpdateBoardTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateTask(request.Title);

        await ValidateAssignedUserAsync(
            request.AssignedUserId,
            cancellationToken);

        var task = await GetTaskOrThrowAsync(
            projectId,
            taskId,
            cancellationToken);

        task.Update(
            request.Title,
            request.Description,
            request.Priority,
            request.AssignedUserId);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        var updatedTask = await GetTaskDtoAsync(
            projectId,
            task.Id,
            cancellationToken);

        await NotifyTaskChangeAsync(
            projectId,
            task.Id,
            BoardChangeType.TaskUpdated);

        return updatedTask;
    }

    public async Task DeleteTaskAsync(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(
            projectId,
            taskId,
            cancellationToken);

        boardRepository.RemoveTask(task);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        await NotifyTaskChangeAsync(
            projectId,
            taskId,
            BoardChangeType.TaskDeleted);
    }

    private async Task ValidateAssignedUserAsync(
        Guid? assignedUserId,
        CancellationToken cancellationToken)
    {
        if (!assignedUserId.HasValue)
        {
            return;
        }

        if (!await userRepository.ExistsAsync(
                assignedUserId.Value,
                cancellationToken))
        {
            throw new ValidationException(
                "El usuario seleccionado no existe.");
        }
    }

    private Task<BoardColumn> GetColumnOrThrowAsync(
        Guid projectId,
        Guid columnId,
        CancellationToken cancellationToken)
    {
        return GetColumnInternalAsync(
            projectId,
            columnId,
            cancellationToken);
    }

    private async Task<BoardColumn> GetColumnInternalAsync(
        Guid projectId,
        Guid columnId,
        CancellationToken cancellationToken)
    {
        return await boardRepository.GetColumnAsync(
                   projectId,
                   columnId,
                   cancellationToken)
               ?? throw new NotFoundException(
                   "No se encontró la columna solicitada");
    }

    private async Task<BoardTask> GetTaskOrThrowAsync(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        return await boardRepository.GetTaskAsync(
                   projectId,
                   taskId,
                   cancellationToken)
               ?? throw new NotFoundException(
                   "No se encontró la tarea.");
    }

    private async Task<BoardTaskDto> GetTaskDtoAsync(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var board = await GetBoardAsync(
            projectId,
            ProjectTaskFilter.Empty,
            cancellationToken);

        return board.Columns
            .SelectMany(column => column.Tasks)
            .Single(task => task.Id == taskId);
    }

    private static BoardColumnDto MapColumn(
        BoardColumn column)
    {
        return new BoardColumnDto(
            column.Id,
            column.Name,
            column.SortOrder,
            Array.Empty<BoardTaskDto>());
    }

    private static void ValidateColumnName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "Se requiere el nombre de la columna.");
        }

        if (name.Trim().Length > 120)
        {
            throw new ValidationException(
                "El nombre de la columna no puede exceder los 120 caracteres.");
        }
    }

    private static void ValidateTask(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException(
                "El título de la tarea es obligatorio.");
        }

        if (title.Trim().Length > 250)
        {
            throw new ValidationException(
                "El título de la tarea no puede exceder los 250 caracteres.");
        }
    }

    public async Task<MoveTaskResponse> MoveTaskAsync(
        Guid projectId,
        Guid taskId,
        MoveTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TargetColumnId == Guid.Empty)
        {
            throw new ValidationException(
                "A valid target column is required.");
        }

        if (request.TargetIndex < 0)
        {
            throw new ValidationException(
                "The target index cannot be negative.");
        }

        var task = await GetTaskOrThrowAsync(
            projectId,
            taskId,
            cancellationToken);

        var sourceColumnId = task.ColumnId;

        _ = await GetColumnOrThrowAsync(
            projectId,
            request.TargetColumnId,
            cancellationToken);

        var affectedColumnIds =
            sourceColumnId == request.TargetColumnId
                ? new[]
                {
                    sourceColumnId
                }
                : new[]
                {
                    sourceColumnId,
                    request.TargetColumnId
                };

        var affectedTasks =
            await boardRepository.GetTasksForColumnsAsync(
                projectId,
                affectedColumnIds,
                cancellationToken);

        var sourceTaskIds = affectedTasks
            .Where(item =>
                item.ColumnId == sourceColumnId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .Select(item => item.Id)
            .ToList();

        var targetTaskIds =
            sourceColumnId == request.TargetColumnId
                ? sourceTaskIds
                : affectedTasks
                    .Where(item =>
                        item.ColumnId ==
                        request.TargetColumnId)
                    .OrderBy(item => item.SortOrder)
                    .ThenBy(item => item.Id)
                    .Select(item => item.Id)
                    .ToList();

        var plan = taskOrderCalculator.Calculate(
            taskId,
            sourceColumnId,
            request.TargetColumnId,
            request.TargetIndex,
            sourceTaskIds,
            targetTaskIds);

        var tasksById = affectedTasks
            .ToDictionary(item => item.Id);

        foreach (var change in plan.Changes)
        {
            if (!tasksById.TryGetValue(
                    change.TaskId,
                    out var affectedTask))
            {
                throw new InvalidOperationException(
                    "The task ordering plan contains an unknown task.");
            }

            affectedTask.ChangePosition(
                change.ColumnId,
                change.SortOrder);
        }

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        var affectedColumns =
            await boardRepository.GetColumnDtosAsync(
                projectId,
                affectedColumnIds,
                cancellationToken);

        await NotifyTaskChangeAsync(
            projectId,
            taskId,
            BoardChangeType.TaskMoved);

        return new MoveTaskResponse(
            taskId,
            sourceColumnId,
            request.TargetColumnId,
            affectedColumns);
    }

    private Task NotifyTaskChangeAsync(
        Guid projectId,
        Guid taskId,
        BoardChangeType changeType)
    {
        var notification =
            new BoardChangedNotification(
                Guid.NewGuid(),
                projectId,
                changeType,
                taskId,
                DateTimeOffset.UtcNow);

        return realtimeNotifier
            .NotifyBoardChangedAsync(notification);
    }
}
