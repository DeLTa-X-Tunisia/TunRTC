using TunRTC.Server.Data;
using TunRTC.Server.Models;
using System.Security.Cryptography;
using System.Text;

namespace TunRTC.Server.Services;

public static class DatabaseSeeder
{
    public static void SeedTestData(TunRTCContext context)
    {
        // Check if data already exists
        if (context.Users.Any())
        {
            return;
        }

        Console.WriteLine("🌱 Seeding test data...");

        // Create test users
        var users = new[]
        {
            new User
            {
                Username = "demo",
                Email = "demo@tunrtc.com",
                PasswordHash = HashPassword("demo123"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Username = "alice",
                Email = "alice@tunrtc.com",
                PasswordHash = HashPassword("alice123"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Username = "bob",
                Email = "bob@tunrtc.com",
                PasswordHash = HashPassword("bob123"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        Console.WriteLine("✅ Test data seeded successfully!");
        Console.WriteLine("📧 Test accounts:");
        Console.WriteLine("   - demo@tunrtc.com / demo123");
        Console.WriteLine("   - alice@tunrtc.com / alice123");
        Console.WriteLine("   - bob@tunrtc.com / bob123");
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
