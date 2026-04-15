using System.Collections.Concurrent;
using FreelanceMarket.Application.Services.Commands;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services;

/// <summary>
/// Invokes proposal commands and stores execution history for undo and auditing.
/// </summary>
public sealed class ProposalCommandInvoker
{
    private readonly ConcurrentDictionary<Guid, StoredCommand> _history = new();

    /// <summary>
    /// Executes a command and stores it in history.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Executed command identifier.</returns>
    public async Task<Guid> InvokeAsync(IProposalCommand command, CancellationToken ct = default)
    {
        await command.ExecuteAsync(ct);

        var projectId = command switch
        {
            AcceptProposalCommand accept => accept.ProjectId,
            RejectProposalCommand reject => reject.ProjectId,
            _ => Guid.Empty
        };

        var entry = new CommandHistoryEntry(
            command.CommandId,
            command.CommandType,
            command.ProposalId,
            command.ExecutedByUserId,
            command.ExecutedAt);

        var storedCommand = new StoredCommand(command, projectId, entry);
        _history[command.CommandId] = storedCommand;

        return command.CommandId;
    }

    /// <summary>
    /// Undoes a previously executed command and removes it from history.
    /// </summary>
    /// <param name="commandId">Command identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task UndoAsync(Guid commandId, CancellationToken ct = default)
    {
        if (!_history.TryGetValue(commandId, out var storedCommand))
        {
            throw new KeyNotFoundException("Команда не найдена");
        }

        await storedCommand.Command.UndoAsync(ct);
        _history.TryRemove(commandId, out _);
    }

    /// <summary>
    /// Gets command history entries for a project.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <returns>Command history snapshot.</returns>
    public IReadOnlyList<CommandHistoryEntry> GetHistory(Guid projectId)
    {
        return _history.Values
            .Where(item => item.ProjectId == projectId)
            .Select(item => item.Entry)
            .OrderByDescending(item => item.ExecutedAt)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets a command history entry by command identifier.
    /// </summary>
    /// <param name="commandId">Command identifier.</param>
    /// <returns>History entry, if found.</returns>
    public CommandHistoryEntry? GetEntry(Guid commandId)
    {
        return _history.TryGetValue(commandId, out var storedCommand)
            ? storedCommand.Entry
            : null;
    }

    private sealed record StoredCommand(
        IProposalCommand Command,
        Guid ProjectId,
        CommandHistoryEntry Entry);
}

/// <summary>
/// Represents executed command metadata.
/// </summary>
/// <param name="CommandId">Command identifier.</param>
/// <param name="CommandType">Command logical type.</param>
/// <param name="ProposalId">Target proposal identifier.</param>
/// <param name="ExecutedByUserId">User who executed the command.</param>
/// <param name="ExecutedAt">Execution timestamp in UTC.</param>
public sealed record CommandHistoryEntry(
    Guid CommandId,
    string CommandType,
    Guid ProposalId,
    Guid ExecutedByUserId,
    DateTime ExecutedAt);
