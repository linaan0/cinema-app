namespace CinemaApp.Auth.Domain.Dto;

public record RegisterRequest(string Email, string Password, string Name);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, string UserId, string Email, string Name, string Role);

public record UserDto(string Id, string Email, string Name, string Role);
