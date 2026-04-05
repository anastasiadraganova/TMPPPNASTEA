using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwt;

    public AuthService(IUserRepository userRepo, IJwtService jwt)
    {
        _userRepo = userRepo;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("Пользователь с таким email уже существует");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);

        var token = _jwt.GenerateToken(user.Id, user.Email, user.Role.ToString());

        // Паттерн Singleton: регистрируем сессию нового пользователя
        SessionStateManager.Instance.AddSession(user.Id, token);

        return new AuthResponse(token, ToDto(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("Неверный email или пароль");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Неверный email или пароль");

        var token = _jwt.GenerateToken(user.Id, user.Email, user.Role.ToString());

        // Паттерн Singleton: обновляем сессию при входе
        SessionStateManager.Instance.AddSession(user.Id, token);

        return new AuthResponse(token, ToDto(user));
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Email, u.Name, u.Role, u.CreatedAt);
}
