using System.Collections.Concurrent;

namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Паттерн Singleton — глобальный менеджер состояния активных сессий/кэша.
/// 
/// Зачем Singleton здесь: единственный экземпляр хранит в памяти кэш активных
/// пользователей и текущих сессий. Это позволяет любому сервису получить
/// доступ к состоянию без повторного обращения к БД.
/// 
/// Потокобезопасность: используем ConcurrentDictionary и Lazy<T> для
/// гарантии единственности экземпляра даже в многопоточной среде.
/// 
/// Ограничения: данные живут в памяти процесса — при перезапуске теряются.
/// Для production рекомендуется Redis/распределённый кэш, но Singleton
/// по‑прежнему может использоваться как фасад к нему.
/// </summary>
public sealed class SessionStateManager
{
    private static readonly Lazy<SessionStateManager> _instance =
        new(() => new SessionStateManager(), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Единственная точка доступа к экземпляру (Singleton).
    /// </summary>
    public static SessionStateManager Instance => _instance.Value;

    private readonly ConcurrentDictionary<Guid, UserSession> _sessions = new();

    // Приватный конструктор — паттерн Singleton запрещает внешнее создание.
    private SessionStateManager() { }

    public void AddSession(Guid userId, string token)
    {
        var session = new UserSession(userId, token);
        _sessions.AddOrUpdate(userId, session, (_, _) => session);
    }

    public void RemoveSession(Guid userId)
    {
        _sessions.TryRemove(userId, out _);
    }

    public UserSession? GetSession(Guid userId)
    {
        _sessions.TryGetValue(userId, out var session);
        return session;
    }

    public bool IsOnline(Guid userId) => _sessions.ContainsKey(userId);

    public int ActiveCount => _sessions.Count;

    public IReadOnlyCollection<Guid> GetOnlineUserIds() =>
        _sessions.Keys.ToList().AsReadOnly();
}

public record UserSession(Guid UserId, string Token, DateTime LoggedInAt = default)
{
    public DateTime LoggedInAt { get; init; } = LoggedInAt == default ? DateTime.UtcNow : LoggedInAt;
}
