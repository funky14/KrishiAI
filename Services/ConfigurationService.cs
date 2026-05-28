using KrishiAI.App.Models;
using System.Text.Json;

namespace KrishiAI.App.Services;

public class ConfigurationService : IConfigurationService
{
    private const string ConfigFileName = "azure_config.json";
    private AzureConfiguration? _cachedConfig;

    public async Task<AzureConfiguration> GetConfigurationAsync()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        try
        {
            var configPath = Path.Combine(FileSystem.AppDataDirectory, ConfigFileName);
            
            if (File.Exists(configPath))
            {
                var json = await File.ReadAllTextAsync(configPath);
                _cachedConfig = JsonSerializer.Deserialize<AzureConfiguration>(json) ?? new AzureConfiguration();
            }
            else
            {
                _cachedConfig = new AzureConfiguration();
                await SaveConfigurationAsync(_cachedConfig);
            }
        }
        catch
        {
            _cachedConfig = new AzureConfiguration();
        }

        return _cachedConfig;
    }

    public async Task SaveConfigurationAsync(AzureConfiguration config)
    {
        try
        {
            var configPath = Path.Combine(FileSystem.AppDataDirectory, ConfigFileName);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configPath, json);
            _cachedConfig = config;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveConfigurationAsync Error: {ex.Message}");
        }
    }
}
