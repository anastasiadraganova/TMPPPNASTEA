using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Domain.Entities;

/// <summary>
/// Сущность проекта. Реализует IPrototype для паттерна Prototype — 
/// позволяет клонировать шаблоны проектов для быстрого создания новых.
/// </summary>
public class Project : IPrototype<Project>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Open;
    public ProjectType Type { get; set; } = ProjectType.FixedPrice;
    public Guid CustomerId { get; set; }
    public Guid? FreelancerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }
    public List<string> RequiredSkills { get; set; } = new();

    // Navigation
    public User? Customer { get; set; }
    public User? Freelancer { get; set; }
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    /// <summary>
    /// Паттерн Prototype: Clone() — создаёт глубокую копию проекта (шаблона)
    /// с новым Id и сброшенными статусами.
    /// </summary>
    public Project Clone()
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Title = Title,
            Description = Description,
            Budget = Budget,
            Status = ProjectStatus.Open,
            Type = Type,
            CustomerId = CustomerId,
            FreelancerId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Deadline = Deadline.HasValue
                ? DateTime.UtcNow.Add(Deadline.Value - CreatedAt)
                : null,
            RequiredSkills = new List<string>(RequiredSkills)
        };
    }
}
