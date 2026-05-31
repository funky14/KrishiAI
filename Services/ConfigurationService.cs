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
                System.Diagnostics.Debug.WriteLine($"⚠️ Config file not found, copying from bundled resources...");
                
                // Try to copy bundled config file from Resources/Raw
                await CopyBundledConfigAsync(configPath);
                
                // Load the copied file
                if (File.Exists(configPath))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Bundled config copied successfully");
                    var json = await File.ReadAllTextAsync(configPath);
                    _cachedConfig = JsonSerializer.Deserialize<AzureConfiguration>(json) ?? new AzureConfiguration();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ No bundled config, creating default...");
                    _cachedConfig = new AzureConfiguration();
                    await SaveConfigurationAsync(_cachedConfig);
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Config file initialized");
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

    private async Task CopyBundledConfigAsync(string targetPath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📦 Attempting to copy bundled config...");
            
            using var stream = await FileSystem.OpenAppPackageFileAsync(ConfigFileName);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            
            await File.WriteAllTextAsync(targetPath, json);
            
            System.Diagnostics.Debug.WriteLine($"✅ Bundled config copied ({json.Length} bytes)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Could not copy bundled config: {ex.Message}");
        }
    }

    public async Task SaveConfigurationAsync(AzureConfiguration config)
    {
        try
        {
            var configPath = Path.Combine(FileSystem.AppDataDirectory, ConfigFileName);
            System.Diagnostics.Debug.WriteLine($"💾 SaveConfigurationAsync STARTED");
            System.Diagnostics.Debug.WriteLine($"   Target path: {configPath}");
            System.Diagnostics.Debug.WriteLine($"   Directory exists: {Directory.Exists(FileSystem.AppDataDirectory)}");
            
            // Ensure directory exists
            Directory.CreateDirectory(FileSystem.AppDataDirectory);
            
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            System.Diagnostics.Debug.WriteLine($"   JSON serialized: {json.Length} bytes");
            
            await File.WriteAllTextAsync(configPath, json);
            _cachedConfig = config;
            
            // Verify file was created
            bool fileExists = File.Exists(configPath);
            long fileSize = fileExists ? new FileInfo(configPath).Length : 0;
            
            System.Diagnostics.Debug.WriteLine($"✅ Config saved successfully!");
            System.Diagnostics.Debug.WriteLine($"   File exists: {fileExists}");
            System.Diagnostics.Debug.WriteLine($"   File size: {fileSize} bytes");
            System.Diagnostics.Debug.WriteLine($"   Content preview: {json.Substring(0, Math.Min(100, json.Length))}...");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ SaveConfigurationAsync FAILED!");
            System.Diagnostics.Debug.WriteLine($"   Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
        }
    }
}
