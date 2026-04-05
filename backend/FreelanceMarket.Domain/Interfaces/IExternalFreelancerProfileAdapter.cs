using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Domain.Interfaces;

/// <summary>
/// Adapter: единый интерфейс для получения внешнего профиля фрилансера
/// и преобразования в внутренний формат приложения.
/// </summary>
public interface IExternalFreelancerProfileAdapter
{
    Task<FreelancerPortfolioProfile?> GetPortfolioProfileAsync(
        Guid userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default);
}
