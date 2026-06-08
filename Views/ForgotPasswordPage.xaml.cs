using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly AuthViewModel _viewModel;

    public ForgotPasswordPage(AuthViewModel viewModel)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔐 ForgotPasswordPage: Constructor started");

            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("   - InitializeComponent() done");

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            System.Diagnostics.Debug.WriteLine("   - ViewModel assigned");

            BindingContext = _viewModel;
            System.Diagnostics.Debug.WriteLine("   - BindingContext set");

            // Subscribe to navigation back to login
            _viewModel.OnNavigateToLogin += () =>
            {
                System.Diagnostics.Debug.WriteLine("🔐 ForgotPasswordPage: OnNavigateToLogin triggered");
                Navigation.PopAsync();
            };
            System.Diagnostics.Debug.WriteLine("   - OnNavigateToLogin event subscribed");

            System.Diagnostics.Debug.WriteLine("✅ ForgotPasswordPage: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ ForgotPasswordPage Constructor FAILED!");
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
            System.Diagnostics.Debug.WriteLine("🔐 ForgotPasswordPage: OnAppearing");
            _viewModel.OnAppearing();
            System.Diagnostics.Debug.WriteLine("   - ViewModel.OnAppearing() done");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ForgotPasswordPage.OnAppearing Error: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try
        {
            System.Diagnostics.Debug.WriteLine("🔐 ForgotPasswordPage: OnDisappearing");
            _viewModel.OnDisappearing();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ForgotPasswordPage.OnDisappearing Error: {ex.Message}");
        }
    }
}
