using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using KrishiAI.App.Resources.Strings;

namespace KrishiAI.App.ViewModels;

public partial class EditProfileViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string currentFullName = string.Empty;

    [ObservableProperty]
    private string currentPhoneNumber = string.Empty;

    [ObservableProperty]
    private string editProfileText = string.Empty;

    [ObservableProperty]
    private string fullNameText = string.Empty;

    [ObservableProperty]
    private string phoneNumberText = string.Empty;

    [ObservableProperty]
    private string updateProfileText = string.Empty;

    [ObservableProperty]
    private string cancelText = string.Empty;

    [ObservableProperty]
    private bool hasError = false;

    public EditProfileViewModel(IAuthenticationService authenticationService, ILocalizationService localizationService)
    {
        _authenticationService = authenticationService;
        _localizationService = localizationService;

        Title = "Edit Profile";
        InitializeLocalization();
        LoadCurrentUserInfo();
    }

    private void InitializeLocalization()
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        EditProfileText = AppStrings.EditProfile;
        FullNameText = AppStrings.FullName;
        PhoneNumberText = AppStrings.PhoneNumber;
        UpdateProfileText = AppStrings.UpdateProfile;
        CancelText = AppStrings.Cancel;
    }

    public override void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
    }

    private async void LoadCurrentUserInfo()
    {
        try
        {
            var currentUser = await _authenticationService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                CurrentFullName = currentUser.FullName ?? "User";
                CurrentPhoneNumber = currentUser.PhoneNumber ?? "No phone number";

                // Initialize form fields with current values
                FullName = currentUser.FullName ?? string.Empty;
                PhoneNumber = currentUser.PhoneNumber ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading user info: {ex.Message}");
            ErrorMessage = $"Error loading profile: {ex.Message}";
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            ErrorMessage = AppStrings.PleaseEnterValidName;
            HasError = true;
            return false;
        }

        HasError = false;
        return true;
    }

    [RelayCommand]
    private async Task UpdateProfile()
    {
        try
        {
            if (!ValidateInput())
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _authenticationService.UpdateUserProfileAsync(
                FullName.Trim(),
                string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim()
            );

            bool success = result.Success;
            string message = result.Message;

            if (success)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    AppStrings.Success,
                    AppStrings.ProfileUpdateSuccess,
                    AppStrings.OK);

                // Navigate back to settings
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = message;
                HasError = true;
                await Application.Current!.MainPage!.DisplayAlert(
                    AppStrings.Error,
                    message,
                    AppStrings.OK);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Update Profile Error: {ex.Message}");
            ErrorMessage = $"Error: {ex.Message}";
            HasError = true;
            await Application.Current!.MainPage!.DisplayAlert(
                AppStrings.Error,
                $"Failed to update profile: {ex.Message}",
                AppStrings.OK);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        // Navigate back to settings without saving
        await Shell.Current.GoToAsync("..");
    }
}
