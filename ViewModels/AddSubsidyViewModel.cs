using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

[QueryProperty(nameof(TransactionToEdit), "TransactionToEdit")]
public partial class AddSubsidyViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private string schemeName = string.Empty;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private DateTime transactionDate = DateTime.Now;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string schemeNamePlaceholder = "Enter scheme name (e.g. PM-KISAN)";

    [ObservableProperty]
    private string pageTitle = "Add Subsidy";

    private FinanceTransaction? _transactionToEdit;
    public FinanceTransaction? TransactionToEdit
    {
        get => _transactionToEdit;
        set
        {
            _transactionToEdit = value;
            if (value != null)
            {
                PageTitle = "Edit Subsidy";
                var categoryToSelect = Categories.FirstOrDefault(c => c.Name == value.SubsidyType) ?? Categories.FirstOrDefault();
                if (categoryToSelect != null)
                {
                    SelectCategory(categoryToSelect);
                }
                
                SchemeName = value.SchemeName ?? "";
                Amount = value.Amount;
                TransactionDate = value.TransactionDate;
                Notes = value.Notes ?? "";
            }
        }
    }

    public ObservableCollection<CategoryItem> Categories { get; } = new();

    public CategoryItem? SelectedCategory => Categories.FirstOrDefault(c => c.IsSelected);

    public AddSubsidyViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
        InitializeCategories();
    }

    private void InitializeCategories()
    {
        Categories.Add(new CategoryItem("PM-KISAN", "🌱") { IsSelected = true }); // Default
        Categories.Add(new CategoryItem("Seed Subsidy", "🧪"));
        Categories.Add(new CategoryItem("Equipment Subsidy", "🚜"));
        Categories.Add(new CategoryItem("Irrigation Subsidy", "💧"));
        Categories.Add(new CategoryItem("Fertilizer Subsidy", "🧴"));
        Categories.Add(new CategoryItem("Other Subsidy", "⋯"));
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
            case "PM-KISAN":
                SchemeNamePlaceholder = "Enter scheme name (e.g. PM-KISAN)";
                SchemeName = "PM-KISAN";
                break;
            case "Seed Subsidy":
                SchemeNamePlaceholder = "Enter scheme name (e.g. Seed DBTL)";
                if (SchemeName == "PM-KISAN" || string.IsNullOrWhiteSpace(SchemeName)) SchemeName = string.Empty;
                break;
            case "Equipment Subsidy":
                SchemeNamePlaceholder = "Enter scheme name (e.g. SMAM Subsidy)";
                if (SchemeName == "PM-KISAN" || string.IsNullOrWhiteSpace(SchemeName)) SchemeName = string.Empty;
                break;
            case "Irrigation Subsidy":
                SchemeNamePlaceholder = "Enter scheme name (e.g. PMKSY)";
                if (SchemeName == "PM-KISAN" || string.IsNullOrWhiteSpace(SchemeName)) SchemeName = string.Empty;
                break;
            case "Fertilizer Subsidy":
                SchemeNamePlaceholder = "Enter scheme name (e.g. DBT Fertilizer)";
                if (SchemeName == "PM-KISAN" || string.IsNullOrWhiteSpace(SchemeName)) SchemeName = string.Empty;
                break;
            default:
                SchemeNamePlaceholder = "Enter scheme name";
                if (SchemeName == "PM-KISAN" || string.IsNullOrWhiteSpace(SchemeName)) SchemeName = string.Empty;
                break;
        }

        OnPropertyChanged(nameof(SelectedCategory));
    }

    [RelayCommand]
    public async Task SaveSubsidyAsync()
    {
        if (string.IsNullOrWhiteSpace(SchemeName))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a scheme name.", "OK");
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
            
            if (_transactionToEdit != null)
            {
                _transactionToEdit.SubsidyType = categoryName;
                _transactionToEdit.SchemeName = SchemeName;
                _transactionToEdit.Description = SchemeName;
                _transactionToEdit.Amount = Amount;
                _transactionToEdit.TransactionDate = TransactionDate;
                _transactionToEdit.ReceivedDate = TransactionDate;
                _transactionToEdit.Notes = Notes;
                
                await _financeService.UpdateSubsidyAsync(_transactionToEdit);
                await Toast.Make("Subsidy updated successfully", ToastDuration.Short, 14).Show();
            }
            else
            {
                var subsidy = new FinanceTransaction
                {
                    UserId = "user123",
                    TransactionType = "Subsidy",
                    Category = "Subsidy",
                    SubsidyType = categoryName,
                    SchemeName = SchemeName,
                    Description = SchemeName,
                    Amount = Amount,
                    TransactionDate = TransactionDate,
                    ReceivedDate = TransactionDate,
                    CreatedDate = DateTime.Now,
                    Notes = Notes
                };

                await _financeService.AddSubsidyAsync(subsidy);
                await Toast.Make("Subsidy saved successfully", ToastDuration.Short, 14).Show();
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
