using LegalAssistantApp.Data;
using LegalAssistantApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LegalAssistantApp.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            if (VerifyPassword(password, user.PasswordHash, user.Salt))
                return user;

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
            return null;
        }
    }

    public string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var bytes = Encoding.UTF8.GetBytes(saltedPassword);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public string GenerateSalt()
    {
        var randomBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private bool VerifyPassword(string password, string storedHash, string salt)
    {
        var computedHash = HashPassword(password, salt);
        return storedHash == computedHash;
    }

    public async Task CreateTestUserAsync()
    {
        if (!await _context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var salt = GenerateSalt();
            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = HashPassword("admin123", salt),
                Salt = salt,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}