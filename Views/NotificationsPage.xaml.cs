using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
