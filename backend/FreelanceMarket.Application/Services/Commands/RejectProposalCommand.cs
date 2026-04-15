using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Commands;

/// <summary>
/// Rejects a proposal with undo support.
/// </summary>
public sealed class RejectProposalCommand : ProposalCommandBase
{
    private readonly IProposalRepository _proposalRepo;
    private readonly IProjectRepository _projectRepo;

    private ProposalStatus? _previousStatus;
    private bool _executed;

    /// <summary>
    /// Gets the related project identifier after execution.
    /// </summary>
    public Guid ProjectId { get; private set; }

    /// <inheritdoc />
    public override string CommandType => "RejectProposal";

    /// <summary>
    /// Creates a reject proposal command.
    /// </summary>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <param name="executedByUserId">Executing customer identifier.</param>
    /// <param name="proposalRepo">Proposal repository.</param>
    /// <param name="projectRepo">Project repository.</param>
    public RejectProposalCommand(
        Guid proposalId,
        Guid executedByUserId,
        IProposalRepository proposalRepo,
        IProjectRepository projectRepo)
        : base(proposalId, executedByUserId)
    {
        _proposalRepo = proposalRepo;
        _projectRepo = projectRepo;
    }

    /// <inheritdoc />
    public override async Task ExecuteAsync(CancellationToken ct = default)
    {
        var proposal = await _proposalRepo.GetByIdAsync(ProposalId)
            ?? throw new KeyNotFoundException("Отклик не найден");

        var project = await _projectRepo.GetByIdAsync(proposal.ProjectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != ExecutedByUserId)
        {
            throw new UnauthorizedAccessException("Вы не являетесь владельцем проекта");
        }

        if (proposal.Status != ProposalStatus.Pending)
        {
            throw new InvalidOperationException("Можно отклонить только отклик со статусом Pending");
        }

        _previousStatus = proposal.Status;
        ProjectId = proposal.ProjectId;

        proposal.Status = ProposalStatus.Rejected;
        await _proposalRepo.UpdateAsync(proposal);

        _executed = true;
    }

    /// <inheritdoc />
    public override async Task UndoAsync(CancellationToken ct = default)
    {
        if (!_executed)
        {
            throw new InvalidOperationException("Команда ещё не была выполнена");
        }

        var proposal = await _proposalRepo.GetByIdAsync(ProposalId)
            ?? throw new KeyNotFoundException("Отклик не найден");

        proposal.Status = _previousStatus ?? ProposalStatus.Pending;
        await _proposalRepo.UpdateAsync(proposal);
    }
}
