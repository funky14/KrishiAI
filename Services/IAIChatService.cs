namespace KrishiAI.App.Services;

public interface IAIChatService
{
    Task<string> ProcessQueryAsync(string userQuery, string languageCode);
}
