using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class AuthViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    public string email = string.Empty;

    [ObservableProperty]
    public string password = string.Empty;

    [ObservableProperty]
    public string confirmPassword = string.Empty;

    [ObservableProperty]
    public string fullName = string.Empty;

    [ObservableProperty]
    public string phoneNumber = string.Empty;

    [ObservableProperty]
    public string newPassword = string.Empty;

    [ObservableProperty]
    public string confirmNewPassword = string.Empty;

    [ObservableProperty]
    public string successMessage = string.Empty;

    [ObservableProperty]
    public bool isLoginMode = true;

    public event Func<Task>? OnLoginSuccess;
    public event Action? OnNavigateToSignup;
    public event Action? OnNavigateToLogin;
    public event Action? OnNavigateToForgotPassword;

    public AuthViewModel(IAuthenticationService authService)
    {
        _authService = authService;
        Title = "Login";
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsBusy)
            return;

        // Validate
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var (success, message, user) = await _authService.LoginAsync(Email, Password);

            if (success && user != null)
            {
                // Clear form
                Email = string.Empty;
                Password = string.Empty;
                ErrorMessage = string.Empty;

                // Notify success and navigate to app shell
                if (OnLoginSuccess != null)
                    await OnLoginSuccess();
            }
            else
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SignupAsync()
    {
        if (IsBusy)
            return;

        // Validate
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            ErrorMessage = "Full name is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var (success, message) = await _authService.RegisterAsync(Email, Password, FullName, PhoneNumber);

            if (success)
            {
                // Clear form
                Email = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                FullName = string.Empty;
                PhoneNumber = string.Empty;

                ErrorMessage = message; // "Registration successful! Please log in."

                // Navigate back to login
                await Task.Delay(1500);
                OnNavigateToLogin?.Invoke();
                IsLoginMode = true;
            }
            else
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Signup error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void NavigateToSignup()
    {
        ClearForm();
        IsLoginMode = false;
        Title = "Sign Up";
        OnNavigateToSignup?.Invoke();
    }

    [RelayCommand]
    public void NavigateToLogin()
    {
        ClearForm();
        IsLoginMode = true;
        Title = "Login";
        OnNavigateToLogin?.Invoke();
    }

    private void ClearForm()
    {
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        FullName = string.Empty;
        PhoneNumber = string.Empty;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    public void NavigateToForgotPassword()
    {
        OnNavigateToForgotPassword?.Invoke();
    }

    [RelayCommand]
    public async Task ResetPasswordAsync()
    {
        if (IsBusy)
            return;

        // Validate email
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email is required";
            return;
        }

        // Validate new password
        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "New password is required";
            return;
        }

        if (NewPassword.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters";
            return;
        }

        // Validate password confirmation
        if (NewPassword != ConfirmNewPassword)
        {
            ErrorMessage = "Passwords do not match";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Call password reset
            var (success, message) = await _authService.ResetPasswordAsync(Email, NewPassword);

            if (success)
            {
                SuccessMessage = "Password reset successfully! Redirecting to login...";
                ErrorMessage = string.Empty;

                // Clear form
                Email = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;

                // Navigate back to login after delay
                await Task.Delay(2000);
                OnNavigateToLogin?.Invoke();
            }
            else
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Reset error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
