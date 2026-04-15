using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Defines a subscriber that reacts to project status changes.
/// </summary>
public interface IProjectObserver
{
    /// <summary>
    /// Handles a project status transition event.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="oldStatus">Previous status value.</param>
    /// <param name="newStatus">New status value.</param>
    /// <param name="triggeredByUserId">Identifier of the user who triggered the change, when available.</param>
    /// <param name="ct">Cancellation token.</param>
    Task OnProjectStatusChangedAsync(
        Guid projectId,
        ProjectStatus oldStatus,
        ProjectStatus newStatus,
        Guid? triggeredByUserId,
        CancellationToken ct = default);
}
