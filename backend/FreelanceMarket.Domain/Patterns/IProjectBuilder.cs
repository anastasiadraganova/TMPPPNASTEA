using FreelanceMarket.Domain.Entities;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Паттерн Builder — интерфейс строителя проекта.
/// Позволяет пошагово конструировать сложный объект Project,
/// настраивая разные аспекты: бюджет, сроки, навыки, тип.
/// </summary>
public interface IProjectBuilder
{
    IProjectBuilder SetTitle(string title);
    IProjectBuilder SetDescription(string description);
    IProjectBuilder SetBudget(decimal budget);
    IProjectBuilder SetDeadline(DateTime deadline);
    IProjectBuilder SetCustomerId(Guid customerId);
    IProjectBuilder AddSkill(string skill);
    Project Build();
}
