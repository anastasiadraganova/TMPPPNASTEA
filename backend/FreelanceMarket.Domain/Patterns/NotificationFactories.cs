using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Паттерн Factory Method — фабричный метод для создания уведомлений.
/// Абстрактный создатель определяет фабричный метод CreateNotification(),
/// а конкретные создатели порождают нужный тип уведомления.
/// Это позволяет легко добавлять новые типы уведомлений без изменения
/// существующего кода (Open/Closed Principle).
/// </summary>
public interface INotificationFactory
{
    Notification CreateNotification();
}

public class ProposalReceivedNotificationFactory : INotificationFactory
{
    private readonly Guid _customerId;
    private readonly Guid _projectId;
    private readonly string _freelancerName;

    public ProposalReceivedNotificationFactory(Guid customerId, Guid projectId, string freelancerName)
    {
        _customerId = customerId;
        _projectId = projectId;
        _freelancerName = freelancerName;
    }

    public Notification CreateNotification() =>
        new ProposalReceivedNotification(_customerId, _projectId, _freelancerName);
}

public class ProposalAcceptedNotificationFactory : INotificationFactory
{
    private readonly Guid _freelancerId;
    private readonly Guid _proposalId;
    private readonly string _projectTitle;

    public ProposalAcceptedNotificationFactory(Guid freelancerId, Guid proposalId, string projectTitle)
    {
        _freelancerId = freelancerId;
        _proposalId = proposalId;
        _projectTitle = projectTitle;
    }

    public Notification CreateNotification() =>
        new ProposalAcceptedNotification(_freelancerId, _proposalId, _projectTitle);
}

public class ProjectCompletedNotificationFactory : INotificationFactory
{
    private readonly Guid _userId;
    private readonly Guid _projectId;
    private readonly string _projectTitle;

    public ProjectCompletedNotificationFactory(Guid userId, Guid projectId, string projectTitle)
    {
        _userId = userId;
        _projectId = projectId;
        _projectTitle = projectTitle;
    }

    public Notification CreateNotification() =>
        new ProjectCompletedNotification(_userId, _projectId, _projectTitle);
}

public class ProjectCancelledNotificationFactory : INotificationFactory
{
    private readonly Guid _userId;
    private readonly Guid _projectId;
    private readonly string _projectTitle;

    public ProjectCancelledNotificationFactory(Guid userId, Guid projectId, string projectTitle)
    {
        _userId = userId;
        _projectId = projectId;
        _projectTitle = projectTitle;
    }

    public Notification CreateNotification() =>
        new ProjectCancelledNotification(_userId, _projectId, _projectTitle);
}

public class ReviewReceivedNotificationFactory : INotificationFactory
{
    private readonly Guid _userId;
    private readonly string _fromUserName;
    private readonly int _rating;

    public ReviewReceivedNotificationFactory(Guid userId, string fromUserName, int rating)
    {
        _userId = userId;
        _fromUserName = fromUserName;
        _rating = rating;
    }

    public Notification CreateNotification() =>
        new ReviewReceivedNotification(_userId, _fromUserName, _rating);
}
