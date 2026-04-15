using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Strategies;

/// <summary>
/// Applies case-insensitive keyword search by title and description.
/// </summary>
public sealed class KeywordSearchStrategy : IProjectFilterStrategy
{
    /// <inheritdoc />
    public IQueryable<Project> Apply(IQueryable<Project> query, ProjectSearchParams parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            return query;
        }

        var keyword = parameters.Keyword.Trim().ToLowerInvariant();
        return query.Where(p =>
            p.Title.ToLowerInvariant().Contains(keyword) ||
            p.Description.ToLowerInvariant().Contains(keyword));
    }
}
