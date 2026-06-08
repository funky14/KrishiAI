using KrishiAI.App.Models;
using System.Diagnostics;

namespace KrishiAI.App.Services;

/// <summary>
/// Authentication Service - Manages user login/registration
/// Uses local preferences for demo, can be extended to use a real API backend
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private User? _currentUser;
    private const string UsersStorageKey = "AuthUsers"; // For demo - stores JSON list of users
    private const string CurrentUserKey = "CurrentUserId";

    public AuthenticationService()
    {
        Log("✅ AuthenticationService initialized");
    }

    private static void Log(string message)
    {
        Debug.WriteLine(message);
        // Also try to use Console for logcat visibility
        try
        {
            Console.WriteLine($"[KrishiAI.Auth] {message}");
        }
        catch { }
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName, string? phoneNumber = null)
    {
        try
        {
            Log($"🔐 RegisterAsync: Registering user {email}");

            // Validate inputs
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
            {
                Log($"   ❌ Validation failed - missing required field");
                return (false, "Email, password, and full name are required");
            }

            // Check if email already exists
            var existingUser = await GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                Log($"   ❌ Email already exists: {email}");
                return (false, "Email already registered");
            }

            // Trim password
            var trimmedPassword = password.Trim();
            Log($"   - Trimmed password length: {trimmedPassword.Length}");

            // TRY METHOD 1: SecureStorage
            try
            {
                await SecureStorage.Default.SetAsync($"pwd_{email}", trimmedPassword);
                Log($"   ✅ Password stored in SecureStorage");
            }
            catch (Exception secEx)
            {
                Log($"   ⚠️ SecureStorage failed: {secEx.Message}, using fallback");
                // Fallback: Store in regular Preferences (not ideal but works for demo)
                Preferences.Set($"pwd_{email}", trimmedPassword);
                Log($"   ✅ Password stored in Preferences (fallback)");
            }

            // Store user metadata in Preferences
            Preferences.Set($"user_{email}_fullname", fullName);
            Preferences.Set($"user_{email}_phone", phoneNumber ?? "");
            Preferences.Set($"user_{email}_created", DateTime.UtcNow.Ticks.ToString());
            Preferences.Set($"user_{email}_exists", "true");
            Log($"   ✅ User metadata stored: {fullName}");

            Log($"✅ User registered successfully: {email}");
            return (true, "Registration successful");
        }
        catch (Exception ex)
        {
            Log($"❌ RegisterAsync Error: {ex.Message}");
            Log($"   Stack: {ex.StackTrace}");
            return (false, $"Registration failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
    {
        try
        {
            Log($"🔐 LoginAsync: User={email}, Pass length={password?.Length ?? 0}");

            // Validate inputs
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Log($"   ❌ Empty email or password");
                return (false, "Email and password are required", null);
            }

            // Check if user exists first
            var exists = Preferences.Get($"user_{email}_exists", "false");
            if (exists != "true")
            {
                Log($"   ❌ User not found (no exists marker): {email}");
                return (false, "Invalid email or password", null);
            }

            // Get stored password - try SecureStorage first, then fallback
            string? storedPassword = null;
            try
            {
                storedPassword = await SecureStorage.Default.GetAsync($"pwd_{email}");
                if (storedPassword != null)
                {
                    Log($"   ✅ Retrieved from SecureStorage");
                }
            }
            catch (Exception ex)
            {
                Log($"   ⚠️ SecureStorage retrieval failed: {ex.Message}");
            }

            // Fallback to Preferences if SecureStorage failed
            if (storedPassword == null)
            {
                storedPassword = Preferences.Get($"pwd_{email}", null);
                if (storedPassword != null)
                {
                    Log($"   ✅ Retrieved from Preferences (fallback)");
                }
            }

            if (storedPassword == null)
            {
                Log($"   ❌ No password found anywhere for: {email}");
                return (false, "Invalid email or password", null);
            }

            // Detailed password comparison
            var trimmedInput = password.Trim();
            var trimmedStored = storedPassword.Trim();

            Log($"   - Input: len={trimmedInput.Length}, first 3 chars: {GetFirstNChars(trimmedInput, 3)}");
            Log($"   - Stored: len={trimmedStored.Length}, first 3 chars: {GetFirstNChars(trimmedStored, 3)}");

            // Do the comparison
            if (trimmedStored == trimmedInput)
            {
                Log($"   ✅ PASSWORD MATCH!");

                // Get user metadata
                var fullName = Preferences.Get($"user_{email}_fullname", email.Split('@')[0]);
                var phoneNumber = Preferences.Get($"user_{email}_phone", "");

                // Create user
                var user = new User
                {
                    Email = email,
                    FullName = fullName,
                    PhoneNumber = string.IsNullOrEmpty(phoneNumber) ? null : phoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Update current user
                _currentUser = user;
                Preferences.Set(CurrentUserKey, user.Id.ToString());
                Preferences.Set("CurrentUserEmail", user.Email);

                Log($"✅ Login successful: {email}");
                return (true, "Login successful", user);
            }
            else
            {
                Log($"   ❌ PASSWORD MISMATCH");
                Log($"   - Input bytes: {string.Join(",", trimmedInput.Take(10).Select(c => (int)c))}...");
                Log($"   - Stored bytes: {string.Join(",", trimmedStored.Take(10).Select(c => (int)c))}...");

                // Character-by-character debug
                int minLen = Math.Min(trimmedInput.Length, trimmedStored.Length);
                for (int i = 0; i < Math.Min(5, minLen); i++)
                {
                    if (trimmedInput[i] != trimmedStored[i])
                    {
                        Log($"   - Char {i}: input='{trimmedInput[i]}'({(int)trimmedInput[i]}) stored='{trimmedStored[i]}'({(int)trimmedStored[i]})");
                    }
                }

                return (false, "Invalid email or password", null);
            }
        }
        catch (Exception ex)
        {
            Log($"❌ LoginAsync Exception: {ex.Message}");
            Log($"   Stack: {ex.StackTrace}");
            return (false, $"Login failed: {ex.Message}", null);
        }
    }

    private static string GetFirstNChars(string? s, int n)
    {
        if (string.IsNullOrEmpty(s)) return "(empty)";
        if (s.Length <= n) return s;
        return s.Substring(0, n) + "...";
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            if (_currentUser != null)
                return _currentUser;

            var userIdStr = Preferences.Get(CurrentUserKey, string.Empty);
            var userEmail = Preferences.Get("CurrentUserEmail", string.Empty);

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return null;

            _currentUser = new User
            {
                Id = userId,
                Email = userEmail,
                FullName = userEmail.Split('@')[0],
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            return _currentUser;
        }
        catch (Exception ex)
        {
            Log($"❌ GetCurrentUserAsync Error: {ex.Message}");
            return null;
        }
    }

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

    public async Task LogoutAsync()
    {
        try
        {
            _currentUser = null;
            Preferences.Remove(CurrentUserKey);
            Preferences.Remove("CurrentUserEmail");
            Log("✅ User logged out successfully");
        }
        catch (Exception ex)
        {
            Log($"❌ LogoutAsync Error: {ex.Message}");
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            var user = await GetUserByEmailAsync(email);
            return user != null;
        }
        catch (Exception ex)
        {
            Log($"❌ EmailExistsAsync Error: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool Success, string Message)> RequestPasswordResetAsync(string email)
    {
        try
        {
            Log($"🔐 RequestPasswordResetAsync: Requesting password reset for {email}");

            // In a real implementation, this would send an email with a reset link
            return (true, "Password reset email sent (feature not fully implemented)");
        }
        catch (Exception ex)
        {
            Log($"❌ RequestPasswordResetAsync Error: {ex.Message}");
            return (false, $"Password reset request failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        try
        {
            Log($"🔐 ResetPasswordAsync: Resetting password for {email}");

            // Verify email exists
            var exists = await EmailExistsAsync(email);
            if (!exists)
                return (false, "User not found");

            // Store new password securely
            var trimmedPassword = newPassword.Trim();
            await SecureStorage.Default.SetAsync($"pwd_{email}", trimmedPassword);

            Log($"✅ Password reset successfully for {email}");
            return (true, "Password reset successfully");
        }
        catch (Exception ex)
        {
            Log($"❌ ResetPasswordAsync Error: {ex.Message}");
            return (false, $"Password reset failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to get a user by email (for demo purposes)
    /// </summary>
    private async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            // Check if password exists for this email
            var password = await SecureStorage.Default.GetAsync($"pwd_{email}");
            if (password != null)
            {
                // Retrieve user metadata
                var fullName = Preferences.Get($"user_{email}_fullname", email.Split('@')[0]);
                var phoneNumber = Preferences.Get($"user_{email}_phone", "");

                return new User
                {
                    Email = email,
                    FullName = fullName,
                    PhoneNumber = string.IsNullOrEmpty(phoneNumber) ? null : phoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Log($"❌ GetUserByEmailAsync Error: {ex.Message}");
            return null;
        }
    }
}
