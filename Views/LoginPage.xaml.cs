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

    private async Task ViewModel_OnLoginSuccess()
    {
        System.Diagnostics.Debug.WriteLine("✅ LoginPage: OnLoginSuccess triggered");
        // MAUI requires UI navigation to happen on the Main Thread
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            var app = (Application.Current as App)!;
            await app.NavigateToAppShellAsync();
        });
    }

    private void ViewModel_OnNavigateToSignup()
    {
        System.Diagnostics.Debug.WriteLine("📝 LoginPage: OnNavigateToSignup triggered");
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            await Navigation.PushAsync(new SignupPage(_viewModel));
        });
    }

    private void ViewModel_OnNavigateToForgotPassword()
    {
        System.Diagnostics.Debug.WriteLine("🔐 LoginPage: OnNavigateToForgotPassword triggered");
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            await Navigation.PushAsync(new ForgotPasswordPage(_viewModel));
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            System.Diagnostics.Debug.WriteLine("📱 LoginPage: OnAppearing");
            
            // Subscribe to events using named methods to allow unsubscription later
            _viewModel.OnLoginSuccess += ViewModel_OnLoginSuccess;
            _viewModel.OnNavigateToSignup += ViewModel_OnNavigateToSignup;
            _viewModel.OnNavigateToForgotPassword += ViewModel_OnNavigateToForgotPassword;
            
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
            
            // Unsubscribe from events to prevent memory leaks and multiple executions
            _viewModel.OnLoginSuccess -= ViewModel_OnLoginSuccess;
            _viewModel.OnNavigateToSignup -= ViewModel_OnNavigateToSignup;
            _viewModel.OnNavigateToForgotPassword -= ViewModel_OnNavigateToForgotPassword;
            
            _viewModel.OnDisappearing();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ LoginPage.OnDisappearing Error: {ex.Message}");
        }
    }
}
