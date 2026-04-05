using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarket.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _db;
    public ProjectRepository(AppDbContext db) => _db = db;

    public async Task<Project?> GetByIdAsync(Guid id) =>
        await _db.Projects.Include(p => p.Proposals).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Project>> GetAllAsync(ProjectStatus? status, decimal? maxBudget)
    {
        var query = _db.Projects.Include(p => p.Proposals).AsQueryable();
        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        if (maxBudget.HasValue) query = query.Where(p => p.Budget <= maxBudget.Value);
        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<IReadOnlyList<Project>> GetByCustomerIdAsync(Guid customerId) =>
        await _db.Projects.Include(p => p.Proposals)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<IReadOnlyList<Project>> GetByFreelancerIdAsync(Guid freelancerId) =>
        await _db.Projects.Include(p => p.Proposals)
            .Where(p => p.FreelancerId == freelancerId)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task AddAsync(Project project)
    {
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        _db.Projects.Update(project);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project is not null)
        {
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
        }
    }
}
