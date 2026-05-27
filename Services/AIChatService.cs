using System.Diagnostics;

namespace KrishiAI.App.Services;

public class AIChatService : IAIChatService
{
    private readonly IConnectivityService _connectivityService;

    public AIChatService(IConnectivityService connectivityService)
    {
        _connectivityService = connectivityService;
    }

    public async Task<string> ProcessQueryAsync(string userQuery, string languageCode)
    {
        try
        {
            if (!_connectivityService.IsConnected())
            {
                return GetOfflineResponse(languageCode);
            }

            // In production, integrate with Azure OpenAI
            // For demo, return mock response
            await Task.Delay(1000); // Simulate API call

            return GetMockResponse(userQuery, languageCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ProcessQueryAsync Error: {ex.Message}");
            return "Sorry, I couldn't process your query at the moment.";
        }
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
