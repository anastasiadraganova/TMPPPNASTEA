using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IUserRepository _userRepo;

    /// <summary>
    /// Паттерн Director — используется в сервисе для упрощённого создания проектов
    /// разных типов (FixedPrice, Hourly, LongTerm).
    /// </summary>
    private readonly ProjectDirector _director = new();

    /// <summary>
    /// Паттерн Prototype — готовые шаблоны проектов, которые можно клонировать.
    /// </summary>
    private static readonly Dictionary<string, ProjectTemplate> Templates = new()
    {
        ["landing"] = new ProjectTemplate
        {
            TemplateId = "landing",
            Title = "Разработка лендинга",
            DefaultDescription = "Создание одностраничного сайта-лендинга с адаптивным дизайном.",
            DefaultBudget = 500,
            DefaultDeadlineOffset = TimeSpan.FromDays(14),
            Type = ProjectType.FixedPrice,
            DefaultSkills = new() { "HTML", "CSS", "JavaScript" }
        },
        ["mobile-app"] = new ProjectTemplate
        {
            TemplateId = "mobile-app",
            Title = "Мобильное приложение",
            DefaultDescription = "Разработка мобильного приложения для iOS и Android.",
            DefaultBudget = 5000,
            DefaultDeadlineOffset = TimeSpan.FromDays(90),
            Type = ProjectType.FixedPrice,
            DefaultSkills = new() { "React Native", "TypeScript", "REST API" }
        },
        ["tech-support"] = new ProjectTemplate
        {
            TemplateId = "tech-support",
            Title = "Техническая поддержка",
            DefaultDescription = "Ежемесячная техническая поддержка и сопровождение проекта.",
            DefaultBudget = 1500,
            DefaultDeadlineOffset = TimeSpan.FromDays(180),
            Type = ProjectType.LongTerm,
            DefaultSkills = new() { "DevOps", "Linux", "Monitoring" }
        }
    };

    public ProjectService(IProjectRepository projectRepo, IUserRepository userRepo)
    {
        _projectRepo = projectRepo;
        _userRepo = userRepo;
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id)
    {
        var p = await _projectRepo.GetByIdAsync(id);
        return p is null ? null : await ToDtoAsync(p);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(ProjectStatus? status, decimal? maxBudget)
    {
        var projects = await _projectRepo.GetAllAsync(status, maxBudget);
        var dtos = new List<ProjectDto>();
        foreach (var p in projects) dtos.Add(await ToDtoAsync(p));
        return dtos;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetByCustomerIdAsync(Guid customerId)
    {
        var projects = await _projectRepo.GetByCustomerIdAsync(customerId);
        var dtos = new List<ProjectDto>();
        foreach (var p in projects) dtos.Add(await ToDtoAsync(p));
        return dtos;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetByFreelancerIdAsync(Guid freelancerId)
    {
        var projects = await _projectRepo.GetByFreelancerIdAsync(freelancerId);
        var dtos = new List<ProjectDto>();
        foreach (var p in projects) dtos.Add(await ToDtoAsync(p));
        return dtos;
    }

    /// <summary>
    /// Создание проекта через Builder + Director (паттерн).
    /// Director выбирает нужный Builder в зависимости от типа проекта.
    /// </summary>
    public async Task<ProjectDto> CreateAsync(CreateProjectRequest req, Guid customerId)
    {
        var project = req.Type switch
        {
            ProjectType.FixedPrice => _director.BuildFixedPriceProject(
                req.Title, req.Description, req.Budget, customerId,
                req.Deadline ?? DateTime.UtcNow.AddDays(30), req.RequiredSkills),

            ProjectType.Hourly => _director.BuildHourlyProject(
                req.Title, req.Description, req.Budget, customerId, req.RequiredSkills),

            ProjectType.LongTerm => _director.BuildLongTermProject(
                req.Title, req.Description, req.Budget, customerId, req.RequiredSkills),

            _ => throw new ArgumentException("Неизвестный тип проекта")
        };

        await _projectRepo.AddAsync(project);
        return await ToDtoAsync(project);
    }

    /// <summary>
    /// Паттерн Prototype: создание проекта из шаблона через Clone().
    /// </summary>
    public async Task<ProjectDto> CreateFromTemplateAsync(string templateId, Guid customerId)
    {
        if (!Templates.TryGetValue(templateId, out var template))
            throw new KeyNotFoundException($"Шаблон «{templateId}» не найден");

        // Prototype: клонируем шаблон
        var project = template.Clone();
        project.CustomerId = customerId;

        await _projectRepo.AddAsync(project);
        return await ToDtoAsync(project);
    }

    public async Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest req, Guid customerId)
    {
        var project = await _projectRepo.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != customerId)
            throw new UnauthorizedAccessException("Вы не являетесь владельцем проекта");

        project.Title = req.Title;
        project.Description = req.Description;
        project.Budget = req.Budget;
        project.Deadline = req.Deadline;
        project.RequiredSkills = req.RequiredSkills;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(project);
        return await ToDtoAsync(project);
    }

    public async Task AssignFreelancerAsync(Guid projectId, Guid freelancerId, Guid customerId)
    {
        var project = await _projectRepo.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != customerId)
            throw new UnauthorizedAccessException("Вы не являетесь владельцем проекта");

        project.FreelancerId = freelancerId;
        project.Status = ProjectStatus.InProgress;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(project);
    }

    public async Task CompleteAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepo.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != userId && project.FreelancerId != userId)
            throw new UnauthorizedAccessException("Нет доступа");

        project.Status = ProjectStatus.Completed;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(project);
    }

    public async Task DeleteAsync(Guid projectId, Guid customerId)
    {
        var project = await _projectRepo.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != customerId)
            throw new UnauthorizedAccessException("Вы не являетесь владельцем проекта");

        await _projectRepo.DeleteAsync(projectId);
    }

    private async Task<ProjectDto> ToDtoAsync(Project p)
    {
        var customer = await _userRepo.GetByIdAsync(p.CustomerId);
        var freelancer = p.FreelancerId.HasValue
            ? await _userRepo.GetByIdAsync(p.FreelancerId.Value)
            : null;

        return new ProjectDto(
            p.Id, p.Title, p.Description, p.Budget, p.Status, p.Type,
            p.CustomerId, customer?.Name ?? "Unknown",
            p.FreelancerId, freelancer?.Name,
            p.CreatedAt, p.Deadline, p.RequiredSkills,
            p.Proposals?.Count ?? 0);
    }
}
