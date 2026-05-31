using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface IConfigurationService
{
    Task<AzureConfiguration> GetConfigurationAsync();
    Task SaveConfigurationAsync(AzureConfiguration config);
}
