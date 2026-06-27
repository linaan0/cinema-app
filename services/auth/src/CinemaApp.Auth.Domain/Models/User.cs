using CinemaApp.Auth.Domain.Common;

namespace CinemaApp.Auth.Domain.Models;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // "Customer" or "Admin"
    public string Role { get; set; } = "Customer";
}
