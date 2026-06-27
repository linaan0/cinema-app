using CinemaApp.Auth.Domain.Dto;
using CinemaApp.Auth.Domain.Models;
using CinemaApp.Auth.Repository;

namespace CinemaApp.Auth.Service;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = "Customer"
        };

        await _userRepository.InsertAsync(user);

        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Email, user.Name, user.Role);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Email, user.Name, user.Role);
    }
}
