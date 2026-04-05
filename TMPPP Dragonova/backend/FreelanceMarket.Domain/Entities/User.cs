using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Project> CustomerProjects { get; set; } = new List<Project>();
    public ICollection<Project> FreelancerProjects { get; set; } = new List<Project>();
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
}
