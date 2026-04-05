using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IUserRepository _userRepo;

    public ReviewService(IReviewRepository reviewRepo, IUserRepository userRepo)
    {
        _reviewRepo = reviewRepo;
        _userRepo = userRepo;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetByUserIdAsync(Guid userId)
    {
        var reviews = await _reviewRepo.GetByUserIdAsync(userId);
        var dtos = new List<ReviewDto>();
        foreach (var r in reviews)
        {
            var from = await _userRepo.GetByIdAsync(r.FromUserId);
            dtos.Add(new ReviewDto(r.Id, r.FromUserId, from?.Name ?? "Unknown",
                r.ToUserId, r.Rating, r.Comment, r.CreatedAt));
        }
        return dtos;
    }

    public async Task<ReviewDto> CreateAsync(CreateReviewRequest request, Guid fromUserId)
    {
        var review = new Review
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = request.ToUserId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepo.AddAsync(review);

        // Паттерн Factory Method: уведомляем получателя отзыва
        var fromUser = await _userRepo.GetByIdAsync(fromUserId);
        INotificationFactory factory = new ReviewReceivedNotificationFactory(
            request.ToUserId, fromUser?.Name ?? "Пользователь", request.Rating);
        var notification = factory.CreateNotification();
        _ = notification;

        return new ReviewDto(review.Id, fromUserId, fromUser?.Name ?? "Unknown",
            review.ToUserId, review.Rating, review.Comment, review.CreatedAt);
    }
}
