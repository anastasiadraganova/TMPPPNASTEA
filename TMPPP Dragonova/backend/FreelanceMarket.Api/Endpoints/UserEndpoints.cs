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
    }
}
