using FluentValidation;
using FreelanceMarket.Api.Extensions;
using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Application.Services;
using FreelanceMarket.Domain.Enums;

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
        group.MapGet("/", async (ProjectStatus? status, decimal? maxBudget, IProjectService svc) =>
        {
            var projects = await svc.GetAllAsync(status, maxBudget);
            return Results.Ok(projects);
        });

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
