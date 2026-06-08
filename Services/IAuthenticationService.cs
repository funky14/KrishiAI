using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface IAuthenticationService
{
    /// <summary>
    /// Register a new user with email and password
    /// </summary>
    Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName, string? phoneNumber = null);

    /// <summary>
    /// Login user with email and password
    /// </summary>
    Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password);

    /// <summary>
    /// Get the currently authenticated user
    /// </summary>
    Task<User?> GetCurrentUserAsync();

    /// <summary>
    /// Check if a user is currently authenticated
    /// </summary>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Logout the current user
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Verify if email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Request password reset for a given email
    /// </summary>
    Task<(bool Success, string Message)> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Reset password using reset token
    /// </summary>
    Task<(bool Success, string Message)> ResetPasswordAsync(string email, string resetToken, string newPassword);
}
