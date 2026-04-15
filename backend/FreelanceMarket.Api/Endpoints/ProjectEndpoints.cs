using FluentValidation;
using FreelanceMarket.Api.Extensions;
using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Application.Services;
using FreelanceMarket.Application.Services.Observers;
using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Patterns;
using Microsoft.AspNetCore.Authorization;

namespace FreelanceMarket.Api.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        // GET /api/projects/categories/tree — дерево категорий (Composite)
        group.MapGet("/categories/tree", async (IProjectCategoryService categoryService) =>
        {
            var categories = await categoryService.GetTreeAsync();
            return Results.Ok(categories);
        });

        // GET /api/projects — список проектов (публичный)
        group.MapGet("/", async (
            ProjectStatus? status,
            decimal? maxBudget,
            decimal? minBudget,
            string? keyword,
            string? skills,
            ProjectType? type,
            string? sortBy,
            IProjectService svc,
            CancellationToken ct) =>
        {
            if (minBudget.HasValue && maxBudget.HasValue && minBudget > maxBudget)
            {
                return Results.BadRequest(new ApiError("Validation", "minBudget не может быть больше maxBudget"));
            }

            var normalizedSort = string.IsNullOrWhiteSpace(sortBy)
                ? "newest"
                : sortBy.Trim().ToLowerInvariant();

            var allowedSort = new[] { "newest", "budget_asc", "budget_desc", "deadline" };
            if (!allowedSort.Contains(normalizedSort, StringComparer.Ordinal))
            {
                return Results.BadRequest(new ApiError("Validation", "sortBy должен быть одним из: newest, budget_asc, budget_desc, deadline"));
            }

            var requiredSkills = string.IsNullOrWhiteSpace(skills)
                ? null
                : skills
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            var parameters = new ProjectSearchParams(
                status,
                maxBudget,
                minBudget,
                keyword,
                requiredSkills,
                type,
                normalizedSort);

            var projects = await svc.GetAllAsync(parameters, ct);
            return Results.Ok(projects);
        })
        .WithName("GetProjects")
        .WithSummary("Возвращает список проектов с фильтрацией и сортировкой")
        .WithTags("Projects");

        // GET /api/projects/audit-log — журнал смен статусов (только администратор)
        group.MapGet("/audit-log", (AuditLogObserver observer) =>
        {
            return Results.Ok(observer.GetLog());
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
        .WithName("GetProjectAuditLog")
        .WithSummary("Возвращает журнал смен статусов проектов")
        .WithTags("Projects");

        // GET /api/projects/{id}
        group.MapGet("/{id:guid}", async (Guid id, IProjectService svc) =>
        {
            var project = await svc.GetByIdAsync(id);
            return project is null ? Results.NotFound() : Results.Ok(project);
        });

        // POST /api/projects — создание проекта (заказчик)
        group.MapPost("/", async (
            CreateProjectRequest request,
            IValidator<CreateProjectRequest> validator,
            HttpContext ctx,
            IProjectService svc) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors.Select(e => new ApiError("Validation", e.ErrorMessage)));

            var userId = ctx.User.GetUserId();
            var project = await svc.CreateAsync(request, userId);
            return Results.Created($"/api/projects/{project.Id}", project);
        }).RequireAuthorization();

        // POST /api/projects/from-template — создание из шаблона (Prototype)
        group.MapPost("/from-template", async (
            CreateFromTemplateRequest request,
            HttpContext ctx,
            IProjectService svc) =>
        {
            try
            {
                var userId = ctx.User.GetUserId();
                var project = await svc.CreateFromTemplateAsync(request.TemplateId, userId);
                return Results.Created($"/api/projects/{project.Id}", project);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new ApiError("NotFound", ex.Message));
            }
        }).RequireAuthorization();

        // PUT /api/projects/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProjectRequest request,
            HttpContext ctx,
            IProjectService svc) =>
        {
            try
            {
                var userId = ctx.User.GetUserId();
                var project = await svc.UpdateAsync(id, request, userId);
                return Results.Ok(project);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        }).RequireAuthorization();

        // PATCH /api/projects/{id}/assign
        group.MapPatch("/{id:guid}/assign", async (
            Guid id,
            AssignRequest request,
            HttpContext ctx,
            IProjectService svc) =>
        {
            try
            {
                var userId = ctx.User.GetUserId();
                await svc.AssignFreelancerAsync(id, request.FreelancerId, userId);
                return Results.NoContent();
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        }).RequireAuthorization();

        // PATCH /api/projects/{id}/complete
        group.MapPatch("/{id:guid}/complete", async (
            Guid id,
            HttpContext ctx,
            IProjectService svc) =>
        {
            try
            {
                var userId = ctx.User.GetUserId();
                await svc.CompleteAsync(id, userId);
                return Results.NoContent();
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        }).RequireAuthorization();

        // DELETE /api/projects/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            HttpContext ctx,
            IProjectService svc) =>
        {
            try
            {
                var userId = ctx.User.GetUserId();
                await svc.DeleteAsync(id, userId);
                return Results.NoContent();
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        }).RequireAuthorization();
    }
}

public record AssignRequest(Guid FreelancerId);
