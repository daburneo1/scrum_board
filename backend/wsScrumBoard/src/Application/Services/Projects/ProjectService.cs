namespace Application.Services.Projects;

using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Contracts.Projects;
using Application.Ports.Persistence;
using Domain.Entities;

public sealed class ProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<PagedResult<ProjectDto>> GetPagedAsync(
        ProjectQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (parameters is null)
        {
            throw new ValidationException(
                "Los parámetros de consulta son obligatorios.");
        }

        ValidatePagination(parameters);

        return _projectRepository.GetPagedAsync(
            parameters,
            cancellationToken);
    }

    public async Task<ProjectDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(
            id,
            cancellationToken);

        return ToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException(
                "Los datos del proyecto son obligatorios.");
        }

        ValidateProject(
            request.Name,
            request.StartDate,
            request.ExpectedEndDate);

        var project = new Project(
            request.Name,
            request.Description,
            request.StartDate,
            request.ExpectedEndDate,
            request.Status);

        _projectRepository.Add(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException(
                "Los datos del proyecto son obligatorios.");
        }

        ValidateProject(
            request.Name,
            request.StartDate,
            request.ExpectedEndDate);

        var project = await GetProjectOrThrowAsync(
            id,
            cancellationToken);

        project.Update(
            request.Name,
            request.Description,
            request.StartDate,
            request.ExpectedEndDate,
            request.Status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(project);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(
            id,
            cancellationToken);

        _projectRepository.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Project> GetProjectOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _projectRepository.GetByIdAsync(
                   id,
                   cancellationToken)
               ?? throw new NotFoundException(
                   $"No se encontró el proyecto con identificador '{id}'.");
    }

    private static void ValidatePagination(
        ProjectQueryParameters parameters)
    {
        if (parameters.PageNumber <= 0)
        {
            throw new ValidationException(
                "El número de página debe ser mayor que 0.");
        }

        if (parameters.PageSize is < 1 or > 100)
        {
            throw new ValidationException(
                "El tamaño de página debe estar entre 1 y 100.");
        }
    }

    private static void ValidateProject(
        string name,
        DateOnly startDate,
        DateOnly expectedEndDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "El nombre del proyecto es obligatorio.");
        }

        if (expectedEndDate < startDate)
        {
            throw new ValidationException(
                "La fecha final no puede ser menor que la fecha de inicio.");
        }
    }

    private static ProjectDto ToDto(Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.StartDate,
            project.ExpectedEndDate,
            project.Status);
    }
}
