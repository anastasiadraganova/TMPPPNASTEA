using FreelanceMarket.Api.Extensions;
using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Application.Services;
using FreelanceMarket.Domain.Interfaces;

namespace FreelanceMarket.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/me", async (HttpContext ctx, IUserRepository userRepo) =>
        {
            var userId = ctx.User.GetUserId();
            var user = await userRepo.GetByIdAsync(userId);
            if (user is null) return Results.NotFound();
            return Results.Ok(new UserDto(user.Id, user.Email, user.Name, user.Role, user.CreatedAt));
        });

        group.MapGet("/{id:guid}", async (Guid id, IUserRepository userRepo) =>
        {
            var user = await userRepo.GetByIdAsync(id);
            if (user is null) return Results.NotFound();
            return Results.Ok(new UserDto(user.Id, user.Email, user.Name, user.Role, user.CreatedAt));
        });

        group.MapGet("/{id:guid}/reviews", async (Guid id, IReviewService reviewService) =>
        {
            var reviews = await reviewService.GetByUserIdAsync(id);
            return Results.Ok(reviews);
        });

        group.MapGet("/{id:guid}/portfolio", async (
            Guid id,
            IUserRepository userRepo,
            IExternalFreelancerProfileAdapter adapter,
            CancellationToken ct) =>
        {
            var user = await userRepo.GetByIdAsync(id);
            if (user is null) return Results.NotFound();

            var profile = await adapter.GetPortfolioProfileAsync(user.Id, user.Email, user.Name, ct);
            if (profile is null) return Results.NotFound();

            var dto = new FreelancerPortfolioDto(
                profile.UserId,
                profile.DisplayName,
                profile.Bio,
                profile.AvatarUrl,
                profile.Skills,
                profile.CompletedProjects,
                profile.ExternalRating,
                profile.Source);

            return Results.Ok(dto);
        });
    }
}
