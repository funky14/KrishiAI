namespace KrishiAI.App.Services;

public interface ITextToSpeechService
{
    Task SpeakAsync(string text, string languageCode);
    Task PauseAsync();
    Task ResumeAsync();
    Task StopAsync();
}
