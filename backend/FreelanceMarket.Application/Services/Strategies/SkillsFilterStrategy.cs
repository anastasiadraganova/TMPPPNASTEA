using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Strategies;

/// <summary>
/// Filters projects that match any requested skill.
/// </summary>
public sealed class SkillsFilterStrategy : IProjectFilterStrategy
{
    /// <inheritdoc />
    public IQueryable<Project> Apply(IQueryable<Project> query, ProjectSearchParams parameters)
    {
        if (parameters.RequiredSkills is null || parameters.RequiredSkills.Count == 0)
        {
            return query;
        }

        var requestedSkills = parameters.RequiredSkills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(skill => skill.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (requestedSkills.Count == 0)
        {
            return query;
        }

        return query.Where(project => project.RequiredSkills
            .Any(projectSkill => requestedSkills.Contains(projectSkill.ToLowerInvariant())));
    }
}
