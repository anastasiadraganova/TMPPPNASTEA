using FreelanceMarket.Application.Dtos;

namespace FreelanceMarket.Application.Services;

public interface IProposalService
{
    Task<IReadOnlyList<ProposalDto>> GetByProjectIdAsync(Guid projectId);
    Task<IReadOnlyList<ProposalDto>> GetByFreelancerIdAsync(Guid freelancerId);
    Task<ProposalDto> CreateAsync(Guid projectId, CreateProposalRequest request, Guid freelancerId);
    Task AcceptAsync(Guid proposalId, Guid customerId);
    Task RejectAsync(Guid proposalId, Guid customerId);
}
