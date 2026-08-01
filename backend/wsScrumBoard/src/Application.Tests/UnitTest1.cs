namespace Application.Tests;

using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Contracts.Projects;
using Application.Ports.Persistence;
using Application.Services.Projects;
using Domain.Entities;
using Domain.Enums;

public sealed class ProjectServiceTests
{
    private FakeProjectRepository _repository = null!;
    private FakeUnitOfWork _unitOfWork = null!;
    private ProjectService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new FakeProjectRepository();
        _unitOfWork = new FakeUnitOfWork();
        _service = new ProjectService(_repository, _unitOfWork);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void GetPagedAsync_WhenPageNumberIsNotPositive_ThrowsValidationException(
        int pageNumber)
    {
        var parameters = new ProjectQueryParameters(pageNumber, 10);

        var exception = Assert.ThrowsAsync<ValidationException>(
            async () => await _service.GetPagedAsync(parameters));

        Assert.That(
            exception!.Message,
            Is.EqualTo("El número de página debe ser mayor que 0."));
    }

    [TestCase(0)]
    [TestCase(101)]
    public void GetPagedAsync_WhenPageSizeIsOutsideRange_ThrowsValidationException(
        int pageSize)
    {
        var parameters = new ProjectQueryParameters(1, pageSize);

        var exception = Assert.ThrowsAsync<ValidationException>(
            async () => await _service.GetPagedAsync(parameters));

        Assert.That(
            exception!.Message,
            Is.EqualTo("El tamaño de página debe estar entre 1 y 100."));
    }

    [Test]
    public async Task GetPagedAsync_WithValidParameters_ReturnsRepositoryResult()
    {
        var parameters = new ProjectQueryParameters(2, 20, "Portal");
        _repository.PagedResult = new PagedResult<ProjectDto>(
            Array.Empty<ProjectDto>(),
            2,
            20,
            25);

        var result = await _service.GetPagedAsync(parameters);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(_repository.PagedResult));
            Assert.That(_repository.LastQuery, Is.SameAs(parameters));
        });
    }

    [Test]
    public async Task GetByIdAsync_WhenProjectExists_ReturnsProject()
    {
        var project = CreateProject();
        _repository.Projects.Add(project.Id, project);

        var result = await _service.GetByIdAsync(project.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(project.Id));
            Assert.That(result.Name, Is.EqualTo(project.Name));
        });
    }

    [Test]
    public void GetByIdAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();

        var exception = Assert.ThrowsAsync<NotFoundException>(
            async () => await _service.GetByIdAsync(id));

        Assert.That(exception!.Message, Does.Contain(id.ToString()));
    }

    [Test]
    public void CreateAsync_WhenNameIsEmpty_ThrowsValidationException()
    {
        var request = CreateRequest(name: " ");

        var exception = Assert.ThrowsAsync<ValidationException>(
            async () => await _service.CreateAsync(request));

        Assert.That(
            exception!.Message,
            Is.EqualTo("El nombre del proyecto es obligatorio."));
    }

    [Test]
    public void CreateAsync_WhenEndDateIsBeforeStartDate_ThrowsValidationException()
    {
        var request = CreateRequest(
            startDate: new DateOnly(2026, 8, 10),
            expectedEndDate: new DateOnly(2026, 8, 9));

        var exception = Assert.ThrowsAsync<ValidationException>(
            async () => await _service.CreateAsync(request));

        Assert.That(
            exception!.Message,
            Is.EqualTo("La fecha final no puede ser menor que la fecha de inicio."));
    }

    [Test]
    public async Task CreateAsync_WithValidRequest_AddsAndSavesProject()
    {
        var request = CreateRequest(name: "  Portal web  ");

        var result = await _service.CreateAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Portal web"));
            Assert.That(_repository.AddedProject, Is.Not.Null);
            Assert.That(_unitOfWork.SaveChangesCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task UpdateAsync_WithValidRequest_UpdatesAndSavesProject()
    {
        var project = CreateProject();
        _repository.Projects.Add(project.Id, project);
        var request = new UpdateProjectRequest(
            "Proyecto actualizado",
            "Nueva descripción",
            new DateOnly(2026, 8, 2),
            new DateOnly(2026, 9, 1),
            ProjectStatus.Active);

        var result = await _service.UpdateAsync(project.Id, request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo(request.Name));
            Assert.That(result.Status, Is.EqualTo(ProjectStatus.Active));
            Assert.That(_unitOfWork.SaveChangesCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void UpdateAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
    {
        var request = new UpdateProjectRequest(
            "Proyecto",
            null,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 2),
            ProjectStatus.Planned);

        Assert.ThrowsAsync<NotFoundException>(
            async () => await _service.UpdateAsync(Guid.NewGuid(), request));
    }

    [Test]
    public async Task DeleteAsync_WhenProjectExists_RemovesAndSavesProject()
    {
        var project = CreateProject();
        _repository.Projects.Add(project.Id, project);

        await _service.DeleteAsync(project.Id);

        Assert.Multiple(() =>
        {
            Assert.That(_repository.RemovedProject, Is.SameAs(project));
            Assert.That(_unitOfWork.SaveChangesCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void DeleteAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
    {
        Assert.ThrowsAsync<NotFoundException>(
            async () => await _service.DeleteAsync(Guid.NewGuid()));
    }

    private static CreateProjectRequest CreateRequest(
        string name = "Portal web",
        DateOnly? startDate = null,
        DateOnly? expectedEndDate = null)
    {
        return new CreateProjectRequest(
            name,
            "Descripción",
            startDate ?? new DateOnly(2026, 8, 1),
            expectedEndDate ?? new DateOnly(2026, 9, 1),
            ProjectStatus.Planned);
    }

    private static Project CreateProject()
    {
        return new Project(
            "Portal web",
            "Descripción",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 9, 1),
            ProjectStatus.Planned);
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        public Dictionary<Guid, Project> Projects { get; } = new();

        public PagedResult<ProjectDto> PagedResult { get; set; } =
            new(Array.Empty<ProjectDto>(), 1, 10, 0);

        public ProjectQueryParameters? LastQuery { get; private set; }

        public Project? AddedProject { get; private set; }

        public Project? RemovedProject { get; private set; }

        public Task<PagedResult<ProjectDto>> GetPagedAsync(
            ProjectQueryParameters parameters,
            CancellationToken cancellationToken = default)
        {
            LastQuery = parameters;
            return Task.FromResult(PagedResult);
        }

        public Task<Project?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            Projects.TryGetValue(id, out var project);
            return Task.FromResult(project);
        }

        public void Add(Project project)
        {
            AddedProject = project;
            Projects.Add(project.Id, project);
        }

        public void Remove(Project project)
        {
            RemovedProject = project;
            Projects.Remove(project.Id);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }
    }
}
