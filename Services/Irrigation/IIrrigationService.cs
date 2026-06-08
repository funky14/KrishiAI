using KrishiAI.App.Models.Irrigation;
using KrishiAI.App.Models.Weather;

namespace KrishiAI.App.Services.Irrigation;

/// <summary>
/// AI-powered irrigation recommendation service
/// </summary>
public interface IIrrigationService
{
    /// <summary>Generate irrigation recommendation for crop based on weather and conditions</summary>
    Task<IrrigationRecommendation?> GenerateRecommendationAsync(
        CropInfo crop,
        WeatherForecast weather,
        double? currentSoilMoisture = null);

    /// <summary>Get latest recommendation for active crop</summary>
    Task<IrrigationRecommendation?> GetLatestRecommendationAsync();

    /// <summary>Get irrigation history for crop</summary>
    Task<List<IrrigationRecommendation>> GetHistoryAsync(int cropProfileId, int limit = 30);

    /// <summary>Mark recommendation as completed by user</summary>
    Task MarkAsActionedAsync(int recommendationId, string? feedback = null);

    /// <summary>Calculate water requirement for crop based on conditions</summary>
    double CalculateWaterRequirement(
        CropInfo crop,
        double temperature,
        double humidity,
        double expectedRainfall);

    /// <summary>Determine best irrigation time</summary>
    (DateTime scheduledTime, string timeDescription) DetermineBestIrrigationTime(
        WeatherForecast weather,
        bool urgentIrrigation);
}
