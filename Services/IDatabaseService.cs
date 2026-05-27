using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<int> SaveDetectionAsync(DiseaseDetectionResult result);
    Task<List<DiseaseDetectionResult>> GetHistoryAsync();
    Task<int> DeleteDetectionAsync(DiseaseDetectionResult result);
    Task<int> ClearHistoryAsync();
}
