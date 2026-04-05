using FreelanceMarket.Domain.Entities;

namespace FreelanceMarket.Domain.Interfaces;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Review review);
}
