using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Infrastructure.Adapters;

/// <summary>
/// Adapter: имитирует внешний GitHub-источник и адаптирует его DTO
/// к внутренней модели FreelancerPortfolioProfile.
/// </summary>
public class GithubFreelancerProfileAdapter : IExternalFreelancerProfileAdapter
{
    public Task<FreelancerPortfolioProfile?> GetPortfolioProfileAsync(
        Guid userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        // Имитация внешнего профиля, построенная детерминированно по email.
        var external = SimulateGithubProfile(email, displayName);

        var adapted = new FreelancerPortfolioProfile
        {
            UserId = userId,
            DisplayName = displayName,
            Bio = string.IsNullOrWhiteSpace(external.About)
                ? $"{displayName} builds software products and collaborates with distributed teams."
                : external.About,
            AvatarUrl = external.AvatarUrl,
            Skills = external.Topics.Count > 0 ? external.Topics : new[] { "C#", "TypeScript", "REST API" },
            CompletedProjects = Math.Max(1, external.PublicRepos / 2),
            ExternalRating = Math.Min(5.0, 3.0 + (external.Followers / 1000.0)),
            Source = "github"
        };

        return Task.FromResult<FreelancerPortfolioProfile?>(adapted);
    }

    private static GithubProfileDto SimulateGithubProfile(string email, string displayName)
    {
        var hash = Math.Abs(email.GetHashCode());
        var topicVariants = new List<string[]>
        {
            new[] { "React", "Next.js", "TypeScript" },
            new[] { "ASP.NET", "C#", "PostgreSQL" },
            new[] { "Node.js", "NestJS", "Docker" },
            new[] { "Flutter", "Dart", "Firebase" }
        };

        var topics = topicVariants[hash % topicVariants.Count];

        return new GithubProfileDto
        {
            Login = displayName.Replace(" ", string.Empty).ToLowerInvariant(),
            About = $"{displayName} specializes in {topics[0]} and {topics[1]} projects.",
            AvatarUrl = $"https://api.dicebear.com/7.x/initials/svg?seed={Uri.EscapeDataString(displayName)}",
            PublicRepos = 10 + (hash % 45),
            Followers = 50 + (hash % 3000),
            Topics = topics
        };
    }
}
