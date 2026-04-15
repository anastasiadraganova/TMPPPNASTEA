using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Defines a publisher for project status change events.
/// </summary>
public interface IProjectStatusSubject
{
    /// <summary>
    /// Subscribes an observer.
    /// </summary>
    /// <param name="observer">Observer instance.</param>
    void Subscribe(IProjectObserver observer);

    /// <summary>
    /// Unsubscribes an observer.
    /// </summary>
    /// <param name="observer">Observer instance.</param>
    void Unsubscribe(IProjectObserver observer);

    /// <summary>
    /// Broadcasts a project status change to all subscribers.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="oldStatus">Previous status value.</param>
    /// <param name="newStatus">New status value.</param>
    /// <param name="triggeredByUserId">Identifier of the user who triggered the change, when available.</param>
    /// <param name="ct">Cancellation token.</param>
    Task NotifyObserversAsync(
        Guid projectId,
        ProjectStatus oldStatus,
        ProjectStatus newStatus,
        Guid? triggeredByUserId,
        CancellationToken ct = default);
}
