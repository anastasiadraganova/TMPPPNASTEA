namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Паттерн Prototype — обобщённый интерфейс для клонирования объектов.
/// Используется для создания новых проектов на основе шаблонов (ProjectTemplate).
/// </summary>
public interface IPrototype<T>
{
    T Clone();
}
