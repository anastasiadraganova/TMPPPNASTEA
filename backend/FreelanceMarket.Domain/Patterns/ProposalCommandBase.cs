namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Base implementation for proposal commands.
/// </summary>
public abstract class ProposalCommandBase : IProposalCommand
{
    /// <inheritdoc />
    public Guid CommandId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public abstract string CommandType { get; }

    /// <inheritdoc />
    public Guid ProposalId { get; }

    /// <inheritdoc />
    public Guid ExecutedByUserId { get; }

    /// <inheritdoc />
    public DateTime ExecutedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a command base.
    /// </summary>
    /// <param name="proposalId">Target proposal identifier.</param>
    /// <param name="executedByUserId">User executing the command.</param>
    protected ProposalCommandBase(Guid proposalId, Guid executedByUserId)
    {
        ProposalId = proposalId;
        ExecutedByUserId = executedByUserId;
    }

    /// <inheritdoc />
    public abstract Task ExecuteAsync(CancellationToken ct = default);

    /// <inheritdoc />
    public virtual Task UndoAsync(CancellationToken ct = default)
    {
        throw new NotSupportedException("This command cannot be undone after execution");
    }
}
