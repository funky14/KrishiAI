using KrishiAI.App.Models.Risk;
using KrishiAI.App.Models.Weather;

namespace KrishiAI.App.Services.Risk;

/// <summary>
/// Weather risk analysis and detection service
/// </summary>
public interface IRiskAnalysisService
{
    /// <summary>Analyze weather forecast for potential risks</summary>
    Task<List<WeatherRisk>> AnalyzeWeatherRisksAsync(WeatherForecast weather, WeatherThresholds thresholds);

    /// <summary>Get active weather risks</summary>
    Task<List<WeatherRisk>> GetActiveRisksAsync();

    /// <summary>Acknowledge a risk (mark as seen by user)</summary>
    Task AcknowledgeRiskAsync(int riskId);

    /// <summary>Clean up expired risks</summary>
    Task CleanupExpiredRisksAsync();

    /// <summary>Check if any critical risks exist</summary>
    Task<bool> HasCriticalRisksAsync();
}
