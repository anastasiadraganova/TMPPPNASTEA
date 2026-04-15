using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectDto>> GetAllAsync(ProjectStatus? status = null, decimal? maxBudget = null);
    Task<IReadOnlyList<ProjectDto>> GetAllAsync(ProjectSearchParams parameters, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectDto>> GetByFreelancerIdAsync(Guid freelancerId, CancellationToken ct = default);
    Task<ProjectDto> CreateAsync(CreateProjectRequest request, Guid customerId, CancellationToken ct = default);
    Task<ProjectDto> CreateFromTemplateAsync(string templateId, Guid customerId, CancellationToken ct = default);
    Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest request, Guid customerId, CancellationToken ct = default);
    Task AssignFreelancerAsync(Guid projectId, Guid freelancerId, Guid customerId, CancellationToken ct = default);
    Task CompleteAsync(Guid projectId, Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid projectId, Guid customerId, CancellationToken ct = default);
}
