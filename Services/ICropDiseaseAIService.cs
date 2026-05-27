using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface ICropDiseaseAIService
{
    Task InitializeAsync();
    Task<DiseaseDetectionResult?> PredictDiseaseAsync(string imagePath);
}
