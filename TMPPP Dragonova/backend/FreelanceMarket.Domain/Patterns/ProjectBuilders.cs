using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Паттерн Builder — конкретный строитель для создания проекта с фиксированной ценой.
/// </summary>
public class FixedPriceProjectBuilder : IProjectBuilder
{
    private readonly Project _project = new() { Type = ProjectType.FixedPrice };

    public IProjectBuilder SetTitle(string title) { _project.Title = title; return this; }
    public IProjectBuilder SetDescription(string description) { _project.Description = description; return this; }
    public IProjectBuilder SetBudget(decimal budget) { _project.Budget = budget; return this; }
    public IProjectBuilder SetDeadline(DateTime deadline) { _project.Deadline = deadline; return this; }
    public IProjectBuilder SetCustomerId(Guid customerId) { _project.CustomerId = customerId; return this; }
    public IProjectBuilder AddSkill(string skill) { _project.RequiredSkills.Add(skill); return this; }

    public Project Build()
    {
        _project.Id = Guid.NewGuid();
        _project.CreatedAt = DateTime.UtcNow;
        _project.UpdatedAt = DateTime.UtcNow;
        _project.Status = ProjectStatus.Open;
        return _project;
    }
}

/// <summary>
/// Паттерн Builder — конкретный строитель для почасового проекта.
/// </summary>
public class HourlyProjectBuilder : IProjectBuilder
{
    private readonly Project _project = new() { Type = ProjectType.Hourly };

    public IProjectBuilder SetTitle(string title) { _project.Title = title; return this; }
    public IProjectBuilder SetDescription(string description) { _project.Description = description; return this; }
    public IProjectBuilder SetBudget(decimal budget) { _project.Budget = budget; return this; }
    public IProjectBuilder SetDeadline(DateTime deadline) { _project.Deadline = deadline; return this; }
    public IProjectBuilder SetCustomerId(Guid customerId) { _project.CustomerId = customerId; return this; }
    public IProjectBuilder AddSkill(string skill) { _project.RequiredSkills.Add(skill); return this; }

    public Project Build()
    {
        _project.Id = Guid.NewGuid();
        _project.CreatedAt = DateTime.UtcNow;
        _project.UpdatedAt = DateTime.UtcNow;
        _project.Status = ProjectStatus.Open;
        return _project;
    }
}

/// <summary>
/// Паттерн Builder — конкретный строитель для долгосрочного проекта.
/// Устанавливает дедлайн по умолчанию на 6 месяцев.
/// </summary>
public class LongTermProjectBuilder : IProjectBuilder
{
    private readonly Project _project = new() { Type = ProjectType.LongTerm };

    public IProjectBuilder SetTitle(string title) { _project.Title = title; return this; }
    public IProjectBuilder SetDescription(string description) { _project.Description = description; return this; }
    public IProjectBuilder SetBudget(decimal budget) { _project.Budget = budget; return this; }
    public IProjectBuilder SetDeadline(DateTime deadline) { _project.Deadline = deadline; return this; }
    public IProjectBuilder SetCustomerId(Guid customerId) { _project.CustomerId = customerId; return this; }
    public IProjectBuilder AddSkill(string skill) { _project.RequiredSkills.Add(skill); return this; }

    public Project Build()
    {
        _project.Id = Guid.NewGuid();
        _project.CreatedAt = DateTime.UtcNow;
        _project.UpdatedAt = DateTime.UtcNow;
        _project.Status = ProjectStatus.Open;
        _project.Deadline ??= DateTime.UtcNow.AddMonths(6);
        return _project;
    }
}
