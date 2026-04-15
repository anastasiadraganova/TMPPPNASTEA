using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Thread-safe publisher for project status transition events.
/// </summary>
public sealed class ProjectStatusPublisher : IProjectStatusSubject
{
    private readonly object _sync = new();
    private readonly List<IProjectObserver> _observers = new();

    /// <inheritdoc />
    public void Subscribe(IProjectObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        lock (_sync)
        {
            if (_observers.Contains(observer))
            {
                return;
            }

            _observers.Add(observer);
        }
    }

    /// <inheritdoc />
    public void Unsubscribe(IProjectObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        lock (_sync)
        {
            _observers.Remove(observer);
        }
    }

    /// <inheritdoc />
    public Task NotifyObserversAsync(
        Guid projectId,
        ProjectStatus oldStatus,
        ProjectStatus newStatus,
        Guid? triggeredByUserId,
        CancellationToken ct = default)
    {
        IProjectObserver[] snapshot;
        lock (_sync)
        {
            snapshot = _observers.ToArray();
        }

        if (snapshot.Length == 0)
        {
            return Task.CompletedTask;
        }

        var tasks = snapshot.Select(observer =>
            observer.OnProjectStatusChangedAsync(projectId, oldStatus, newStatus, triggeredByUserId, ct));

        return Task.WhenAll(tasks);
    }
}
