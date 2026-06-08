using KrishiAI.App.Models;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace KrishiAI.App.Services;

/// <summary>
/// Service for user management operations against SQL Server
/// Handles registration, login, and password reset
/// </summary>
public class SqlServerUserService : ISqlServerUserService
{
    private readonly ISqlServerConnectionService _connectionService;

    public SqlServerUserService(ISqlServerConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    private static void Log(string message)
    {
        Debug.WriteLine($"[SqlServerUserService] {message}");
        Console.WriteLine($"[SqlServerUserService] {message}");
    }

    /// <summary>
    /// Register a new user in SQL Server database
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> RegisterUserAsync(string email, string password, string fullName, string? phoneNumber = null)
    {
        try
        {
            Log($"🔐 RegisterUserAsync: Registering {email}");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
            {
                Log($"   ❌ Validation failed - missing required fields");
                return (false, "Email, password, and full name are required", null);
            }

            var trimmedPassword = password.Trim();

            // Check if user already exists
            const string checkQuery = "SELECT COUNT(*) FROM dbo.[User] WHERE Email = @Email";
            var count = await _connectionService.ExecuteScalarAsync<int>(checkQuery, new Dictionary<string, object> { { "@Email", email } });

            if (count > 0)
            {
                Log($"   ❌ Email already exists: {email}");
                return (false, "Email already registered", null);
            }

            // Insert new user
            const string insertQuery = @"
INSERT INTO dbo.[User] (Email, PasswordHash, FullName, PhoneNumber, CreatedAt, IsActive)
VALUES (@Email, @PasswordHash, @FullName, @PhoneNumber, @CreatedAt, @IsActive);
SELECT CAST(SCOPE_IDENTITY() as int);";

            var userId = await _connectionService.ExecuteScalarAsync<int>(insertQuery, new Dictionary<string, object>
            {
                { "@Email", email },
                { "@PasswordHash", trimmedPassword },
                { "@FullName", fullName },
                { "@PhoneNumber", (object?)phoneNumber ?? DBNull.Value },
                { "@CreatedAt", DateTime.UtcNow },
                { "@IsActive", true }
            });

            var user = new User
            {
                Id = userId,
                Email = email,
                PasswordHash = trimmedPassword,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            Log($"✅ User registered successfully: {email} (ID: {userId})");
            return (true, "Registration successful", user);
        }
        catch (Exception ex)
        {
            Log($"❌ RegisterUserAsync Error: {ex.Message}");
            return (false, $"Registration failed: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
    {
        try
        {
            Log($"🔐 LoginAsync: Authenticating {email}");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Log($"   ❌ Empty email or password");
                return (false, "Email and password are required", null);
            }

            var trimmedPassword = password.Trim();

            // Get user by email
            const string query = @"
SELECT Id, Email, PasswordHash, FullName, PhoneNumber, CreatedAt, LastLogin, IsActive
FROM dbo.[User]
WHERE Email = @Email";

            await using var reader = await _connectionService.ExecuteReaderAsync(query, new Dictionary<string, object> { { "@Email", email } });

            if (reader == null || !await reader.ReadAsync())
            {
                Log($"   ❌ User not found: {email}");
                return (false, "Invalid email or password", null);
            }

            // Read user data
            var userId = reader.GetInt32(0);
            var storedEmail = reader.GetString(1);
            var storedPasswordHash = reader.GetString(2);
            var storedFullName = reader.GetString(3);
            var phoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4);
            var createdAt = reader.GetDateTime(5);
            var isActive = reader.GetBoolean(7);

            await reader.CloseAsync();

            // Verify password
            if (storedPasswordHash.Trim() != trimmedPassword)
            {
                Log($"   ❌ Password mismatch for {email}");
                return (false, "Invalid email or password", null);
            }

            if (!isActive)
            {
                Log($"   ❌ User account is inactive: {email}");
                return (false, "User account is inactive", null);
            }

            // Update last login
            const string updateQuery = "UPDATE dbo.[User] SET LastLogin = @LastLogin WHERE Id = @Id";
            await _connectionService.ExecuteNonQueryAsync(updateQuery, new Dictionary<string, object>
            {
                { "@LastLogin", DateTime.UtcNow },
                { "@Id", userId }
            });

            var user = new User
            {
                Id = userId,
                Email = storedEmail,
                PasswordHash = storedPasswordHash,
                FullName = storedFullName,
                PhoneNumber = phoneNumber,
                CreatedAt = createdAt,
                LastLogin = DateTime.UtcNow,
                IsActive = isActive
            };

            Log($"✅ Login successful: {email}");
            return (true, "Login successful", user);
        }
        catch (Exception ex)
        {
            Log($"❌ LoginAsync Error: {ex.Message}");
            return (false, $"Login failed: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Reset user password
    /// </summary>
    public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string newPassword)
    {
        try
        {
            Log($"🔐 ResetPasswordAsync: Resetting password for {email}");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
            {
                Log($"   ❌ Email and password are required");
                return (false, "Email and new password are required");
            }

            var trimmedPassword = newPassword.Trim();

            // Check if user exists
            const string checkQuery = "SELECT Id FROM dbo.[User] WHERE Email = @Email";
            var userId = await _connectionService.ExecuteScalarAsync<int?>(checkQuery, new Dictionary<string, object> { { "@Email", email } });

            if (userId == null)
            {
                Log($"   ❌ User not found: {email}");
                return (false, "User not found");
            }

            // Update password
            const string updateQuery = "UPDATE dbo.[User] SET PasswordHash = @PasswordHash WHERE Email = @Email";
            await _connectionService.ExecuteNonQueryAsync(updateQuery, new Dictionary<string, object>
            {
                { "@PasswordHash", trimmedPassword },
                { "@Email", email }
            });

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
    /// Get user by email
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            const string query = @"
SELECT Id, Email, PasswordHash, FullName, PhoneNumber, CreatedAt, LastLogin, IsActive
FROM dbo.[User]
WHERE Email = @Email";

            await using var reader = await _connectionService.ExecuteReaderAsync(query, new Dictionary<string, object> { { "@Email", email } });

            if (reader == null || !await reader.ReadAsync())
                return null;

            var user = new User
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.GetString(3),
                PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5),
                LastLogin = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                IsActive = reader.GetBoolean(7)
            };

            await reader.CloseAsync();
            return user;
        }
        catch (Exception ex)
        {
            Log($"❌ GetUserByEmailAsync Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            const string query = @"
SELECT Id, Email, PasswordHash, FullName, PhoneNumber, CreatedAt, LastLogin, IsActive
FROM dbo.[User]
WHERE Id = @Id";

            await using var reader = await _connectionService.ExecuteReaderAsync(query, new Dictionary<string, object> { { "@Id", userId } });

            if (reader == null || !await reader.ReadAsync())
                return null;

            var user = new User
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.GetString(3),
                PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5),
                LastLogin = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                IsActive = reader.GetBoolean(7)
            };

            await reader.CloseAsync();
            return user;
        }
        catch (Exception ex)
        {
            Log($"❌ GetUserByIdAsync Error: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// Interface for SQL Server user service
/// </summary>
public interface ISqlServerUserService
{
    Task<(bool Success, string Message, User? User)> RegisterUserAsync(string email, string password, string fullName, string? phoneNumber = null);
    Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password);
    Task<(bool Success, string Message)> ResetPasswordAsync(string email, string newPassword);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
}
