using Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class PasswordHasher : Application.Interfaces.IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _hasher = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(
            new object(),
            password);
    }

    public bool VerifyPassword(
        string password,
        string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(
            new object(),
            passwordHash,
            password);

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}