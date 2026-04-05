using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Паттерн Prototype — шаблон проекта, который можно клонировать для быстрого
/// создания нового проекта с предзаполненными полями.
/// Например: «Разработка лендинга», «Мобильное приложение», «Тех. поддержка» и т.д.
/// </summary>
public class ProjectTemplate : IPrototype<Project>
{
    public string TemplateId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DefaultDescription { get; set; } = string.Empty;
    public decimal DefaultBudget { get; set; }
    public TimeSpan DefaultDeadlineOffset { get; set; } = TimeSpan.FromDays(14);
    public ProjectType Type { get; set; } = ProjectType.FixedPrice;
    public List<string> DefaultSkills { get; set; } = new();

    /// <summary>
    /// Clone() — создаёт новый Project на основе данного шаблона (Prototype).
    /// </summary>
    public Project Clone()
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Title = Title,
            Description = DefaultDescription,
            Budget = DefaultBudget,
            Status = ProjectStatus.Open,
            Type = Type,
            Deadline = DateTime.UtcNow.Add(DefaultDeadlineOffset),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RequiredSkills = new List<string>(DefaultSkills)
        };
    }
}
