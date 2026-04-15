using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Strategies;

/// <summary>
/// Filters projects by status and type parameters.
/// </summary>
public sealed class StatusFilterStrategy : IProjectFilterStrategy
{
    /// <inheritdoc />
    public IQueryable<Project> Apply(IQueryable<Project> query, ProjectSearchParams parameters)
    {
        if (parameters.Status.HasValue)
        {
            query = query.Where(p => p.Status == parameters.Status.Value);
        }

        if (parameters.Type.HasValue)
        {
            query = query.Where(p => p.Type == parameters.Type.Value);
        }

        return query;
    }
}
