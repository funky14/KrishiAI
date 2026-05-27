namespace KrishiAI.App.Models;

public class AppSettings
{
    public string PreferredLanguage { get; set; } = "en-US";
    public bool SaveHistory { get; set; } = true;
    public bool CacheImages { get; set; } = true;
    public int MaxHistoryItems { get; set; } = 100;
    public bool AutoPlayResponses { get; set; } = true;
    public string Theme { get; set; } = "Light";
}
