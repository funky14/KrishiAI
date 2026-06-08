using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthViewModel _viewModel;

    public LoginPage(AuthViewModel viewModel)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("📱 LoginPage: Constructor started");

            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("   - InitializeComponent() done");

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            System.Diagnostics.Debug.WriteLine("   - ViewModel assigned");

            BindingContext = _viewModel;
            System.Diagnostics.Debug.WriteLine("   - BindingContext set");

            // Subscribe to login success - navigate to app shell
            _viewModel.OnLoginSuccess += async () =>
            {
                System.Diagnostics.Debug.WriteLine("✅ LoginPage: OnLoginSuccess triggered");
                var app = (Application.Current as App)!;
                await app.NavigateToAppShellAsync();
            };
            System.Diagnostics.Debug.WriteLine("   - OnLoginSuccess event subscribed");

            // Subscribe to navigation to signup
            _viewModel.OnNavigateToSignup += async () =>
            {
                System.Diagnostics.Debug.WriteLine("📝 LoginPage: OnNavigateToSignup triggered");
                await Navigation.PushAsync(new SignupPage(_viewModel));
            };
            System.Diagnostics.Debug.WriteLine("   - OnNavigateToSignup event subscribed");

            // Subscribe to navigation to forgot password
            _viewModel.OnNavigateToForgotPassword += async () =>
            {
                System.Diagnostics.Debug.WriteLine("🔐 LoginPage: OnNavigateToForgotPassword triggered");
                await Navigation.PushAsync(new ForgotPasswordPage(_viewModel));
            };
            System.Diagnostics.Debug.WriteLine("   - OnNavigateToForgotPassword event subscribed");

            System.Diagnostics.Debug.WriteLine("✅ LoginPage: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ LoginPage Constructor FAILED!");
            System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"❌ Inner: {ex.InnerException?.Message}");
            throw;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            System.Diagnostics.Debug.WriteLine("📱 LoginPage: OnAppearing");
            _viewModel.OnAppearing();
            System.Diagnostics.Debug.WriteLine("   - ViewModel.OnAppearing() done");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ LoginPage.OnAppearing Error: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try
        {
            System.Diagnostics.Debug.WriteLine("📱 LoginPage: OnDisappearing");
            _viewModel.OnDisappearing();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ LoginPage.OnDisappearing Error: {ex.Message}");
        }
    }
}
