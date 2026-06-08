using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using Microcharts;
using SkiaSharp;

namespace KrishiAI.App.ViewModels;

public partial class ProfitSummaryViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private FinancialSummary currentMonthSummary = new();

    [ObservableProperty]
    private Chart profitTrendChart;

    public ProfitSummaryViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    public async Task InitializeAsync()
    {
        await LoadSummaryAsync();
    }

    [RelayCommand]
    public async Task LoadSummaryAsync()
    {
        IsBusy = true;
        try
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            CurrentMonthSummary = await _financeService.GetFinancialSummaryAsync(startOfMonth, endOfMonth);

            await BuildTrendChartAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load summary: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BuildTrendChartAsync()
    {
        var entries = new List<ChartEntry>();
        var now = DateTime.Now;

        for (int i = 5; i >= 0; i--)
        {
            var month = now.AddMonths(-i);
            var start = new DateTime(month.Year, month.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var summary = await _financeService.GetFinancialSummaryAsync(start, end);
            var profit = (float)summary.NetProfit;

            entries.Add(new ChartEntry(profit)
            {
                Label = month.ToString("MMM"),
                ValueLabel = profit.ToString("F0"),
                Color = profit >= 0 ? SKColor.Parse("#3bb54a") : SKColor.Parse("#ff5a5a"),
                ValueLabelColor = SKColor.Parse("#2c2c2c")
            });
        }

        ProfitTrendChart = new LineChart
        {
            Entries = entries,
            LineMode = LineMode.Spline,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            LabelTextSize = 30f,
            BackgroundColor = SKColors.Transparent
        };
    }

    [RelayCommand]
    public async Task NavigateToAiInsightsAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.AiInsightsPage));
    }
}
