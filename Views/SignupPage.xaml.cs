using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class SignupPage : ContentPage
{
    private readonly AuthViewModel _viewModel;

    public SignupPage(AuthViewModel viewModel)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("📝 SignupPage: Constructor started");

            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("   - InitializeComponent() done");

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            System.Diagnostics.Debug.WriteLine("   - ViewModel assigned");

            BindingContext = _viewModel;
            System.Diagnostics.Debug.WriteLine("   - BindingContext set");

            // Subscribe to navigation events
            _viewModel.OnNavigateToLogin += () =>
            {
                System.Diagnostics.Debug.WriteLine("🔐 SignupPage: OnNavigateToLogin triggered");
                Navigation.PopAsync();
            };
            System.Diagnostics.Debug.WriteLine("   - OnNavigateToLogin event subscribed");

            System.Diagnostics.Debug.WriteLine("✅ SignupPage: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ SignupPage Constructor FAILED!");
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
            System.Diagnostics.Debug.WriteLine("📝 SignupPage: OnAppearing");
            _viewModel.OnAppearing();
            System.Diagnostics.Debug.WriteLine("   - ViewModel.OnAppearing() done");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ SignupPage.OnAppearing Error: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try
        {
            System.Diagnostics.Debug.WriteLine("📝 SignupPage: OnDisappearing");
            _viewModel.OnDisappearing();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ SignupPage.OnDisappearing Error: {ex.Message}");
        }
    }
}
