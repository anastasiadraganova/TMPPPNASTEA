using FreelanceMarket.Domain.Entities;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Defines a project query filtering or sorting strategy.
/// </summary>
public interface IProjectFilterStrategy
{
    /// <summary>
    /// Applies a strategy to the source query.
    /// </summary>
    /// <param name="query">Source query.</param>
    /// <param name="parameters">Search parameters.</param>
    /// <returns>Transformed query.</returns>
    IQueryable<Project> Apply(IQueryable<Project> query, ProjectSearchParams parameters);
}
