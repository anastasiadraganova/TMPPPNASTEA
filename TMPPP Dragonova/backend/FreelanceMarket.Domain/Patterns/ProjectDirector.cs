using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Паттерн Director — управляет процессом создания проектов через Builder.
/// Определяет порядок вызовов методов Builder для разных типов проектов,
/// инкапсулируя алгоритм конструирования.
/// </summary>
public class ProjectDirector
{
    /// <summary>
    /// Создаёт проект с фиксированной ценой по типовому сценарию.
    /// </summary>
    public Project BuildFixedPriceProject(string title, string description, decimal budget,
        Guid customerId, DateTime deadline, IEnumerable<string> skills)
    {
        IProjectBuilder builder = new FixedPriceProjectBuilder();
        builder.SetTitle(title)
               .SetDescription(description)
               .SetBudget(budget)
               .SetCustomerId(customerId)
               .SetDeadline(deadline);

        foreach (var skill in skills)
            builder.AddSkill(skill);

        return builder.Build();
    }

    /// <summary>
    /// Создаёт почасовой проект. Budget — ставка в час.
    /// </summary>
    public Project BuildHourlyProject(string title, string description, decimal hourlyRate,
        Guid customerId, IEnumerable<string> skills)
    {
        IProjectBuilder builder = new HourlyProjectBuilder();
        builder.SetTitle(title)
               .SetDescription(description)
               .SetBudget(hourlyRate)
               .SetCustomerId(customerId);

        foreach (var skill in skills)
            builder.AddSkill(skill);

        return builder.Build();
    }

    /// <summary>
    /// Создаёт долгосрочный проект. По умолчанию дедлайн — через 6 месяцев.
    /// </summary>
    public Project BuildLongTermProject(string title, string description, decimal monthlyBudget,
        Guid customerId, IEnumerable<string> skills)
    {
        IProjectBuilder builder = new LongTermProjectBuilder();
        builder.SetTitle(title)
               .SetDescription(description)
               .SetBudget(monthlyBudget)
               .SetCustomerId(customerId);

        foreach (var skill in skills)
            builder.AddSkill(skill);

        return builder.Build();
    }
}
