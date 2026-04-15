using FluentValidation;
using FreelanceMarket.Api.Extensions;
using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Application.Services;
using FreelanceMarket.Application.Services.Commands;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Api.Endpoints;

public static class ProposalEndpoints
{
    public static void MapProposalEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/projects/{projectId}/proposals
        app.MapGet("/api/projects/{projectId:guid}/proposals", async (
            Guid projectId,
            IProposalService svc) =>
        {
            var proposals = await svc.GetByProjectIdAsync(projectId);
            return Results.Ok(proposals);
        }).WithTags("Proposals").RequireAuthorization();

        // POST /api/projects/{projectId}/proposals
        app.MapPost("/api/projects/{projectId:guid}/proposals", async (
            Guid projectId,
            CreateProposalRequest request,
            IValidator<CreateProposalRequest> validator,
            HttpContext ctx,
            IProposalService svc) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors.Select(e => new ApiError("Validation", e.ErrorMessage)));

            try
            {
                var userId = ctx.User.GetUserId();
                var proposal = await svc.CreateAsync(projectId, request, userId);
                return Results.Created($"/api/proposals/{proposal.Id}", proposal);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new ApiError("Error", ex.Message)); }
        }).WithTags("Proposals").RequireAuthorization();

        // PATCH /api/proposals/{id}/accept
        app.MapPatch("/api/proposals/{id:guid}/accept", async (
            Guid id,
            HttpContext ctx,
            IProposalRepository proposalRepo,
            IProjectRepository projectRepo,
            IProjectStatusSubject publisher,
            ProposalCommandInvoker invoker,
            CancellationToken ct) =>
        {
            try
            {
                var userId = ctx.User.GetUserId();
                var command = new AcceptProposalCommand(id, userId, proposalRepo, projectRepo, publisher);
                var commandId = await invoker.InvokeAsync(command, ct);
                return Results.Ok(new { commandId });
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new ApiError("Error", ex.Message)); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        })
        .WithName("AcceptProposal")
        .WithSummary("Принимает отклик и возвращает идентификатор команды")
        .WithTags("Proposals")
        .RequireAuthorization();

        // PATCH /api/proposals/{id}/reject
        app.MapPatch("/api/proposals/{id:guid}/reject", async (
            Guid id,
            HttpContext ctx,
            IProposalRepository proposalRepo,
            IProjectRepository projectRepo,
            ProposalCommandInvoker invoker,
            CancellationToken ct) =>
        {
            try
            {
                var userId = ctx.User.GetUserId();
                var command = new RejectProposalCommand(id, userId, proposalRepo, projectRepo);
                var commandId = await invoker.InvokeAsync(command, ct);
                return Results.Ok(new { commandId });
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new ApiError("Error", ex.Message)); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        })
        .WithName("RejectProposal")
        .WithSummary("Отклоняет отклик и возвращает идентификатор команды")
        .WithTags("Proposals")
        .RequireAuthorization();

        // DELETE /api/proposals/commands/{commandId}
        app.MapDelete("/api/proposals/commands/{commandId:guid}", async (
            Guid commandId,
            HttpContext ctx,
            ProposalCommandInvoker invoker,
            CancellationToken ct) =>
        {
            try
            {
                var userId = ctx.User.GetUserId();
                var role = ctx.User.GetRole();

                var entry = invoker.GetEntry(commandId);
                if (entry is null)
                {
                    return Results.NotFound();
                }

                if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                    && entry.ExecutedByUserId != userId)
                {
                    return Results.Forbid();
                }

                await invoker.UndoAsync(commandId, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new ApiError("Error", ex.Message));
            }
            catch (NotSupportedException ex)
            {
                return Results.BadRequest(new ApiError("Error", ex.Message));
            }
        })
        .WithName("UndoProposalCommand")
        .WithSummary("Отменяет последнее действие по отклику")
        .WithTags("Proposals")
        .RequireAuthorization();

        // GET /api/projects/{projectId}/proposal-history
        app.MapGet("/api/projects/{projectId:guid}/proposal-history", async (
            Guid projectId,
            HttpContext ctx,
            IProjectRepository projectRepo,
            ProposalCommandInvoker invoker) =>
        {
            var project = await projectRepo.GetByIdAsync(projectId);
            if (project is null)
            {
                return Results.NotFound();
            }

            var userId = ctx.User.GetUserId();
            var role = ctx.User.GetRole();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                && project.CustomerId != userId)
            {
                return Results.Forbid();
            }

            return Results.Ok(invoker.GetHistory(projectId));
        })
        .WithName("GetProposalHistory")
        .WithSummary("Возвращает историю действий по откликам проекта")
        .WithTags("Proposals")
        .RequireAuthorization();
    }
}
