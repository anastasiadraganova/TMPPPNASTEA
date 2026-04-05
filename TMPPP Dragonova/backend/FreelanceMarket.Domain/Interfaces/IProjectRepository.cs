using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Project>> GetAllAsync(ProjectStatus? status = null, decimal? maxBudget = null);
    Task<IReadOnlyList<Project>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<Project>> GetByFreelancerIdAsync(Guid freelancerId);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
}
