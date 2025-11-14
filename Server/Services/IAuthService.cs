using TunRTC.Server.Models;
using TunRTC.Server.Models.DTOs;

namespace TunRTC.Server.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
    string GenerateJwtToken(User user);
}
