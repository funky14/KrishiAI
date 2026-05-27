namespace KrishiAI.App.Models;

public class VoiceCommand
{
    public string CommandText { get; set; } = string.Empty;
    public string Language { get; set; } = "en-US";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Response { get; set; } = string.Empty;
    public TimeSpan AudioDuration { get; set; }
    public bool IsUserMessage { get; set; } = true;
}
