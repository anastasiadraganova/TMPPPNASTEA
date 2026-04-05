using FreelanceMarket.Domain.Entities;

namespace FreelanceMarket.Domain.Interfaces;

public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Proposal>> GetByProjectIdAsync(Guid projectId);
    Task<IReadOnlyList<Proposal>> GetByFreelancerIdAsync(Guid freelancerId);
    Task AddAsync(Proposal proposal);
    Task UpdateAsync(Proposal proposal);
}
