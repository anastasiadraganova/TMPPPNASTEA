using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Entities;

public class Proposal
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid FreelancerId { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public decimal BidAmount { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Project? Project { get; set; }
    public User? Freelancer { get; set; }
}
