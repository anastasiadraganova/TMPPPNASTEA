using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services.Observers;

/// <summary>
/// Synchronizes in-memory session snapshots with project status changes.
/// </summary>
public sealed class SessionSyncObserver : IProjectObserver
{
    private readonly SessionStateManager _sessionStateManager;

    /// <summary>
    /// Creates a session sync observer.
    /// </summary>
    /// <param name="sessionStateManager">Singleton session state manager instance.</param>
    public SessionSyncObserver(SessionStateManager sessionStateManager)
    {
        _sessionStateManager = sessionStateManager;
    }

    /// <inheritdoc />
    public Task OnProjectStatusChangedAsync(
        Guid projectId,
        ProjectStatus oldStatus,
        ProjectStatus newStatus,
        Guid? triggeredByUserId,
        CancellationToken ct = default)
    {
        _sessionStateManager.UpdateActiveProjectStatus(projectId, newStatus);
        return Task.CompletedTask;
    }
}
