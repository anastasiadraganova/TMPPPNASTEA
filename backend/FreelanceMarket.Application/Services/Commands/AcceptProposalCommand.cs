using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Commands;

/// <summary>
/// Accepts a proposal, assigns a freelancer to the project, and rejects competing pending proposals.
/// </summary>
public sealed class AcceptProposalCommand : ProposalCommandBase
{
    private readonly IProposalRepository _proposalRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectStatusSubject _publisher;

    private ProjectStatus? _previousProjectStatus;
    private Guid? _previousFreelancerId;
    private readonly List<Guid> _rejectedProposalIds = new();
    private bool _executed;

    /// <summary>
    /// Gets the related project identifier after execution.
    /// </summary>
    public Guid ProjectId { get; private set; }

    /// <inheritdoc />
    public override string CommandType => "AcceptProposal";

    /// <summary>
    /// Creates an accept proposal command.
    /// </summary>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <param name="executedByUserId">Executing customer identifier.</param>
    /// <param name="proposalRepo">Proposal repository.</param>
    /// <param name="projectRepo">Project repository.</param>
    /// <param name="publisher">Project status publisher.</param>
    public AcceptProposalCommand(
        Guid proposalId,
        Guid executedByUserId,
        IProposalRepository proposalRepo,
        IProjectRepository projectRepo,
        IProjectStatusSubject publisher)
        : base(proposalId, executedByUserId)
    {
        _proposalRepo = proposalRepo;
        _projectRepo = projectRepo;
        _publisher = publisher;
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
            throw new InvalidOperationException("Можно принять только отклик со статусом Pending");
        }

        _previousProjectStatus = project.Status;
        _previousFreelancerId = project.FreelancerId;
        ProjectId = project.Id;

        proposal.Status = ProposalStatus.Accepted;
        await _proposalRepo.UpdateAsync(proposal);

        var projectProposals = await _proposalRepo.GetByProjectIdAsync(project.Id);
        foreach (var otherProposal in projectProposals)
        {
            if (otherProposal.Id == proposal.Id)
            {
                continue;
            }

            if (otherProposal.Status != ProposalStatus.Pending)
            {
                continue;
            }

            otherProposal.Status = ProposalStatus.Rejected;
            _rejectedProposalIds.Add(otherProposal.Id);
            await _proposalRepo.UpdateAsync(otherProposal);
        }

        project.FreelancerId = proposal.FreelancerId;
        project.Status = ProjectStatus.InProgress;
        project.UpdatedAt = DateTime.UtcNow;
        await _projectRepo.UpdateAsync(project);

        await _publisher.NotifyObserversAsync(
            project.Id,
            _previousProjectStatus.Value,
            project.Status,
            ExecutedByUserId,
            ct);

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

        var project = await _projectRepo.GetByIdAsync(proposal.ProjectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.Status == ProjectStatus.Completed)
        {
            throw new NotSupportedException("Нельзя отменить принятие отклика после завершения проекта");
        }

        var currentStatus = project.Status;

        proposal.Status = ProposalStatus.Pending;
        await _proposalRepo.UpdateAsync(proposal);

        foreach (var rejectedProposalId in _rejectedProposalIds)
        {
            var rejectedProposal = await _proposalRepo.GetByIdAsync(rejectedProposalId);
            if (rejectedProposal is null || rejectedProposal.Status != ProposalStatus.Rejected)
            {
                continue;
            }

            rejectedProposal.Status = ProposalStatus.Pending;
            await _proposalRepo.UpdateAsync(rejectedProposal);
        }

        project.FreelancerId = _previousFreelancerId;
        project.Status = _previousProjectStatus ?? ProjectStatus.Open;
        project.UpdatedAt = DateTime.UtcNow;
        await _projectRepo.UpdateAsync(project);

        if (currentStatus != project.Status)
        {
            await _publisher.NotifyObserversAsync(
                project.Id,
                currentStatus,
                project.Status,
                ExecutedByUserId,
                ct);
        }
    }
}
