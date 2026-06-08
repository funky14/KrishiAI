using KrishiAI.App.Models;
using System.Diagnostics;

namespace KrishiAI.App.Services;

/// <summary>
/// Authentication Service - Manages user login/registration/password reset
/// Uses SQL Server database directly for all operations
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private User? _currentUser;
    private const string CurrentUserKey = "CurrentUserId";
    private const string CurrentUserEmailKey = "CurrentUserEmail";
    private readonly ISqlServerUserService _userService;

    public AuthenticationService(ISqlServerUserService userService)
    {
        _userService = userService;
        Log("✅ AuthenticationService initialized");
    }

    private static void Log(string message)
    {
        Debug.WriteLine($"[Auth] {message}");
        try
        {
            Console.WriteLine($"[Auth] {message}");
        }
        catch { }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName, string? phoneNumber = null)
    {
        try
        {
            Log($"RegisterAsync: {email}");
            var (success, message, user) = await _userService.RegisterUserAsync(email, password, fullName, phoneNumber);

            if (success && user != null)
            {
                // Store locally for quick access
                Preferences.Set($"user_{email}_fullname", fullName);
                Preferences.Set($"user_{email}_phone", phoneNumber ?? "");
                Log($"✅ Registered: {email}");
            }

            return (success, message);
        }
        catch (Exception ex)
        {
            Log($"❌ RegisterAsync error: {ex.Message}");
            return (false, $"Registration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Login user with email and password
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
    {
        try
        {
            Log($"LoginAsync: {email}");
            var (success, message, user) = await _userService.LoginAsync(email, password);

            if (success && user != null)
            {
                _currentUser = user;
                Preferences.Set(CurrentUserKey, user.Id.ToString());
                Preferences.Set(CurrentUserEmailKey, user.Email);
                Log($"✅ Logged in: {email}");
                return (true, message, user);
            }

            return (false, message, null);
        }
        catch (Exception ex)
        {
            Log($"❌ LoginAsync error: {ex.Message}");
            return (false, $"Login failed: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Reset password for user
    /// </summary>
    public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string newPassword)
    {
        try
        {
            Log($"ResetPasswordAsync: {email}");
            var (success, message) = await _userService.ResetPasswordAsync(email, newPassword);

            if (success)
            {
                Log($"✅ Password reset: {email}");
            }

            return (success, message);
        }
        catch (Exception ex)
        {
            Log($"❌ ResetPasswordAsync error: {ex.Message}");
            return (false, $"Password reset failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get currently authenticated user
    /// </summary>
    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            if (_currentUser != null)
                return _currentUser;

            var userIdStr = Preferences.Get(CurrentUserKey, string.Empty);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return null;

            _currentUser = await _userService.GetUserByIdAsync(userId);
            return _currentUser;
        }
        catch (Exception ex)
        {
            Log($"❌ GetCurrentUserAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var user = await GetCurrentUserAsync();
            return user != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            _currentUser = null;
            Preferences.Remove(CurrentUserKey);
            Preferences.Remove(CurrentUserEmailKey);
            Log("✅ Logged out");
        }
        catch (Exception ex)
        {
            Log($"❌ LogoutAsync error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if email exists
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            return user != null;
        }
        catch (Exception ex)
        {
            Log($"❌ EmailExistsAsync error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Request password reset (for email verification in future)
    /// </summary>
    public async Task<(bool Success, string Message)> RequestPasswordResetAsync(string email)
    {
        try
        {
            Log($"RequestPasswordResetAsync: {email}");
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return (false, "User not found");
            }

            Log($"✅ Password reset requested for {email}");
            return (true, "Password reset email would be sent");
        }
        catch (Exception ex)
        {
            Log($"❌ RequestPasswordResetAsync error: {ex.Message}");
            return (false, $"Password reset request failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthenticationService
{
    Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName, string? phoneNumber = null);
    Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password);
    Task<(bool Success, string Message)> ResetPasswordAsync(string email, string newPassword);
    Task<User?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task LogoutAsync();
    Task<bool> EmailExistsAsync(string email);
    Task<(bool Success, string Message)> RequestPasswordResetAsync(string email);
}
