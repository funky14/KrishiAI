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
            System.Diagnostics.Debug.WriteLine($"📁 Config path: {configPath}");
            
            if (File.Exists(configPath))
            {
                System.Diagnostics.Debug.WriteLine($"✅ Config file exists, loading...");
                var json = await File.ReadAllTextAsync(configPath);
                _cachedConfig = JsonSerializer.Deserialize<AzureConfiguration>(json) ?? new AzureConfiguration();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Config file not found, creating new...");
                _cachedConfig = new AzureConfiguration();
                await SaveConfigurationAsync(_cachedConfig);
                System.Diagnostics.Debug.WriteLine($"✅ New config file created");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ GetConfigurationAsync Error: {ex.Message}");
            _cachedConfig = new AzureConfiguration();
            await SaveConfigurationAsync(_cachedConfig);
        }

        return _cachedConfig;
    }

    public async Task SaveConfigurationAsync(AzureConfiguration config)
    {
        try
        {
            var configPath = Path.Combine(FileSystem.AppDataDirectory, ConfigFileName);
            System.Diagnostics.Debug.WriteLine($"💾 Saving config to: {configPath}");
            
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configPath, json);
            _cachedConfig = config;
            
            System.Diagnostics.Debug.WriteLine($"✅ Config saved successfully ({json.Length} bytes)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ SaveConfigurationAsync Error: {ex.Message}");
        }
    }
}
