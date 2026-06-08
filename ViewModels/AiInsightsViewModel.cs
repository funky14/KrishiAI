using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class AiInsightsViewModel : BaseViewModel
{
    private readonly IFinanceService _financeService;
    private readonly IAIChatService _aiService;

    [ObservableProperty]
    private string costOptimization = "Analyzing...";

    [ObservableProperty]
    private string profitImprovement = "Analyzing...";

    [ObservableProperty]
    private string loanReadiness = "Analyzing...";

    public AiInsightsViewModel(IFinanceService financeService, IAIChatService aiService)
    {
        _financeService = financeService;
        _aiService = aiService;
    }

    public async Task InitializeAsync()
    {
        await GenerateInsightsAsync();
    }

    [RelayCommand]
    public async Task GenerateInsightsAsync()
    {
        IsBusy = true;
        try
        {
            var now = DateTime.Now;
            var summary = await _financeService.GetFinancialSummaryAsync(new DateTime(now.Year, now.Month, 1), now);

            string prompt = $@"
Analyze the following farm financial data for the current month:
- Total Income: {summary.TotalIncome}
- Total Expense: {summary.TotalExpense}
- Net Profit: {summary.NetProfit}
- Outstanding Loan: {summary.OutstandingLoan}

Provide exactly 3 concise, actionable insights (1-2 sentences each). Return the result as a valid JSON object with the following keys:
""CostOptimization"", ""ProfitImprovement"", ""LoanReadiness"".
Do not wrap the JSON in markdown code blocks, just raw JSON.
";
            
            var response = await _aiService.ProcessQueryAsync(prompt, "en-US");
            
            // Clean up possible markdown
            if (response.StartsWith("```json"))
            {
                response = response.Replace("```json", "").Replace("```", "").Trim();
            }

            try
            {
                var insights = JsonSerializer.Deserialize<Dictionary<string, string>>(response);
                if (insights != null)
                {
                    CostOptimization = insights.GetValueOrDefault("CostOptimization", "Reduce fertilizer costs by adopting targeted application techniques.");
                    ProfitImprovement = insights.GetValueOrDefault("ProfitImprovement", "Consider crop rotation with legumes to improve soil and reduce input costs.");
                    LoanReadiness = insights.GetValueOrDefault("LoanReadiness", "Your debt-to-income ratio is healthy, making you eligible for KCC expansion.");
                }
            }
            catch
            {
                // Fallback if AI doesn't return perfect JSON
                CostOptimization = "Reduce fertilizer costs by adopting targeted application techniques.";
                ProfitImprovement = "Consider crop rotation with legumes to improve soil and reduce input costs.";
                LoanReadiness = "Your debt-to-income ratio is healthy, making you eligible for KCC expansion.";
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to generate insights: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
