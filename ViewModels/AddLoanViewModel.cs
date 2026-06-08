using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class AddLoanViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private string loanName = string.Empty;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private string lenderName = string.Empty;

    [ObservableProperty]
    private DateTime transactionDate = DateTime.Now;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string loanNamePlaceholder = "Enter loan name (e.g. SBI Crop Loan)";

    [ObservableProperty]
    private string lenderNameLabel = "Bank Name";

    [ObservableProperty]
    private string lenderNamePlaceholder = "Enter bank name";

    public ObservableCollection<CategoryItem> Categories { get; } = new();

    public CategoryItem? SelectedCategory => Categories.FirstOrDefault(c => c.IsSelected);

    public AddLoanViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
        InitializeCategories();
    }

    private void InitializeCategories()
    {
        Categories.Add(new CategoryItem("Bank Loan", "🏦") { IsSelected = true }); // Default
        Categories.Add(new CategoryItem("KCC Loan", "🌳"));
        Categories.Add(new CategoryItem("Private Loan", "👤"));
        Categories.Add(new CategoryItem("Government Loan", "🏛️"));
        Categories.Add(new CategoryItem("Other Loan", "⋯"));
    }

    [RelayCommand]
    public void SelectCategory(CategoryItem selected)
    {
        foreach (var category in Categories)
        {
            category.IsSelected = category == selected;
        }

        switch (selected.Name)
        {
            case "Bank Loan":
                LoanNamePlaceholder = "Enter loan name (e.g. SBI Crop Loan)";
                LenderNameLabel = "Bank Name";
                LenderNamePlaceholder = "Enter bank name";
                break;
            case "KCC Loan":
                LoanNamePlaceholder = "Enter loan name (e.g. KCC Loan)";
                LenderNameLabel = "Bank Name";
                LenderNamePlaceholder = "Enter bank name";
                break;
            case "Private Loan":
                LoanNamePlaceholder = "Enter loan name (e.g. Loan from Uncle)";
                LenderNameLabel = "Lender Name";
                LenderNamePlaceholder = "Enter lender name";
                break;
            case "Government Loan":
                LoanNamePlaceholder = "Enter scheme name (e.g. State Agri Loan)";
                LenderNameLabel = "Department / Bank Name";
                LenderNamePlaceholder = "Enter department or bank";
                break;
            default:
                LoanNamePlaceholder = "Enter loan name";
                LenderNameLabel = "Lender Name";
                LenderNamePlaceholder = "Enter lender name";
                break;
        }

        OnPropertyChanged(nameof(SelectedCategory));
    }

    [RelayCommand]
    public async Task SaveLoanAsync()
    {
        if (string.IsNullOrWhiteSpace(LoanName))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a loan name.", "OK");
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
            var loan = new LoanTransaction
            {
                UserId = "user123",
                TransactionType = "Loan",
                Category = "Loan",
                LoanType = categoryName,
                Description = LoanName,
                Amount = Amount,
                RemainingAmount = Amount,
                LenderName = LenderName,
                TransactionDate = TransactionDate,
                CreatedDate = DateTime.Now,
                DueDate = TransactionDate.AddYears(1), // Default due date 1 year
                Notes = Notes
            };

            await _financeService.AddLoanAsync(loan);
            
            // Navigate back
            await Shell.Current.GoToAsync("..");

            // Show success message
            await Toast.Make("Loan saved successfully", ToastDuration.Short, 14).Show();
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
