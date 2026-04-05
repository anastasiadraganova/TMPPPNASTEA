using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarket.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _db;
    public ReviewRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId) =>
        await _db.Reviews.Where(r => r.ToUserId == userId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task AddAsync(Review review)
    {
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
    }
}
