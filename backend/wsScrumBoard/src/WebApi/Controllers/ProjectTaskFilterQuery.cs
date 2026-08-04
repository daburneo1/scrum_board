using Application.Contracts.Tasks;
using Domain.Enums;

namespace WebApi.Controllers;

public sealed record ProjectTaskFilterQuery(
    Guid? AssigneeId,
    WorkItemPriority? Priority,
    string? Search)
{
    public ProjectTaskFilter ToFilter()
    {
        return ProjectTaskFilter.Create(
            AssigneeId,
            Priority,
            Search);
    }
}
