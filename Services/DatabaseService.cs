using KrishiAI.App.Models;
using KrishiAI.App.Models.Weather;
using KrishiAI.App.Models.Irrigation;
using KrishiAI.App.Models.Risk;
using SQLite;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _database;

    public async Task InitializeAsync()
    {
        try
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "krishiai.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            // Create disease detection tables
            await _database.CreateTableAsync<DiseaseDetectionResult>();

            // Create weather tables
            await _database.CreateTableAsync<WeatherForecast>();
            await _database.CreateTableAsync<CurrentWeather>();
            await _database.CreateTableAsync<HourlyForecast>();
            await _database.CreateTableAsync<DailyForecast>();

            // Create irrigation tables
            await _database.CreateTableAsync<CropInfo>();
            await _database.CreateTableAsync<IrrigationRecommendation>();

            // Create risk tables
            await _database.CreateTableAsync<WeatherRisk>();
            await _database.CreateTableAsync<WeatherAlert>();

            Debug.WriteLine($"Database initialized at: {dbPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"InitializeAsync Error: {ex.Message}");
        }
    }

    public async Task<int> SaveDetectionAsync(DiseaseDetectionResult result)
    {
        await InitializeAsync();
        
        if (result.Id == 0)
        {
            return await _database!.InsertAsync(result);
        }
        else
        {
            return await _database!.UpdateAsync(result);
        }
    }

    public async Task<List<DiseaseDetectionResult>> GetHistoryAsync()
    {
        await InitializeAsync();
        return await _database!.Table<DiseaseDetectionResult>()
            .OrderByDescending(x => x.DetectedDate)
            .ToListAsync();
    }

    public async Task<int> DeleteDetectionAsync(DiseaseDetectionResult result)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync(result);
    }

    public async Task<int> ClearHistoryAsync()
    {
        await InitializeAsync();
        return await _database!.DeleteAllAsync<DiseaseDetectionResult>();
    }

    // ===== SYNC IMPLEMENTATIONS (Phase 2) =====

    public async Task<List<DiseaseDetectionResult>> GetPendingSyncRecordsAsync()
    {
        await InitializeAsync();
        return await _database!.Table<DiseaseDetectionResult>()
            .Where(x => !x.IsSynced && !x.IsDeleted)
            .OrderBy(x => x.SyncRetryCount)
            .ToListAsync();
    }

    public async Task<List<DiseaseDetectionResult>> GetDeletedRecordsPendingSyncAsync()
    {
        await InitializeAsync();
        return await _database!.Table<DiseaseDetectionResult>()
            .Where(x => x.IsDeleted && !string.IsNullOrEmpty(x.RemoteId))
            .ToListAsync();
    }

    public async Task UpdateSyncStatusAsync(DiseaseDetectionResult result, bool isSynced, string? remoteId, string? error = null)
    {
        await InitializeAsync();
        result.IsSynced = isSynced;
        result.LastSyncTime = isSynced ? DateTime.UtcNow : null;
        result.SyncError = error;
        
        if (isSynced)
        {
            result.SyncRetryCount = 0;
            if (!string.IsNullOrEmpty(remoteId))
                result.RemoteId = remoteId;
        }
        else
        {
            result.SyncRetryCount++;
        }

        await _database!.UpdateAsync(result);
    }

    public async Task<int> MergeRemoteChangesAsync(List<DiseaseDetectionResult> remoteRecords)
    {
        await InitializeAsync();
        int merged = 0;

        foreach (var remoteRecord in remoteRecords)
        {
            if (string.IsNullOrEmpty(remoteRecord.RemoteId))
                continue;

            // Find existing local record by remote ID
            var localRecord = await GetByRemoteIdAsync(remoteRecord.RemoteId);

            if (localRecord == null)
            {
                // New record from server - insert locally
                remoteRecord.Id = 0; // Reset local ID so SQLite auto-generates
                remoteRecord.IsSynced = true;
                await _database!.InsertAsync(remoteRecord);
                merged++;
            }
            else
            {
                // Last-write-wins conflict resolution by UTC timestamp
                if (remoteRecord.LastModifiedDateUtc > localRecord.LastModifiedDateUtc)
                {
                    // Server version is newer - update local record
                    remoteRecord.Id = localRecord.Id; // Keep local ID
                    await _database!.UpdateAsync(remoteRecord);
                    merged++;
                }
            }
        }

        return merged;
    }

    public async Task<DateTime?> GetLastSyncAnchorAsync()
    {
        var anchor = await SecureStorage.GetAsync("LastSyncAnchor");
        if (!string.IsNullOrEmpty(anchor) && DateTime.TryParse(anchor, out var result))
            return result;
        return null;
    }

    public async Task SetLastSyncAnchorAsync(DateTime timestamp)
    {
        await SecureStorage.SetAsync("LastSyncAnchor", timestamp.ToUniversalTime().ToString("O"));
    }

    public async Task UpdateRemoteIdAsync(int localId, string remoteId)
    {
        await InitializeAsync();
        var record = await _database!.GetAsync<DiseaseDetectionResult>(localId);
        if (record != null)
        {
            record.RemoteId = remoteId;
            await _database!.UpdateAsync(record);
        }
    }

    public async Task<DiseaseDetectionResult?> GetByRemoteIdAsync(string remoteId)
    {
        await InitializeAsync();
        return await _database!.Table<DiseaseDetectionResult>()
            .FirstOrDefaultAsync(x => x.RemoteId == remoteId);
    }

    public async Task SoftDeleteAsync(DiseaseDetectionResult result)
    {
        await InitializeAsync();
        result.IsDeleted = true;
        result.LastModifiedDateUtc = DateTime.UtcNow;
        await _database!.UpdateAsync(result);
    }

    // ===== WEATHER FORECAST IMPLEMENTATIONS =====

    public async Task<int> SaveWeatherForecastAsync(WeatherForecast forecast)
    {
        await InitializeAsync();

        if (forecast.Id == 0)
        {
            var forecastId = await _database!.InsertAsync(forecast);
            forecast.Id = forecastId;
        }
        else
        {
            await _database!.UpdateAsync(forecast);
        }

        // Save related current weather
        if (forecast.Current != null)
        {
            forecast.Current.ForecastId = forecast.Id;
            if (forecast.Current.Id == 0)
                await _database!.InsertAsync(forecast.Current);
            else
                await _database!.UpdateAsync(forecast.Current);
        }

        // Save hourly forecasts
        foreach (var hourly in forecast.HourlyForecasts)
        {
            hourly.ForecastId = forecast.Id;
            if (hourly.Id == 0)
                await _database!.InsertAsync(hourly);
            else
                await _database!.UpdateAsync(hourly);
        }

        // Save daily forecasts
        foreach (var daily in forecast.DailyForecasts)
        {
            daily.ForecastId = forecast.Id;
            if (daily.Id == 0)
                await _database!.InsertAsync(daily);
            else
                await _database!.UpdateAsync(daily);
        }

        return forecast.Id;
    }

    public async Task<WeatherForecast?> GetWeatherForecastAsync(double latitude, double longitude)
    {
        await InitializeAsync();

        // Find forecast within 0.1 degree (~11km) and not expired
        var forecast = await _database!.Table<WeatherForecast>()
            .Where(x => 
                Math.Abs(x.Latitude - latitude) < 0.1 && 
                Math.Abs(x.Longitude - longitude) < 0.1 &&
                x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.FetchedAt)
            .FirstOrDefaultAsync();

        if (forecast != null)
        {
            await LoadWeatherForecastRelations(forecast);
        }

        return forecast;
    }

    public async Task<WeatherForecast?> GetLatestWeatherForecastAsync()
    {
        await InitializeAsync();

        var forecast = await _database!.Table<WeatherForecast>()
            .OrderByDescending(x => x.FetchedAt)
            .FirstOrDefaultAsync();

        if (forecast != null)
        {
            await LoadWeatherForecastRelations(forecast);
        }

        return forecast;
    }

    private async Task LoadWeatherForecastRelations(WeatherForecast forecast)
    {
        forecast.Current = await _database!.Table<CurrentWeather>()
            .Where(x => x.ForecastId == forecast.Id)
            .FirstOrDefaultAsync();

        forecast.HourlyForecasts = await _database!.Table<HourlyForecast>()
            .Where(x => x.ForecastId == forecast.Id)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        forecast.DailyForecasts = await _database!.Table<DailyForecast>()
            .Where(x => x.ForecastId == forecast.Id)
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<int> DeleteExpiredWeatherForecastsAsync()
    {
        await InitializeAsync();
        var expired = await _database!.Table<WeatherForecast>()
            .Where(x => x.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        int deleted = 0;
        foreach (var forecast in expired)
        {
            // Delete related records
            await _database!.ExecuteAsync("DELETE FROM CurrentWeather WHERE ForecastId = ?", forecast.Id);
            await _database!.ExecuteAsync("DELETE FROM HourlyForecasts WHERE ForecastId = ?", forecast.Id);
            await _database!.ExecuteAsync("DELETE FROM DailyForecasts WHERE ForecastId = ?", forecast.Id);
            await _database!.DeleteAsync(forecast);
            deleted++;
        }

        return deleted;
    }

    // ===== CROP PROFILE IMPLEMENTATIONS =====

    public async Task<int> SaveCropProfileAsync(CropInfo cropInfo)
    {
        await InitializeAsync();
        cropInfo.UpdatedAt = DateTime.UtcNow;

        if (cropInfo.Id == 0)
        {
            return await _database!.InsertAsync(cropInfo);
        }
        else
        {
            return await _database!.UpdateAsync(cropInfo);
        }
    }

    public async Task<CropInfo?> GetActiveCropProfileAsync()
    {
        await InitializeAsync();
        return await _database!.Table<CropInfo>()
            .Where(x => x.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CropInfo>> GetAllCropProfilesAsync()
    {
        await InitializeAsync();
        return await _database!.Table<CropInfo>()
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<int> DeleteCropProfileAsync(CropInfo cropInfo)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync(cropInfo);
    }

    public async Task SetActiveCropAsync(int cropId)
    {
        await InitializeAsync();

        // Deactivate all crops
        var allCrops = await GetAllCropProfilesAsync();
        foreach (var crop in allCrops)
        {
            crop.IsActive = false;
            await _database!.UpdateAsync(crop);
        }

        // Activate selected crop
        var selectedCrop = await _database!.GetAsync<CropInfo>(cropId);
        if (selectedCrop != null)
        {
            selectedCrop.IsActive = true;
            await _database!.UpdateAsync(selectedCrop);
        }
    }

    // ===== IRRIGATION RECOMMENDATION IMPLEMENTATIONS =====

    public async Task<int> SaveIrrigationRecommendationAsync(IrrigationRecommendation recommendation)
    {
        await InitializeAsync();

        if (recommendation.Id == 0)
        {
            return await _database!.InsertAsync(recommendation);
        }
        else
        {
            return await _database!.UpdateAsync(recommendation);
        }
    }

    public async Task<IrrigationRecommendation?> GetLatestIrrigationRecommendationAsync(int cropProfileId)
    {
        await InitializeAsync();
        return await _database!.Table<IrrigationRecommendation>()
            .Where(x => x.CropProfileId == cropProfileId)
            .OrderByDescending(x => x.GeneratedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<IrrigationRecommendation>> GetIrrigationHistoryAsync(int cropProfileId, int limit = 30)
    {
        await InitializeAsync();
        return await _database!.Table<IrrigationRecommendation>()
            .Where(x => x.CropProfileId == cropProfileId)
            .OrderByDescending(x => x.GeneratedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task UpdateRecommendationActionAsync(int recommendationId, bool actioned, string? feedback = null)
    {
        await InitializeAsync();
        var recommendation = await _database!.GetAsync<IrrigationRecommendation>(recommendationId);
        if (recommendation != null)
        {
            recommendation.UserActioned = actioned;
            recommendation.UserFeedback = feedback;
            await _database!.UpdateAsync(recommendation);
        }
    }

    // ===== WEATHER RISK IMPLEMENTATIONS =====

    public async Task<int> SaveWeatherRiskAsync(WeatherRisk risk)
    {
        await InitializeAsync();

        if (risk.Id == 0)
        {
            return await _database!.InsertAsync(risk);
        }
        else
        {
            return await _database!.UpdateAsync(risk);
        }
    }

    public async Task<List<WeatherRisk>> GetActiveWeatherRisksAsync()
    {
        await InitializeAsync();
        return await _database!.Table<WeatherRisk>()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.RiskLevel)
            .ThenBy(x => x.ExpectedAt)
            .ToListAsync();
    }

    public async Task AcknowledgeRiskAsync(int riskId)
    {
        await InitializeAsync();
        var risk = await _database!.GetAsync<WeatherRisk>(riskId);
        if (risk != null)
        {
            risk.IsAcknowledged = true;
            await _database!.UpdateAsync(risk);
        }
    }

    public async Task<int> DeactivateExpiredRisksAsync()
    {
        await InitializeAsync();
        var expired = await _database!.Table<WeatherRisk>()
            .Where(x => x.IsActive && x.EndsAt < DateTime.UtcNow)
            .ToListAsync();

        foreach (var risk in expired)
        {
            risk.IsActive = false;
            await _database!.UpdateAsync(risk);
        }

        return expired.Count;
    }

    // ===== WEATHER ALERT IMPLEMENTATIONS =====

    public async Task<int> SaveWeatherAlertAsync(WeatherAlert alert)
    {
        await InitializeAsync();

        if (alert.Id == 0)
        {
            return await _database!.InsertAsync(alert);
        }
        else
        {
            return await _database!.UpdateAsync(alert);
        }
    }

    public async Task<List<WeatherAlert>> GetPendingAlertsAsync()
    {
        await InitializeAsync();
        return await _database!.Table<WeatherAlert>()
            .Where(x => !x.IsShown && !x.IsDismissed)
            .OrderBy(x => x.ScheduledAt)
            .ToListAsync();
    }

    public async Task MarkAlertAsShownAsync(int alertId)
    {
        await InitializeAsync();
        var alert = await _database!.GetAsync<WeatherAlert>(alertId);
        if (alert != null)
        {
            alert.IsShown = true;
            await _database!.UpdateAsync(alert);
        }
    }

    public async Task DismissAlertAsync(int alertId)
    {
        await InitializeAsync();
        var alert = await _database!.GetAsync<WeatherAlert>(alertId);
        if (alert != null)
        {
            alert.IsDismissed = true;
            await _database!.UpdateAsync(alert);
        }
    }
}
