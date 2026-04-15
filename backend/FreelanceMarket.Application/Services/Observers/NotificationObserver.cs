using System.Collections.Concurrent;
using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceMarket.Application.Services.Observers;

/// <summary>
/// Creates business notifications in response to project status transitions.
/// </summary>
public sealed class NotificationObserver : IProjectObserver
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentBag<Notification> _notifications = new();

    /// <summary>
    /// Creates a notification observer instance.
    /// </summary>
    /// <param name="scopeFactory">Factory for resolving scoped dependencies.</param>
    public NotificationObserver(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public async Task OnProjectStatusChangedAsync(
        Guid projectId,
        ProjectStatus oldStatus,
        ProjectStatus newStatus,
        Guid? triggeredByUserId,
        CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var projectRepo = scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        var project = await projectRepo.GetByIdAsync(projectId);
        if (project is null)
        {
            return;
        }

        INotificationFactory? factory = newStatus switch
        {
            ProjectStatus.Completed =>
                new ProjectCompletedNotificationFactory(project.CustomerId, project.Id, project.Title),
            ProjectStatus.Cancelled when project.FreelancerId.HasValue =>
                new ProjectCancelledNotificationFactory(project.FreelancerId.Value, project.Id, project.Title),
            _ => null
        };

        if (factory is null)
        {
            return;
        }

        var notification = factory.CreateNotification();
        _notifications.Add(notification);
    }

    /// <summary>
    /// Returns created notifications for diagnostics or future persistence wiring.
    /// </summary>
    /// <returns>Notification snapshot.</returns>
    public IReadOnlyList<Notification> GetNotifications()
    {
        return _notifications.ToList().AsReadOnly();
    }
}
