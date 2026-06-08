using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class FinanceVoiceEntryViewModel : BaseViewModel
{
    private readonly IAIChatService _aiService;
    private readonly IFinanceService _financeService;

    [ObservableProperty]
    private string selectedLanguage = "English";

    [ObservableProperty]
    private bool isListening;

    [ObservableProperty]
    private string recognizedText = string.Empty;

    [ObservableProperty]
    private string aiResponseText = string.Empty;

    [ObservableProperty]
    private bool hasResult;

    public List<string> Languages { get; } = new() { "English", "Hindi", "Marathi", "Gujarati" };

    public FinanceVoiceEntryViewModel(IAIChatService aiService, IFinanceService financeService)
    {
        _aiService = aiService;
        _financeService = financeService;
    }

    [RelayCommand]
    public async Task StartListeningAsync()
    {
        if (IsListening) return;

        IsListening = true;
        HasResult = false;
        RecognizedText = "Listening...";
        AiResponseText = string.Empty;

        try
        {
            // Simulate voice recognition delay for prototype
            await Task.Delay(2000);
            
            // For the prototype, if they just tap it, simulate the prompt from the mockup:
            RecognizedText = "500 rupay khaad kharidi";
            
            await ProcessVoiceCommandAsync(RecognizedText);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Voice recognition failed: {ex.Message}", "OK");
        }
        finally
        {
            IsListening = false;
        }
    }

    private async Task ProcessVoiceCommandAsync(string text)
    {
        IsBusy = true;
        try
        {
            // Prepare a prompt for the AI to extract transaction data
            string prompt = $@"
Extract the finance transaction details from this text: '{text}'.
Return ONLY a raw JSON object with the following schema:
{{
  ""Type"": ""Expense"" | ""Income"",
  ""Category"": string (e.g. ""Fertilizer"", ""Seeds"", ""Crop Sale""),
  ""Amount"": number,
  ""Name"": string (e.g. ""khaad"", ""wheat"")
}}
Do not include any markdown formatting, just the raw JSON.
";

            // Get response from AI
            var aiResponse = await _aiService.ProcessQueryAsync(prompt, "en-US");
            
            // Parse JSON
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<JsonElement>(aiResponse.Replace("```json", "").Replace("```", "").Trim());
            
            string type = data.GetProperty("Type").GetString() ?? "Expense";
            string category = data.GetProperty("Category").GetString() ?? "General";
            decimal amount = data.GetProperty("Amount").GetDecimal();
            string name = data.GetProperty("Name").GetString() ?? "";

            // Save to database
            if (type == "Expense")
            {
                var expense = new FinanceTransaction
                {
                    UserId = "user123",
                    TransactionType = "Expense",
                    Category = category,
                    ExpenseCategory = category,
                    ExpenseName = name,
                    Amount = amount,
                    TransactionDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                };
                await _financeService.AddExpenseAsync(expense);
                AiResponseText = $"{name} expense ₹{amount} added successfully.";
            }
            else
            {
                var income = new FinanceTransaction
                {
                    UserId = "user123",
                    TransactionType = "Income",
                    Category = category,
                    CropName = name,
                    Quantity = 1,
                    PricePerUnit = amount,
                    Amount = amount,
                    TransactionDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                };
                await _financeService.AddIncomeAsync(income);
                AiResponseText = $"{name} income ₹{amount} added successfully.";
            }

            HasResult = true;
        }
        catch (Exception ex)
        {
            AiResponseText = "Sorry, I couldn't understand that transaction.";
            System.Diagnostics.Debug.WriteLine($"AI Parse Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void AddAnother()
    {
        RecognizedText = string.Empty;
        AiResponseText = string.Empty;
        HasResult = false;
    }
}
