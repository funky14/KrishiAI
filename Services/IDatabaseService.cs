using KrishiAI.App.Models;
using KrishiAI.App.Models.Weather;
using KrishiAI.App.Models.Irrigation;
using KrishiAI.App.Models.Risk;

namespace KrishiAI.App.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<int> SaveDetectionAsync(DiseaseDetectionResult result);
    Task<List<DiseaseDetectionResult>> GetHistoryAsync();
    Task<int> DeleteDetectionAsync(DiseaseDetectionResult result);
    Task<int> ClearHistoryAsync();

    // ===== SYNC-SPECIFIC METHODS (Phase 2) =====
    /// <summary>Get all records pending sync (create, update, or delete)</summary>
    Task<List<DiseaseDetectionResult>> GetPendingSyncRecordsAsync();

    /// <summary>Get records marked for deletion (soft-delete sync queue)</summary>
    Task<List<DiseaseDetectionResult>> GetDeletedRecordsPendingSyncAsync();

    /// <summary>Update sync status after successful push to server</summary>
    Task UpdateSyncStatusAsync(DiseaseDetectionResult result, bool isSynced, string? remoteId, string? error = null);

    /// <summary>Merge remote changes into local store (for delta pull)</summary>
    Task<int> MergeRemoteChangesAsync(List<DiseaseDetectionResult> remoteRecords);

    /// <summary>Get sync anchor (timestamp of last successful pull)</summary>
    Task<DateTime?> GetLastSyncAnchorAsync();

    /// <summary>Update sync anchor after successful pull</summary>
    Task SetLastSyncAnchorAsync(DateTime timestamp);

    /// <summary>Update remote ID mapping for a local record</summary>
    Task UpdateRemoteIdAsync(int localId, string remoteId);

    /// <summary>Find local record by remote ID</summary>
    Task<DiseaseDetectionResult?> GetByRemoteIdAsync(string remoteId);

    /// <summary>Soft delete a record (mark for sync, don't hard delete)</summary>
    Task SoftDeleteAsync(DiseaseDetectionResult result);

    // ===== WEATHER FORECAST METHODS =====
    /// <summary>Save weather forecast to cache</summary>
    Task<int> SaveWeatherForecastAsync(WeatherForecast forecast);

    /// <summary>Get cached weather forecast for location</summary>
    Task<WeatherForecast?> GetWeatherForecastAsync(double latitude, double longitude);

    /// <summary>Get most recent weather forecast</summary>
    Task<WeatherForecast?> GetLatestWeatherForecastAsync();

    /// <summary>Delete expired weather forecasts</summary>
    Task<int> DeleteExpiredWeatherForecastsAsync();

    // ===== CROP PROFILE METHODS =====
    /// <summary>Save or update crop profile</summary>
    Task<int> SaveCropProfileAsync(CropInfo cropInfo);

    /// <summary>Get active crop profile</summary>
    Task<CropInfo?> GetActiveCropProfileAsync();

    /// <summary>Get all crop profiles</summary>
    Task<List<CropInfo>> GetAllCropProfilesAsync();

    /// <summary>Delete crop profile</summary>
    Task<int> DeleteCropProfileAsync(CropInfo cropInfo);

    /// <summary>Set crop as active</summary>
    Task SetActiveCropAsync(int cropId);

    // ===== IRRIGATION RECOMMENDATION METHODS =====
    /// <summary>Save irrigation recommendation</summary>
    Task<int> SaveIrrigationRecommendationAsync(IrrigationRecommendation recommendation);

    /// <summary>Get latest recommendation for crop</summary>
    Task<IrrigationRecommendation?> GetLatestIrrigationRecommendationAsync(int cropProfileId);

    /// <summary>Get irrigation history for crop</summary>
    Task<List<IrrigationRecommendation>> GetIrrigationHistoryAsync(int cropProfileId, int limit = 30);

    /// <summary>Mark recommendation as actioned</summary>
    Task UpdateRecommendationActionAsync(int recommendationId, bool actioned, string? feedback = null);

    // ===== WEATHER RISK METHODS =====
    /// <summary>Save weather risk</summary>
    Task<int> SaveWeatherRiskAsync(WeatherRisk risk);

    /// <summary>Get active weather risks</summary>
    Task<List<WeatherRisk>> GetActiveWeatherRisksAsync();

    /// <summary>Acknowledge weather risk</summary>
    Task AcknowledgeRiskAsync(int riskId);

    /// <summary>Deactivate expired risks</summary>
    Task<int> DeactivateExpiredRisksAsync();

    // ===== WEATHER ALERT METHODS =====
    /// <summary>Save weather alert</summary>
    Task<int> SaveWeatherAlertAsync(WeatherAlert alert);

    /// <summary>Get pending alerts (not shown yet)</summary>
    Task<List<WeatherAlert>> GetPendingAlertsAsync();

    /// <summary>Mark alert as shown</summary>
    Task MarkAlertAsShownAsync(int alertId);

    /// <summary>Dismiss alert</summary>
    Task DismissAlertAsync(int alertId);
}
