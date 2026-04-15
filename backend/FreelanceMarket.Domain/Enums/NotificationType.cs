namespace FreelanceMarket.Domain.Enums;

/// <summary>
/// Типы уведомлений, используется паттерном Factory Method для создания разных уведомлений.
/// </summary>
public enum NotificationType
{
    ProposalReceived,
    ProposalAccepted,
    ProposalRejected,
    ProjectCompleted,
    ProjectCancelled,
    ReviewReceived
}
