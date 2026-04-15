using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Паттерн Factory Method — уведомления.
/// Базовый класс уведомления; конкретные фабрики создают разные типы.
/// </summary>
public abstract class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}

public class ProposalReceivedNotification : Notification
{
    public Guid ProjectId { get; set; }
    public ProposalReceivedNotification(Guid userId, Guid projectId, string freelancerName)
    {
        UserId = userId;
        ProjectId = projectId;
        Type = NotificationType.ProposalReceived;
        Message = $"Новый отклик на ваш проект от {freelancerName}";
    }
}

public class ProposalAcceptedNotification : Notification
{
    public Guid ProposalId { get; set; }
    public ProposalAcceptedNotification(Guid freelancerId, Guid proposalId, string projectTitle)
    {
        UserId = freelancerId;
        ProposalId = proposalId;
        Type = NotificationType.ProposalAccepted;
        Message = $"Ваш отклик на проект «{projectTitle}» принят!";
    }
}

public class ProjectCompletedNotification : Notification
{
    public Guid ProjectId { get; set; }
    public ProjectCompletedNotification(Guid userId, Guid projectId, string projectTitle)
    {
        UserId = userId;
        ProjectId = projectId;
        Type = NotificationType.ProjectCompleted;
        Message = $"Проект «{projectTitle}» завершён";
    }
}

public class ProjectCancelledNotification : Notification
{
    public Guid ProjectId { get; set; }
    public ProjectCancelledNotification(Guid userId, Guid projectId, string projectTitle)
    {
        UserId = userId;
        ProjectId = projectId;
        Type = NotificationType.ProjectCancelled;
        Message = $"Проект «{projectTitle}» был отменён";
    }
}

public class ReviewReceivedNotification : Notification
{
    public ReviewReceivedNotification(Guid userId, string fromUserName, int rating)
    {
        UserId = userId;
        Type = NotificationType.ReviewReceived;
        Message = $"Вы получили отзыв от {fromUserName} — оценка {rating}/5";
    }
}
