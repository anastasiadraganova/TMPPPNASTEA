namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Внутренняя модель публичного портфолио фрилансера,
/// к которой приводятся внешние профили (через Adapter).
/// </summary>
public class FreelancerPortfolioProfile
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public IReadOnlyList<string> Skills { get; set; } = Array.Empty<string>();
    public int CompletedProjects { get; set; }
    public double ExternalRating { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Условный внешний DTO GitHub-подобного источника.
/// </summary>
public class GithubProfileDto
{
    public string Login { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int PublicRepos { get; set; }
    public int Followers { get; set; }
    public IReadOnlyList<string> Topics { get; set; } = Array.Empty<string>();
}
