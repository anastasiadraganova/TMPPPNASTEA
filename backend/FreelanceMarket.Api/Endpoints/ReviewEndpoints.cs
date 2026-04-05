using FluentValidation;
using FreelanceMarket.Api.Extensions;
using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Application.Services;

namespace FreelanceMarket.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews").WithTags("Reviews").RequireAuthorization();

        group.MapPost("/", async (
            CreateReviewRequest request,
            IValidator<CreateReviewRequest> validator,
            HttpContext ctx,
            IReviewService svc) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors.Select(e => new ApiError("Validation", e.ErrorMessage)));

            var userId = ctx.User.GetUserId();
            var review = await svc.CreateAsync(request, userId);
            return Results.Created($"/api/reviews/{review.Id}", review);
        });
    }
}
