using System.Collections.Concurrent;
using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Observers;

/// <summary>
/// Captures project status changes into an in-memory audit stream.
/// </summary>
public sealed class AuditLogObserver : IProjectObserver
{
    private readonly ConcurrentQueue<ProjectStatusAuditEntry> _log = new();

    /// <inheritdoc />
    public Task OnProjectStatusChangedAsync(
        Guid projectId,
        ProjectStatus oldStatus,
        ProjectStatus newStatus,
        Guid? triggeredByUserId,
        CancellationToken ct = default)
    {
        _log.Enqueue(new ProjectStatusAuditEntry(
            projectId,
            oldStatus,
            newStatus,
            DateTime.UtcNow,
            triggeredByUserId));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns audit entries in insertion order.
    /// </summary>
    /// <returns>Immutable log snapshot.</returns>
    public IReadOnlyList<ProjectStatusAuditEntry> GetLog()
    {
        return _log.ToList().AsReadOnly();
    }
}

/// <summary>
/// Represents an audit entry for project status transitions.
/// </summary>
/// <param name="ProjectId">Project identifier.</param>
/// <param name="OldStatus">Previous status value.</param>
/// <param name="NewStatus">New status value.</param>
/// <param name="ChangedAt">UTC timestamp of the change.</param>
/// <param name="TriggeredByUserId">User identifier that triggered the transition, when known.</param>
public sealed record ProjectStatusAuditEntry(
    Guid ProjectId,
    ProjectStatus OldStatus,
    ProjectStatus NewStatus,
    DateTime ChangedAt,
    Guid? TriggeredByUserId);
