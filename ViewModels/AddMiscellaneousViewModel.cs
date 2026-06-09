using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

[QueryProperty(nameof(TransactionToEdit), "TransactionToEdit")]
public partial class AddMiscellaneousViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private string transactionName = string.Empty;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private DateTime transactionDate = DateTime.Now;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string pageTitle = "Add Miscellaneous";

    private FinanceTransaction? _transactionToEdit;
    public FinanceTransaction? TransactionToEdit
    {
        get => _transactionToEdit;
        set
        {
            _transactionToEdit = value;
            if (value != null)
            {
                PageTitle = "Edit Miscellaneous";
                var categoryToSelect = Categories.FirstOrDefault(c => c.Name == value.MiscCategory) ?? Categories.FirstOrDefault();
                if (categoryToSelect != null)
                {
                    SelectCategory(categoryToSelect);
                }
                
                TransactionName = value.Description ?? "";
                Amount = value.Amount;
                TransactionDate = value.TransactionDate;
                Notes = value.Notes ?? "";
            }
        }
    }

    public ObservableCollection<CategoryItem> Categories { get; } = new();

    public CategoryItem? SelectedCategory => Categories.FirstOrDefault(c => c.IsSelected);

    public AddMiscellaneousViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
        InitializeCategories();
    }

    private void InitializeCategories()
    {
        Categories.Add(new CategoryItem("Other Expense", "🧾") { IsSelected = true }); // Default
        Categories.Add(new CategoryItem("Other Income", "💰"));
        Categories.Add(new CategoryItem("Training / Education", "📚"));
        Categories.Add(new CategoryItem("Rental Income", "🏠"));
        Categories.Add(new CategoryItem("Service Charges", "⚙️"));
        Categories.Add(new CategoryItem("Custom Entry", "✏️"));
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
    public async Task SaveTransactionAsync()
    {
        if (string.IsNullOrWhiteSpace(TransactionName))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a transaction name.", "OK");
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
            var categoryName = SelectedCategory?.Name ?? "Custom Entry";
            var direction = (categoryName.Contains("Income") || categoryName.Contains("Rental")) ? "Incoming" : "Outgoing";
            
            if (_transactionToEdit != null)
            {
                _transactionToEdit.MiscCategory = categoryName;
                _transactionToEdit.Description = TransactionName;
                _transactionToEdit.Amount = Amount;
                _transactionToEdit.TransactionDirection = direction;
                _transactionToEdit.TransactionDate = TransactionDate;
                _transactionToEdit.Notes = Notes;
                
                await _financeService.UpdateMiscTransactionAsync(_transactionToEdit);
                await Toast.Make("Transaction updated successfully", ToastDuration.Short, 14).Show();
            }
            else
            {
                var misc = new FinanceTransaction
                {
                    UserId = "demo_user",
                    TransactionType = "Misc",
                    Category = "Misc",
                    MiscCategory = categoryName,
                    Description = TransactionName,
                    Amount = Amount,
                    TransactionDirection = direction,
                    TransactionDate = TransactionDate,
                    CreatedDate = DateTime.Now,
                    Notes = Notes
                };

                await _financeService.AddMiscTransactionAsync(misc);
                await Toast.Make("Transaction saved successfully", ToastDuration.Short, 14).Show();
            }
            
            // Navigate back
            await Shell.Current.GoToAsync("..");
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
