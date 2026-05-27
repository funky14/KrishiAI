using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface IRecommendationService
{
    Task<DiseaseRecommendation?> GetRecommendationAsync(string diseaseName);
    Task InitializeAsync();
}
