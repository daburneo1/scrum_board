using Application.Common.Exceptions;
using Application.Contracts.Boards;
using Application.Ports.Persistence;
using Domain.Entities;

namespace Application.Services.Boards;

public sealed class BoardService
{
    private const int OrderSpacing = 1000;

    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BoardService(
        IBoardRepository boardRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectBoardDto> GetBoardAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _boardRepository.GetBoardAsync(
            projectId,
            cancellationToken)
            ?? throw new NotFoundException(
                "The requested project was not found.");
    }

    public Task<IReadOnlyCollection<UserOptionDto>>
        GetUsersAsync(
            CancellationToken cancellationToken = default)
    {
        return _userRepository.GetOptionsAsync(
            cancellationToken);
    }

    public async Task<BoardColumnDto> CreateColumnAsync(
        Guid projectId,
        CreateColumnRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateColumnName(request.Name);

        var project = await _projectRepository.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(
                "No se encontró el proyecto solicitado..");
        }

        var sortOrder =
            await _boardRepository.GetNextColumnSortOrderAsync(
                projectId,
                cancellationToken);

        var column = new BoardColumn(
            request.Name,
            projectId,
            sortOrder);

        _boardRepository.AddColumn(column);

        await _unitOfWork.SaveChangesAsync(
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

        await _unitOfWork.SaveChangesAsync(
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

        if (await _boardRepository.ColumnHasTasksAsync(
            columnId,
            cancellationToken))
        {
            throw new ConflictException(
                "No se puede eliminar una columna que contiene tareas.");
        }

        _boardRepository.RemoveColumn(column);

        await _unitOfWork.SaveChangesAsync(
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
            await _boardRepository.GetColumnsAsync(
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

        var columnsById = columns.ToDictionary(
            column => column.Id);

        for (var index = 0; index < orderedIds.Count; index++)
        {
            var newOrder = checked(
                (index + 1) * OrderSpacing);

            columnsById[orderedIds[index]]
                .ChangeSortOrder(newOrder);
        }

        await _unitOfWork.SaveChangesAsync(
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
            await _boardRepository.GetNextTaskSortOrderAsync(
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

        _boardRepository.AddTask(task);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await GetTaskDtoAsync(
            projectId,
            task.Id,
            cancellationToken);
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

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await GetTaskDtoAsync(
            projectId,
            task.Id,
            cancellationToken);
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

        _boardRepository.RemoveTask(task);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    private async Task ValidateAssignedUserAsync(
        Guid? assignedUserId,
        CancellationToken cancellationToken)
    {
        if (!assignedUserId.HasValue)
        {
            return;
        }

        if (!await _userRepository.ExistsAsync(
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
        return await _boardRepository.GetColumnAsync(
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
        return await _boardRepository.GetTaskAsync(
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
}