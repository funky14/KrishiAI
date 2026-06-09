using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

[QueryProperty(nameof(TransactionToEdit), "TransactionToEdit")]
public partial class AddIncomeViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private string cropName = string.Empty;

    [ObservableProperty]
    private decimal quantity;

    [ObservableProperty]
    private string quantityUnit = "Quintals";

    [ObservableProperty]
    private decimal pricePerUnit;

    [ObservableProperty]
    private string buyerName = string.Empty;

    [ObservableProperty]
    private bool isCropSale = true;

    [ObservableProperty]
    private string itemName = string.Empty;

    [ObservableProperty]
    private decimal totalAmount;

    [ObservableProperty]
    private DateTime transactionDate = DateTime.Now;

    [ObservableProperty]
    private string pageTitle = "Add Income";

    private FinanceTransaction? _transactionToEdit;
    public FinanceTransaction? TransactionToEdit
    {
        get => _transactionToEdit;
        set
        {
            _transactionToEdit = value;
            if (value != null)
            {
                PageTitle = "Edit Income";
                var categoryToSelect = Categories.FirstOrDefault(c => c.Name == value.Category) ?? Categories.FirstOrDefault();
                if (categoryToSelect != null)
                {
                    SelectCategory(categoryToSelect);
                }
                
                TransactionDate = value.TransactionDate;
                BuyerName = value.BuyerName ?? "";
                
                if (IsCropSale)
                {
                    CropName = value.CropName ?? "";
                    Quantity = value.Quantity;
                    QuantityUnit = value.QuantityUnit ?? "Quintals";
                    PricePerUnit = value.PricePerUnit;
                }
                else
                {
                    ItemName = value.Description ?? "";
                    TotalAmount = value.Amount;
                }
            }
        }
    }

    public ObservableCollection<CategoryItem> Categories { get; } = new();

    public CategoryItem? SelectedCategory => Categories.FirstOrDefault(c => c.IsSelected);

    public ObservableCollection<string> Units { get; } = new() { "Quintals", "Kgs", "Tons" };

    public AddIncomeViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
        InitializeCategories();
    }

    private void InitializeCategories()
    {
        Categories.Add(new CategoryItem("Crop Sale", "🌿") { IsSelected = true }); // Default
        Categories.Add(new CategoryItem("Equipment Sale", "🚜"));
        Categories.Add(new CategoryItem("Asset Sale", "🏡"));
        Categories.Add(new CategoryItem("Government Subsidy", "🏛️"));
        Categories.Add(new CategoryItem("Insurance Claim", "🛡️"));
        Categories.Add(new CategoryItem("Other Income", "💼"));
    }

    [RelayCommand]
    public void SelectCategory(CategoryItem selected)
    {
        foreach (var category in Categories)
        {
            category.IsSelected = category == selected;
        }
        IsCropSale = selected.Name == "Crop Sale";
        OnPropertyChanged(nameof(SelectedCategory));
    }

    [RelayCommand]
    public async Task SaveIncomeAsync()
    {
        if (IsCropSale)
        {
            if (string.IsNullOrWhiteSpace(CropName))
            {
                await Shell.Current.DisplayAlert("Error", "Please enter crop name.", "OK");
                return;
            }
            if (Quantity <= 0 || PricePerUnit <= 0)
            {
                await Shell.Current.DisplayAlert("Error", "Please enter valid quantity and price.", "OK");
                return;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ItemName))
            {
                await Shell.Current.DisplayAlert("Error", "Please enter an item name or description.", "OK");
                return;
            }
            if (TotalAmount <= 0)
            {
                await Shell.Current.DisplayAlert("Error", "Please enter a valid amount.", "OK");
                return;
            }
        }

        IsBusy = true;
        try
        {
            var categoryName = SelectedCategory?.Name ?? "General";
            
            if (_transactionToEdit != null)
            {
                _transactionToEdit.Category = categoryName;
                _transactionToEdit.TransactionDate = TransactionDate;
                _transactionToEdit.BuyerName = BuyerName;

                if (IsCropSale)
                {
                    _transactionToEdit.CropName = CropName;
                    _transactionToEdit.Quantity = Quantity;
                    _transactionToEdit.QuantityUnit = QuantityUnit;
                    _transactionToEdit.PricePerUnit = PricePerUnit;
                    _transactionToEdit.Amount = Quantity * PricePerUnit;
                    _transactionToEdit.Description = "Crop Sale";
                }
                else
                {
                    _transactionToEdit.CropName = "N/A";
                    _transactionToEdit.Quantity = 1;
                    _transactionToEdit.QuantityUnit = "Units";
                    _transactionToEdit.PricePerUnit = TotalAmount;
                    _transactionToEdit.Amount = TotalAmount;
                    _transactionToEdit.Description = ItemName;
                }

                await _financeService.UpdateIncomeAsync(_transactionToEdit);
                await Toast.Make("Income updated successfully", ToastDuration.Short, 14).Show();
            }
            else
            {
                var income = new FinanceTransaction
                {
                    UserId = "user123",
                    TransactionType = "Income",
                    Category = categoryName,
                    TransactionDate = TransactionDate,
                    CreatedDate = DateTime.Now,
                    BuyerName = BuyerName
                };

                if (IsCropSale)
                {
                    income.CropName = CropName;
                    income.Quantity = Quantity;
                    income.QuantityUnit = QuantityUnit;
                    income.PricePerUnit = PricePerUnit;
                    income.Amount = Quantity * PricePerUnit;
                    income.Description = "Crop Sale";
                }
                else
                {
                    income.CropName = "N/A";
                    income.Quantity = 1;
                    income.QuantityUnit = "Units";
                    income.PricePerUnit = TotalAmount;
                    income.Amount = TotalAmount;
                    income.Description = ItemName;
                }

                await _financeService.AddIncomeAsync(income);
                await Toast.Make("Income saved successfully", ToastDuration.Short, 14).Show();
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
