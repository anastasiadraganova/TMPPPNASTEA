using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarket.Infrastructure.Repositories;

public class ProposalRepository : IProposalRepository
{
    private readonly AppDbContext _db;
    public ProposalRepository(AppDbContext db) => _db = db;

    public async Task<Proposal?> GetByIdAsync(Guid id) =>
        await _db.Proposals.FindAsync(id);

    public async Task<IReadOnlyList<Proposal>> GetByProjectIdAsync(Guid projectId) =>
        await _db.Proposals.Where(p => p.ProjectId == projectId)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<IReadOnlyList<Proposal>> GetByFreelancerIdAsync(Guid freelancerId) =>
        await _db.Proposals.Where(p => p.FreelancerId == freelancerId)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task AddAsync(Proposal proposal)
    {
        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Proposal proposal)
    {
        _db.Proposals.Update(proposal);
        await _db.SaveChangesAsync();
    }
}
