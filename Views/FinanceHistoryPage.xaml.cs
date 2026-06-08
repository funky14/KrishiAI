using System.Globalization;
using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class FinanceHistoryPage : ContentPage
{
    public FinanceHistoryPage(FinanceHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FinanceHistoryViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}

public class FilterBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value?.ToString() == parameter?.ToString())
            return Color.FromArgb("#0f8d39"); // Selected (Green)
        return Color.FromArgb("#eef8ef"); // Unselected (Light green)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class FilterTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value?.ToString() == parameter?.ToString())
            return Colors.White;
        return Color.FromArgb("#0f8d39");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class AmountColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var type = value?.ToString();
        if (type == "Expense" || type == "Loan")
            return Color.FromArgb("#ff5a5a"); // Red
        return Color.FromArgb("#3bb54a"); // Green
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
