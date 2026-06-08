using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using Microcharts;
using SkiaSharp;

namespace KrishiAI.App.ViewModels;

public partial class FinanceReportsViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private Chart expenseChart;

    [ObservableProperty]
    private ObservableCollection<IncomeBreakdownItem> incomeBreakdown = new();

    public FinanceReportsViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            // For now, load current month
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var transactions = await _financeService.GetAllTransactionsAsync(startDate, endDate);
            
            BuildExpenseChart(transactions.Where(t => t.TransactionType == "Expense").ToList());
            BuildIncomeBreakdown(transactions.Where(t => t.TransactionType == "Income").ToList());
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load reports: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildExpenseChart(List<FinanceTransaction> expenses)
    {
        if (expenses.Count == 0)
        {
            ExpenseChart = new DonutChart { Entries = new List<ChartEntry>() };
            return;
        }

        var total = expenses.Sum(e => e.Amount);
        var grouped = expenses.GroupBy(e => e.Category)
                              .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                              .OrderByDescending(g => g.Amount)
                              .ToList();

        var colors = new[] { "#ff5a5a", "#ff9800", "#4CAF50", "#2b72d2", "#9c27b0" };
        var entries = new List<ChartEntry>();

        for (int i = 0; i < grouped.Count; i++)
        {
            var item = grouped[i];
            var percentage = (float)(item.Amount / total * 100);
            
            entries.Add(new ChartEntry((float)item.Amount)
            {
                Label = item.Category,
                ValueLabel = $"{percentage:F0}%",
                Color = SKColor.Parse(colors[i % colors.Length]),
                ValueLabelColor = SKColor.Parse(colors[i % colors.Length])
            });
        }

        ExpenseChart = new DonutChart
        {
            Entries = entries,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 30f,
            HoleRadius = 0.5f
        };
    }

    private void BuildIncomeBreakdown(List<FinanceTransaction> incomes)
    {
        IncomeBreakdown.Clear();
        if (incomes.Count == 0) return;

        var total = incomes.Sum(i => i.Amount);
        var grouped = incomes.GroupBy(i => i.Category)
                             .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                             .OrderByDescending(g => g.Amount)
                             .ToList();

        var colors = new[] { "#3bb54a", "#4CAF50", "#8BC34A", "#CDDC39" };
        
        for (int i = 0; i < grouped.Count; i++)
        {
            var item = grouped[i];
            IncomeBreakdown.Add(new IncomeBreakdownItem
            {
                Name = item.Category,
                Amount = item.Amount,
                Percentage = (double)(item.Amount / total),
                ColorHex = colors[i % colors.Length]
            });
        }
    }

    [RelayCommand]
    public async Task NavigateToProfitSummaryAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.ProfitSummaryPage));
    }
}

public class IncomeBreakdownItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; } // 0.0 to 1.0 for Progress property
    public string ColorHex { get; set; } = "#3bb54a";
}
