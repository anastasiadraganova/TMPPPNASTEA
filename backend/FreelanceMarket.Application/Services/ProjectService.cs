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
    private readonly ProjectFilterContext _filterContext;
    private readonly IProjectStatusSubject _publisher;

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

    public ProjectService(
        IProjectRepository projectRepo,
        IUserRepository userRepo,
        ProjectFilterContext filterContext,
        IProjectStatusSubject publisher)
    {
        _projectRepo = projectRepo;
        _userRepo = userRepo;
        _filterContext = filterContext;
        _publisher = publisher;
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _projectRepo.GetByIdAsync(id);
        return p is null ? null : await ToDtoAsync(p, ct);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(ProjectStatus? status, decimal? maxBudget)
    {
        var searchParams = new ProjectSearchParams(
            status,
            maxBudget,
            null,
            null,
            null,
            null,
            "newest");

        return await GetAllAsync(searchParams);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(ProjectSearchParams parameters, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var projects = await _projectRepo.GetAllAsync();
        var query = _filterContext.ApplyAll(projects.AsQueryable(), parameters);

        var dtos = new List<ProjectDto>();
        foreach (var p in query)
        {
            dtos.Add(await ToDtoAsync(p, ct));
        }

        return dtos;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var projects = await _projectRepo.GetByCustomerIdAsync(customerId);
        var dtos = new List<ProjectDto>();
        foreach (var p in projects)
        {
            dtos.Add(await ToDtoAsync(p, ct));
        }

        return dtos;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetByFreelancerIdAsync(Guid freelancerId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var projects = await _projectRepo.GetByFreelancerIdAsync(freelancerId);
        var dtos = new List<ProjectDto>();
        foreach (var p in projects)
        {
            dtos.Add(await ToDtoAsync(p, ct));
        }

        return dtos;
    }

    /// <summary>
    /// Создание проекта через Builder + Director (паттерн).
    /// Director выбирает нужный Builder в зависимости от типа проекта.
    /// </summary>
    public async Task<ProjectDto> CreateAsync(CreateProjectRequest req, Guid customerId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

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
        return await ToDtoAsync(project, ct);
    }

    /// <summary>
    /// Паттерн Prototype: создание проекта из шаблона через Clone().
    /// </summary>
    public async Task<ProjectDto> CreateFromTemplateAsync(string templateId, Guid customerId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!Templates.TryGetValue(templateId, out var template))
            throw new KeyNotFoundException($"Шаблон «{templateId}» не найден");

        // Prototype: клонируем шаблон
        var project = template.Clone();
        project.CustomerId = customerId;

        await _projectRepo.AddAsync(project);
        return await ToDtoAsync(project, ct);
    }

    public async Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest req, Guid customerId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

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
        return await ToDtoAsync(project, ct);
    }

    public async Task AssignFreelancerAsync(
        Guid projectId,
        Guid freelancerId,
        Guid customerId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var project = await _projectRepo.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != customerId)
            throw new UnauthorizedAccessException("Вы не являетесь владельцем проекта");

        var oldStatus = project.Status;
        project.FreelancerId = freelancerId;
        project.Status = ProjectStatus.InProgress;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(project);

        if (oldStatus != project.Status)
        {
            await _publisher.NotifyObserversAsync(
                project.Id,
                oldStatus,
                project.Status,
                customerId,
                ct);
        }
    }

    public async Task CompleteAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var project = await _projectRepo.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != userId && project.FreelancerId != userId)
            throw new UnauthorizedAccessException("Нет доступа");

        var oldStatus = project.Status;
        project.Status = ProjectStatus.Completed;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(project);

        if (oldStatus != project.Status)
        {
            await _publisher.NotifyObserversAsync(
                project.Id,
                oldStatus,
                project.Status,
                userId,
                ct);
        }
    }

    public async Task DeleteAsync(Guid projectId, Guid customerId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var project = await _projectRepo.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != customerId)
            throw new UnauthorizedAccessException("Вы не являетесь владельцем проекта");

        if (project.Status == ProjectStatus.InProgress)
        {
            var oldStatus = project.Status;
            project.Status = ProjectStatus.Cancelled;
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepo.UpdateAsync(project);
            await _publisher.NotifyObserversAsync(
                project.Id,
                oldStatus,
                project.Status,
                customerId,
                ct);
        }

        await _projectRepo.DeleteAsync(projectId);
    }

    private async Task<ProjectDto> ToDtoAsync(Project p, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

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
