using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class AddExpenseViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private string expenseName = string.Empty;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private DateTime transactionDate = DateTime.Now;

    [ObservableProperty]
    private string notes = string.Empty;

    public ObservableCollection<CategoryItem> Categories { get; } = new();

    public CategoryItem? SelectedCategory => Categories.FirstOrDefault(c => c.IsSelected);

    public AddExpenseViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
        InitializeCategories();
    }

    private void InitializeCategories()
    {
        Categories.Add(new CategoryItem("Seeds", "🌱") { IsSelected = true }); // Default
        Categories.Add(new CategoryItem("Fertilizers & Pesticides", "🌿"));
        Categories.Add(new CategoryItem("Labor", "👨‍🌾"));
        Categories.Add(new CategoryItem("Irrigation", "💧"));
        Categories.Add(new CategoryItem("Equipment", "⚙️"));
        Categories.Add(new CategoryItem("Transportation", "🚚"));
        Categories.Add(new CategoryItem("Land Rent", "🏡"));
        Categories.Add(new CategoryItem("Miscellaneous", "ellipsis")); // Using emoji or icon
    }

    [RelayCommand]
    public void SelectCategory(CategoryItem selected)
    {
        foreach (var category in Categories)
        {
            category.IsSelected = category == selected;
        }
        OnPropertyChanged(nameof(SelectedCategory));
    }

    [RelayCommand]
    public async Task SaveExpenseAsync()
    {
        if (string.IsNullOrWhiteSpace(ExpenseName))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter an expense name.", "OK");
            return;
        }

        if (Amount <= 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a valid amount.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var categoryName = SelectedCategory?.Name ?? "General";
            var expense = new ExpenseTransaction
            {
                UserId = "user123", // TODO: Get from auth service
                TransactionType = "Expense",
                Category = categoryName,
                ExpenseCategory = categoryName,
                ExpenseName = ExpenseName,
                Amount = Amount,
                TransactionDate = TransactionDate,
                CreatedDate = DateTime.Now,
                Notes = Notes
            };

            await _financeService.AddExpenseAsync(expense);
            
            // Navigate back
            await Shell.Current.GoToAsync("..");

            // Show mobile-friendly success message
            await Toast.Make("Expense saved successfully", ToastDuration.Short, 14).Show();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
