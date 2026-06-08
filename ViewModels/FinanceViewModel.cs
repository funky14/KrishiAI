using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class FinanceViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private FinancialSummary financialSummary = new();

    [ObservableProperty]
    private List<FinanceTransaction> transactions = new();

    [ObservableProperty]
    private List<IncomeTransaction> incomeTransactions = new();

    [ObservableProperty]
    private List<ExpenseTransaction> expenseTransactions = new();

    [ObservableProperty]
    private decimal totalIncome = 0;

    [ObservableProperty]
    private decimal totalExpense = 0;

    [ObservableProperty]
    private decimal netProfit = 0;

    [ObservableProperty]
    private bool isLoading = false;

    public FinanceViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    [RelayCommand]
    public async Task LoadFinancialSummaryAsync()
    {
        try
        {
            IsLoading = true;
            var endDate = DateTime.Now;
            var startDate = new DateTime(endDate.Year, endDate.Month, 1);

            FinancialSummary = await _financeService.GetFinancialSummaryAsync(startDate, endDate);
            
            TotalIncome = FinancialSummary.TotalIncome;
            TotalExpense = FinancialSummary.TotalExpense;
            NetProfit = FinancialSummary.NetProfit;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Timeout") || ex.Message.Contains("network"))
            {
                await Toast.Make("Network timeout. The server is waking up, please pull down to refresh.", ToastDuration.Long).Show();
            }
            else
            {
                await Toast.Make("Failed to load summary. Offline mode active.", ToastDuration.Short).Show();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadTransactionsAsync()
    {
        try
        {
            IsLoading = true;
            Transactions = await _financeService.GetAllTransactionsAsync();
            IncomeTransactions = (await _financeService.GetAllIncomeAsync()).Cast<IncomeTransaction>().ToList();
            ExpenseTransactions = (await _financeService.GetAllExpensesAsync()).Cast<ExpenseTransaction>().ToList();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Timeout") || ex.Message.Contains("network"))
            {
                await Toast.Make("Network timeout loading transactions.", ToastDuration.Short).Show();
            }
            else
            {
                await Toast.Make("Failed to load transactions.", ToastDuration.Short).Show();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddIncomeAsync(IncomeTransaction income)
    {
        try
        {
            IsLoading = true;
            await _financeService.AddIncomeAsync(income);
            await LoadFinancialSummaryAsync();
            await LoadTransactionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to add income: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddExpenseAsync(ExpenseTransaction expense)
    {
        try
        {
            IsLoading = true;
            await _financeService.AddExpenseAsync(expense);
            await LoadFinancialSummaryAsync();
            await LoadTransactionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to add expense: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddLoanAsync(LoanTransaction loan)
    {
        try
        {
            IsLoading = true;
            await _financeService.AddLoanAsync(loan);
            await LoadFinancialSummaryAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to add loan: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddSubsidyAsync(SubsidyTransaction subsidy)
    {
        try
        {
            IsLoading = true;
            await _financeService.AddSubsidyAsync(subsidy);
            await LoadFinancialSummaryAsync();
            await LoadTransactionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to add subsidy: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddMiscTransactionAsync(MiscellaneousTransaction misc)
    {
        try
        {
            IsLoading = true;
            await _financeService.AddMiscTransactionAsync(misc);
            await LoadFinancialSummaryAsync();
            await LoadTransactionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to add transaction: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteIncomeAsync(IncomeTransaction income)
    {
        try
        {
            if (await Shell.Current.DisplayAlert("Confirm", "Delete this income record?", "Yes", "No"))
            {
                IsLoading = true;
                await _financeService.DeleteIncomeAsync(income.Id);
                await LoadTransactionsAsync();
                await LoadFinancialSummaryAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteExpenseAsync(ExpenseTransaction expense)
    {
        try
        {
            if (await Shell.Current.DisplayAlert("Confirm", "Delete this expense record?", "Yes", "No"))
            {
                IsLoading = true;
                await _financeService.DeleteExpenseAsync(expense.Id);
                await LoadTransactionsAsync();
                await LoadFinancialSummaryAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // UI Interaction Commands
    [RelayCommand]
    public async Task PromptAddIncomeAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.AddIncomePage));
    }

    [RelayCommand]
    public async Task PromptAddExpenseAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.AddExpensePage));
    }

    [RelayCommand]
    public async Task PromptAddLoanAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.AddLoanPage));
    }

    [RelayCommand]
    public async Task PromptAddSubsidyAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.AddSubsidyPage));
    }

    [RelayCommand]
    public async Task PromptAddMiscAsync()
    {
        string result = await Shell.Current.DisplayPromptAsync("Add Misc", "Enter amount (₹):", keyboard: Keyboard.Numeric);
        if (!string.IsNullOrWhiteSpace(result) && decimal.TryParse(result, out decimal amount))
        {
            await AddMiscTransactionAsync(new MiscellaneousTransaction { Amount = amount, TransactionDate = DateTime.Now, CreatedDate = DateTime.Now, UserId = "demo_user", TransactionType = "Misc", MiscCategory = "Other", TransactionDirection = "Outgoing", Category = "Misc" });
        }
    }

    [RelayCommand]
    public async Task NavigateToVoiceAssistantAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.FinanceVoiceEntryPage));
    }

    [RelayCommand]
    public async Task NavigateToReportsAsync()
    {
        await Shell.Current.DisplayAlert("Reports", "Detailed reports feature coming soon!", "OK");
    }

    [RelayCommand]
    public async Task NavigateToHistoryAsync()
    {
        await Shell.Current.GoToAsync("//history");
    }

    /// <summary>
    /// Load financial data for a specific date range
    /// </summary>
    public async Task LoadByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            IsLoading = true;
            FinancialSummary = await _financeService.GetFinancialSummaryAsync(startDate, endDate);
            Transactions = await _financeService.GetAllTransactionsAsync(startDate, endDate);
            
            TotalIncome = FinancialSummary.TotalIncome;
            TotalExpense = FinancialSummary.TotalExpense;
            NetProfit = FinancialSummary.NetProfit;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
