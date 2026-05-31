namespace KrishiAI.App.Models;

public class AzureConfiguration
{
    // Azure Speech Service
    public string SpeechServiceKey { get; set; } = string.Empty;
    public string SpeechServiceRegion { get; set; } = "centralindia";

    // Azure OpenAI Service
    public string OpenAIEndpoint { get; set; } = string.Empty;
    public string OpenAIKey { get; set; } = string.Empty;
    public string OpenAIDeploymentName { get; set; } = "gpt-5";

    // Feature Flags
    public bool UseRealSpeechRecognition { get; set; } = false;
    public bool UseRealAIChat { get; set; } = false;
    public bool UseRealImageProcessing { get; set; } = true;
}
