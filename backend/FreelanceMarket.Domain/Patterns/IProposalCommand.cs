namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Defines an executable proposal action with optional undo support.
/// </summary>
public interface IProposalCommand
{
    /// <summary>
    /// Unique command identifier.
    /// </summary>
    Guid CommandId { get; }

    /// <summary>
    /// Logical command type name.
    /// </summary>
    string CommandType { get; }

    /// <summary>
    /// Target proposal identifier.
    /// </summary>
    Guid ProposalId { get; }

    /// <summary>
    /// User that executed the command.
    /// </summary>
    Guid ExecutedByUserId { get; }

    /// <summary>
    /// Command execution timestamp in UTC.
    /// </summary>
    DateTime ExecutedAt { get; }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task ExecuteAsync(CancellationToken ct = default);

    /// <summary>
    /// Reverts the command.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task UndoAsync(CancellationToken ct = default);
}
