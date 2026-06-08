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
    private readonly ISpeechRecognitionService _speechService;
    private readonly ITextToSpeechService _ttsService;
    private readonly ILocalizationService _localizationService;
    private readonly List<string> _conversationHistory = new();

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

    public FinanceVoiceEntryViewModel(
        IAIChatService aiService, 
        IFinanceService financeService,
        ISpeechRecognitionService speechService,
        ITextToSpeechService ttsService,
        ILocalizationService localizationService)
    {
        _aiService = aiService;
        _financeService = financeService;
        _speechService = speechService;
        _ttsService = ttsService;
        _localizationService = localizationService;
    }

    [RelayCommand]
    public async Task StartListeningAsync()
    {
        if (IsListening) return;

        IsListening = true;
        HasResult = false;
        RecognizedText = "Listening...";

        try
        {
            var languageCode = _localizationService.GetCurrentLanguageCode();
            var transcription = await _speechService.StartListeningAsync(languageCode);
            
            if (!string.IsNullOrWhiteSpace(transcription))
            {
                RecognizedText = transcription;
                _conversationHistory.Add($"Farmer: {transcription}");
                await ProcessVoiceCommandAsync(transcription, languageCode);
            }
            else
            {
                RecognizedText = "Could not understand. Try again.";
            }
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

    private async Task ProcessVoiceCommandAsync(string text, string languageCode)
    {
        IsBusy = true;
        try
        {
            string historyString = string.Join("\n", _conversationHistory);
            
            string prompt = $@"
You are a Finance Voice Assistant for Indian farmers. Your goal is to collect finance transaction details.
The mandatory fields are:
1. Type (Expense or Income)
2. Category (e.g., Fertilizer, Seeds, Crop Sale, Labor)
3. Amount (number)
4. Name (string, e.g., khaad, wheat)

Here is the conversation history so far:
{historyString}

If you have ALL the mandatory fields based on the history, output ONLY a raw JSON object with this schema:
{{
  ""Type"": ""Expense"" | ""Income"",
  ""Category"": string,
  ""Amount"": number,
  ""Name"": string
}}
DO NOT wrap it in markdown. Just the raw JSON.

If ANY mandatory field is missing, output a friendly question in the user's language asking for the missing information. DO NOT output JSON in this case, just the plain text question.
";

            // Get response from AI
            var aiResponse = await _aiService.ProcessQueryAsync(prompt, languageCode);
            
            var cleanResponse = aiResponse.Replace("```json", "").Replace("```", "").Trim();
            
            if (cleanResponse.StartsWith("{") && cleanResponse.EndsWith("}"))
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<JsonElement>(cleanResponse);
                
                string type = data.GetProperty("Type").GetString() ?? "Expense";
                string category = data.GetProperty("Category").GetString() ?? "General";
                decimal amount = data.GetProperty("Amount").GetDecimal();
                string name = data.GetProperty("Name").GetString() ?? "";

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
                }

                AiResponseText = $"{name} {type.ToLower()} ₹{amount} added successfully.";
                HasResult = true;
                _conversationHistory.Clear();

                await _ttsService.SpeakAsync(AiResponseText, languageCode);
            }
            else
            {
                AiResponseText = aiResponse;
                _conversationHistory.Add($"Assistant: {aiResponse}");
                HasResult = false;
                
                await _ttsService.SpeakAsync(aiResponse, languageCode);
            }
        }
        catch (Exception ex)
        {
            AiResponseText = "Sorry, I couldn't process that.";
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
        _conversationHistory.Clear();
    }

    public async Task SpeakGreetingAsync()
    {
        var languageCode = _localizationService.GetCurrentLanguageCode();
        
        var greetings = new Dictionary<string, string>
        {
            ["en-US"] = "Welcome. What finance transaction would you like to record?",
            ["hi-IN"] = "नमस्ते। आप कौन सा वित्तीय लेन-देन दर्ज करना चाहेंगे?",
            ["mr-IN"] = "नमस्कार. तुम्हाला कोणता आर्थिक व्यवहार नोंदवायचा आहे?",
            ["ta-IN"] = "வணக்கம். நீங்கள் என்ன நிதி பரிவர்த்தனையை பதிவு செய்ய விரும்புகிறீர்கள்?",
            ["te-IN"] = "నమస్కారం. మీరు ఏ ఆర్థిక లావాదేవీని రికార్డ్ చేయాలనుకుంటున్నారు?",
            ["pa-IN"] = "ਸਤਿ ਸ੍ਰੀ ਅਕਾਲ। ਤੁਸੀਂ ਕਿਹੜਾ ਵਿੱਤੀ ਲੈਣ-ਦੇਣ ਰਿਕਾਰਡ ਕਰਨਾ ਚਾਹੋਗੇ?",
            ["gu-IN"] = "નમસ્તે. તમે કયો નાણાકીય વ્યવહાર નોંધવા માંગો છો?",
            ["bn-IN"] = "নমস্কার। আপনি কোন আর্থিক লেনদেন রেকর্ড করতে চান?"
        };

        if (!greetings.TryGetValue(languageCode, out string greeting))
        {
            greeting = greetings["en-US"];
        }

        if (!IsListening && string.IsNullOrEmpty(RecognizedText))
        {
            AiResponseText = greeting;
            await _ttsService.SpeakAsync(greeting, languageCode);
        }
    }
}
