using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using KrishiAI.App.Models;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class AIChatService : IAIChatService
{
    private readonly IConnectivityService _connectivityService;
    private readonly IConfigurationService _configService;

    public AIChatService(IConnectivityService connectivityService, IConfigurationService configService)
    {
        _connectivityService = connectivityService;
        _configService = configService;
    }

    public async Task<string> ProcessQueryAsync(string userQuery, string languageCode)
    {
        try
        {
            if (!_connectivityService.IsConnected())
            {
                return GetOfflineResponse(languageCode);
            }

            var config = await _configService.GetConfigurationAsync();

            // Use real Azure OpenAI if configured
            if (config.UseRealAIChat && 
                !string.IsNullOrEmpty(config.OpenAIKey) && 
                !string.IsNullOrEmpty(config.OpenAIEndpoint))
            {
                return await GetAzureOpenAIResponseAsync(userQuery, languageCode, config);
            }

            // Fallback to mock for testing
            Debug.WriteLine("⚠️ Using mock AI chat (Azure OpenAI not configured)");
            await Task.Delay(1000); // Simulate API call
            return GetMockResponse(userQuery, languageCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ProcessQueryAsync Error: {ex.Message}");
            return "Sorry, I couldn't process your query at the moment.";
        }
    }

    private async Task<string> GetAzureOpenAIResponseAsync(string userQuery, string languageCode, AzureConfiguration config)
    {
        try
        {
            var client = new AzureOpenAIClient(
                new Uri(config.OpenAIEndpoint),
                new AzureKeyCredential(config.OpenAIKey));

            var chatClient = client.GetChatClient(config.OpenAIDeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(GetSystemPrompt(languageCode)),
                new UserChatMessage(userQuery)
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 500
            };

            Debug.WriteLine($"🤖 Sending query to Azure OpenAI...");
            var response = await chatClient.CompleteChatAsync(messages, options);
            var answer = response.Value.Content[0].Text;
            
            Debug.WriteLine($"✅ Received AI response ({answer.Length} chars)");
            return answer;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAzureOpenAIResponseAsync Error: {ex.Message}");
            return GetMockResponse(userQuery, languageCode);
        }
    }

    private string GetSystemPrompt(string languageCode)
    {
        var languageInstructions = languageCode.StartsWith("hi") ? "Respond in Hindi." :
                                   languageCode.StartsWith("mr") ? "Respond in Marathi." :
                                   languageCode.StartsWith("ta") ? "Respond in Tamil." :
                                   languageCode.StartsWith("te") ? "Respond in Telugu." :
                                   languageCode.StartsWith("pa") ? "Respond in Punjabi." :
                                   languageCode.StartsWith("gu") ? "Respond in Gujarati." :
                                   languageCode.StartsWith("bn") ? "Respond in Bengali." :
                                   "Respond in English.";

        return $@"You are KrishiAI, an expert agricultural assistant for Indian farmers. 
Provide practical, actionable farming advice focusing on:
- Crop disease identification and treatment
- Pest management (organic and chemical solutions)
- Soil health and fertilization
- Irrigation and water management
- Weather-based farming recommendations
- Government schemes for farmers

Keep responses concise (2-3 sentences), practical, and farmer-friendly.
{languageInstructions}";
    }

    private string GetOfflineResponse(string languageCode)
    {
        var offlineResponses = new Dictionary<string, string>
        {
            ["en-US"] = "I'm currently offline. Please check your internet connection to get AI-powered advice.",
            ["hi-IN"] = "मैं अभी ऑफलाइन हूं। कृपया अपना इंटरनेट कनेक्शन जांचें।"
        };

        return offlineResponses.TryGetValue(languageCode, out var response) 
            ? response 
            : offlineResponses["en-US"];
    }

    private string GetMockResponse(string query, string languageCode)
    {
        var responses = new Dictionary<string, string>
        {
            ["en-US"] = "Based on your description, this could be a nutrient deficiency or early signs of disease. I recommend: 1) Upload a photo for disease detection 2) Check soil moisture 3) Apply balanced fertilizer. For specific treatment, please use the Disease Detection feature.",
            ["hi-IN"] = "आपके विवरण के आधार पर, यह पोषक तत्वों की कमी या रोग के शुरुआती संकेत हो सकते हैं। मैं सुझाव देता हूं: 1) रोग पहचान के लिए फोटो अपलोड करें 2) मिट्टी की नमी जांचें 3) संतुलित उर्वरक डालें।"
        };

        return responses.TryGetValue(languageCode, out var response) 
            ? response 
            : responses["en-US"];
    }
}
