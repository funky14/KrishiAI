using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class FinanceHistoryViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private ObservableCollection<FinanceTransaction> transactions = new();

    private List<FinanceTransaction> _allTransactions = new();

    [ObservableProperty]
    private string selectedFilter = "All";

    public FinanceHistoryViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    public async Task InitializeAsync()
    {
        await LoadHistoryAsync();
    }

    [RelayCommand]
    public async Task LoadHistoryAsync()
    {
        IsBusy = true;
        try
        {
            // Fetch past 6 months by default as per screenshot "Last 6 Months"
            _allTransactions = await _financeService.GetAllTransactionsAsync(DateTime.Now.AddMonths(-6), DateTime.Now);
            ApplyFilter(SelectedFilter);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load history: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ApplyFilter(string filter)
    {
        SelectedFilter = filter;
        
        if (filter == "All")
        {
            Transactions = new ObservableCollection<FinanceTransaction>(_allTransactions.OrderByDescending(t => t.TransactionDate));
        }
        else
        {
            Transactions = new ObservableCollection<FinanceTransaction>(
                _allTransactions.Where(t => t.TransactionType == filter)
                              .OrderByDescending(t => t.TransactionDate));
        }
    }

    [RelayCommand]
    public async Task ClearHistoryAsync()
    {
        if (await Shell.Current.DisplayAlert("Clear History", "Are you sure you want to delete all visible records?", "Yes", "No"))
        {
            IsBusy = true;
            try
            {
                foreach (var tx in Transactions.ToList())
                {
                    if (tx.TransactionType == "Expense") await _financeService.DeleteExpenseAsync(tx.Id);
                    else if (tx.TransactionType == "Income") await _financeService.DeleteIncomeAsync(tx.Id);
                    else if (tx.TransactionType == "Loan") await _financeService.DeleteLoanAsync(tx.Id);
                    else if (tx.TransactionType == "Subsidy") await _financeService.DeleteSubsidyAsync(tx.Id);
                    else if (tx.TransactionType == "Misc" || tx.TransactionType == "Miscellaneous") await _financeService.DeleteMiscTransactionAsync(tx.Id);
                }
                await LoadHistoryAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to clear: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
