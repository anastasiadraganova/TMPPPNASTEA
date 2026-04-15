using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Strategies;

/// <summary>
/// Filters projects by optional minimum and maximum budget.
/// </summary>
public sealed class BudgetRangeFilterStrategy : IProjectFilterStrategy
{
    /// <inheritdoc />
    public IQueryable<Project> Apply(IQueryable<Project> query, ProjectSearchParams parameters)
    {
        if (parameters.MinBudget.HasValue)
        {
            query = query.Where(p => p.Budget >= parameters.MinBudget.Value);
        }

        if (parameters.MaxBudget.HasValue)
        {
            query = query.Where(p => p.Budget <= parameters.MaxBudget.Value);
        }

        return query;
    }
}
