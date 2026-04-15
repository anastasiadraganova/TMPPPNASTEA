using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Describes project catalog search and sorting parameters.
/// </summary>
/// <param name="Status">Optional project status filter.</param>
/// <param name="MaxBudget">Optional upper budget bound.</param>
/// <param name="MinBudget">Optional lower budget bound.</param>
/// <param name="Keyword">Optional title/description search phrase.</param>
/// <param name="RequiredSkills">Optional skills to match.</param>
/// <param name="Type">Optional project type filter.</param>
/// <param name="SortBy">Sorting mode: newest, budget_asc, budget_desc, deadline.</param>
public sealed record ProjectSearchParams(
    ProjectStatus? Status,
    decimal? MaxBudget,
    decimal? MinBudget,
    string? Keyword,
    List<string>? RequiredSkills,
    ProjectType? Type,
    string SortBy = "newest");
