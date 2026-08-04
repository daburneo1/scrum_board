using Application.Common.Exceptions;
using Application.Ports.Persistence;

namespace Application.Services.Boards;

public sealed class BoardSubscriptionService(IProjectRepository projectRepository)
{
    public async Task EnsureProjectExistsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "Se requiere un identificador de proyecto válido.");
        }

        var exists = await projectRepository.ExistsAsync(
            projectId,
            cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(
                "No se encotnró el proyecto.");
        }
    }
}