using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Application.Services;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<ProjectDto>> GetAllAsync(ProjectStatus? status = null, decimal? maxBudget = null);
    Task<IReadOnlyList<ProjectDto>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<ProjectDto>> GetByFreelancerIdAsync(Guid freelancerId);
    Task<ProjectDto> CreateAsync(CreateProjectRequest request, Guid customerId);
    Task<ProjectDto> CreateFromTemplateAsync(string templateId, Guid customerId);
    Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest request, Guid customerId);
    Task AssignFreelancerAsync(Guid projectId, Guid freelancerId, Guid customerId);
    Task CompleteAsync(Guid projectId, Guid userId);
    Task DeleteAsync(Guid projectId, Guid customerId);
}
