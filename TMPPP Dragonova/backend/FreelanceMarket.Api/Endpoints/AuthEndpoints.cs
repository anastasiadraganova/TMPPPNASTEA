using FluentValidation;
using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Application.Services;

namespace FreelanceMarket.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            IValidator<RegisterRequest> validator,
            IAuthService authService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors.Select(e => new ApiError("Validation", e.ErrorMessage)));

            try
            {
                var result = await authService.RegisterAsync(request);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new ApiError("Conflict", ex.Message));
            }
        });

        group.MapPost("/login", async (
            LoginRequest request,
            IValidator<LoginRequest> validator,
            IAuthService authService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors.Select(e => new ApiError("Validation", e.ErrorMessage)));

            try
            {
                var result = await authService.LoginAsync(request);
                return Results.Ok(result);
            }
            catch (InvalidOperationException)
            {
                return Results.Json(
                    new ApiError("Unauthorized", "Неверный email или пароль"),
                    statusCode: 401);
            }
        });
    }
}
