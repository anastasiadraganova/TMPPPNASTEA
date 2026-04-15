using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services;

/// <summary>
/// Applies a chain of project query strategies.
/// </summary>
public sealed class ProjectFilterContext
{
    private readonly IReadOnlyList<IProjectFilterStrategy> _strategies;

    /// <summary>
    /// Creates a filter context.
    /// </summary>
    /// <param name="strategies">Strategy collection in DI registration order.</param>
    public ProjectFilterContext(IEnumerable<IProjectFilterStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    /// <summary>
    /// Applies all configured strategies to a query.
    /// </summary>
    /// <param name="query">Source query.</param>
    /// <param name="parameters">Search parameters.</param>
    /// <returns>Filtered query.</returns>
    public IQueryable<Project> ApplyAll(IQueryable<Project> query, ProjectSearchParams parameters)
    {
        var current = query;
        foreach (var strategy in _strategies)
        {
            current = strategy.Apply(current, parameters);
        }

        return current;
    }
}
