using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Strategies;

/// <summary>
/// Applies project sorting based on search parameters.
/// </summary>
public sealed class ProjectSortStrategy : IProjectFilterStrategy
{
    /// <inheritdoc />
    public IQueryable<Project> Apply(IQueryable<Project> query, ProjectSearchParams parameters)
    {
        return (parameters.SortBy ?? "newest").ToLowerInvariant() switch
        {
            "budget_asc" => query.OrderBy(p => p.Budget),
            "budget_desc" => query.OrderByDescending(p => p.Budget),
            "deadline" => query.OrderBy(p => p.Deadline ?? DateTime.MaxValue),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }
}
