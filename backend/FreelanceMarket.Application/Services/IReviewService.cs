using FreelanceMarket.Application.Dtos;

namespace FreelanceMarket.Application.Services;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetByUserIdAsync(Guid userId);
    Task<ReviewDto> CreateAsync(CreateReviewRequest request, Guid fromUserId);
}
