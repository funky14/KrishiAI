using KrishiAI.App.Models.Risk;
using KrishiAI.App.Models.Weather;
using KrishiAI.App.Models.Irrigation;
using System.Diagnostics;

namespace KrishiAI.App.Services.Risk;

/// <summary>
/// Weather risk analysis and detection implementation
/// </summary>
public class RiskAnalysisService : IRiskAnalysisService
{
    private readonly IDatabaseService _database;

    public RiskAnalysisService(IDatabaseService database)
    {
        _database = database;
    }

    public async Task<List<WeatherRisk>> AnalyzeWeatherRisksAsync(
        WeatherForecast weather,
        WeatherThresholds thresholds)
    {
        var risks = new List<WeatherRisk>();

        try
        {
            if (weather.Current == null || weather.DailyForecasts.Count == 0)
                return risks;

            // Check for frost risk
            var frostRisk = CheckFrostRisk(weather, thresholds);
            if (frostRisk != null) risks.Add(frostRisk);

            // Check for heatwave risk
            var heatwaveRisk = CheckHeatwaveRisk(weather, thresholds);
            if (heatwaveRisk != null) risks.Add(heatwaveRisk);

            // Check for heavy rain risk
            var heavyRainRisk = CheckHeavyRainRisk(weather, thresholds);
            if (heavyRainRisk != null) risks.Add(heavyRainRisk);

            // Check for drought risk
            var droughtRisk = CheckDroughtRisk(weather, thresholds);
            if (droughtRisk != null) risks.Add(droughtRisk);

            // Check for strong wind risk
            var windRisk = CheckStrongWindRisk(weather, thresholds);
            if (windRisk != null) risks.Add(windRisk);

            // Save risks to database
            foreach (var risk in risks)
            {
                await _database.SaveWeatherRiskAsync(risk);
            }

            return risks;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AnalyzeWeatherRisksAsync Error: {ex.Message}");
            return risks;
        }
    }

    public async Task<List<WeatherRisk>> GetActiveRisksAsync()
    {
        return await _database.GetActiveWeatherRisksAsync();
    }

    public async Task AcknowledgeRiskAsync(int riskId)
    {
        await _database.AcknowledgeRiskAsync(riskId);
    }

    public async Task CleanupExpiredRisksAsync()
    {
        await _database.DeactivateExpiredRisksAsync();
    }

    public async Task<bool> HasCriticalRisksAsync()
    {
        var risks = await GetActiveRisksAsync();
        return risks.Any(r => r.RiskLevel == RiskLevel.Critical || r.RiskLevel == RiskLevel.High);
    }

    // ===== RISK DETECTION METHODS =====

    private WeatherRisk? CheckFrostRisk(WeatherForecast weather, WeatherThresholds thresholds)
    {
        // Check next 24 hours for frost
        var minTemp = weather.HourlyForecasts
            .Take(24)
            .MinBy(h => h.Temperature)?.Temperature ?? weather.Current!.Temperature;

        if (minTemp <= thresholds.FrostTemperature)
        {
            var criticalFrost = minTemp <= thresholds.CriticalLowTemperature;
            var level = criticalFrost ? RiskLevel.Critical : minTemp <= 2.0 ? RiskLevel.High : RiskLevel.Moderate;

            var frostTime = weather.HourlyForecasts
                .FirstOrDefault(h => h.Temperature <= thresholds.FrostTemperature)?.Timestamp 
                ?? DateTime.Now.AddHours(6);

            return new WeatherRisk
            {
                RiskType = RiskType.Frost,
                RiskLevel = level,
                DetectedAt = DateTime.UtcNow,
                ExpectedAt = frostTime,
                EndsAt = frostTime.AddHours(6),
                Latitude = weather.Latitude,
                Longitude = weather.Longitude,
                LocationName = weather.LocationName,
                Title = criticalFrost ? "⚠️ Critical Frost Warning" : "❄️ Frost Risk",
                Description = $"Temperature expected to drop to {minTemp:F1}°C. Frost conditions likely.",
                RecommendedAction = criticalFrost 
                    ? "URGENT: Cover sensitive crops immediately. Use frost protection measures."
                    : "Protect vulnerable crops. Consider covering seedlings and young plants.",
                TriggerValue = minTemp,
                ThresholdValue = thresholds.FrostTemperature,
                Unit = "°C",
                IsActive = true,
                CropImpact = "Can damage or kill tender plants, flowers, and young seedlings.",
                ConfidenceScore = 90
            };
        }

        return null;
    }

    private WeatherRisk? CheckHeatwaveRisk(WeatherForecast weather, WeatherThresholds thresholds)
    {
        // Check next 3 days for heatwave
        var maxTemp = weather.DailyForecasts
            .Take(3)
            .MaxBy(d => d.TemperatureMax)?.TemperatureMax ?? weather.Current!.Temperature;

        if (maxTemp >= thresholds.HeatwaveTemperature)
        {
            var criticalHeat = maxTemp >= thresholds.CriticalHighTemperature;
            var level = criticalHeat ? RiskLevel.Critical : maxTemp >= 40.0 ? RiskLevel.High : RiskLevel.Moderate;

            var heatDay = weather.DailyForecasts
                .FirstOrDefault(d => d.TemperatureMax >= thresholds.HeatwaveTemperature)?.Date 
                ?? DateTime.Today.AddDays(1);

            return new WeatherRisk
            {
                RiskType = RiskType.Heatwave,
                RiskLevel = level,
                DetectedAt = DateTime.UtcNow,
                ExpectedAt = heatDay,
                EndsAt = heatDay.AddDays(1),
                Latitude = weather.Latitude,
                Longitude = weather.Longitude,
                LocationName = weather.LocationName,
                Title = criticalHeat ? "🔴 Extreme Heat Warning" : "🌡️ Heatwave Alert",
                Description = $"Temperature expected to reach {maxTemp:F1}°C. Extreme heat conditions.",
                RecommendedAction = criticalHeat
                    ? "CRITICAL: Increase irrigation frequency. Provide shade. Monitor crops hourly."
                    : "Increase watering frequency. Irrigate early morning and evening. Monitor for heat stress.",
                TriggerValue = maxTemp,
                ThresholdValue = thresholds.HeatwaveTemperature,
                Unit = "°C",
                IsActive = true,
                CropImpact = "Can cause wilting, reduced yields, flower drop, and crop stress.",
                ConfidenceScore = 88
            };
        }

        return null;
    }

    private WeatherRisk? CheckHeavyRainRisk(WeatherForecast weather, WeatherThresholds thresholds)
    {
        // Check next 48 hours for heavy rain
        var totalRain = weather.HourlyForecasts
            .Take(48)
            .Sum(h => h.Rainfall);

        if (totalRain >= thresholds.HeavyRainThreshold)
        {
            var extremeRain = totalRain >= thresholds.ExtremeRainThreshold;
            var level = extremeRain ? RiskLevel.Critical : totalRain >= 75.0 ? RiskLevel.High : RiskLevel.Moderate;

            var rainStart = weather.HourlyForecasts
                .FirstOrDefault(h => h.Rainfall > 5.0)?.Timestamp 
                ?? DateTime.Now.AddHours(6);

            return new WeatherRisk
            {
                RiskType = RiskType.HeavyRain,
                RiskLevel = level,
                DetectedAt = DateTime.UtcNow,
                ExpectedAt = rainStart,
                EndsAt = rainStart.AddHours(24),
                Latitude = weather.Latitude,
                Longitude = weather.Longitude,
                LocationName = weather.LocationName,
                Title = extremeRain ? "🚨 Extreme Rainfall Warning" : "🌧️ Heavy Rain Alert",
                Description = $"Expected rainfall: {totalRain:F1} mm in next 48 hours.",
                RecommendedAction = extremeRain
                    ? "URGENT: Check drainage. Risk of flooding. Delay all irrigation."
                    : "Delay irrigation. Ensure proper drainage. Protect crops from waterlogging.",
                TriggerValue = totalRain,
                ThresholdValue = thresholds.HeavyRainThreshold,
                Unit = "mm",
                IsActive = true,
                CropImpact = "Can cause waterlogging, root rot, nutrient leaching, and crop lodging.",
                ConfidenceScore = 85
            };
        }

        return null;
    }

    private WeatherRisk? CheckDroughtRisk(WeatherForecast weather, WeatherThresholds thresholds)
    {
        // Check if no significant rain in forecast period
        var daysWithRain = weather.DailyForecasts
            .Count(d => d.Rainfall > thresholds.MinimumSignificantRain);

        var daysWithoutRain = weather.DailyForecasts.Count - daysWithRain;

        if (daysWithoutRain >= thresholds.DroughtDaysThreshold)
        {
            var level = daysWithoutRain >= 10 ? RiskLevel.High : RiskLevel.Moderate;

            return new WeatherRisk
            {
                RiskType = RiskType.Drought,
                RiskLevel = level,
                DetectedAt = DateTime.UtcNow,
                ExpectedAt = DateTime.Today,
                EndsAt = DateTime.Today.AddDays(daysWithoutRain),
                Latitude = weather.Latitude,
                Longitude = weather.Longitude,
                LocationName = weather.LocationName,
                Title = "🏜️ Drought Conditions",
                Description = $"No significant rainfall expected for {daysWithoutRain} days.",
                RecommendedAction = daysWithoutRain >= 10
                    ? "Prepare for extended drought. Increase irrigation. Consider mulching to retain moisture."
                    : "Monitor soil moisture closely. Plan irrigation schedule carefully.",
                TriggerValue = daysWithoutRain,
                ThresholdValue = thresholds.DroughtDaysThreshold,
                Unit = "days",
                IsActive = true,
                CropImpact = "Can lead to water stress, reduced growth, and lower yields.",
                ConfidenceScore = 80
            };
        }

        return null;
    }

    private WeatherRisk? CheckStrongWindRisk(WeatherForecast weather, WeatherThresholds thresholds)
    {
        // Check next 24 hours for strong winds
        var maxWind = weather.HourlyForecasts
            .Take(24)
            .MaxBy(h => h.WindSpeed)?.WindSpeed ?? weather.Current!.WindSpeed;

        if (maxWind >= thresholds.StrongWindThreshold)
        {
            var level = maxWind >= 60.0 ? RiskLevel.High : RiskLevel.Moderate;

            var windTime = weather.HourlyForecasts
                .FirstOrDefault(h => h.WindSpeed >= thresholds.StrongWindThreshold)?.Timestamp 
                ?? DateTime.Now.AddHours(6);

            return new WeatherRisk
            {
                RiskType = RiskType.StrongWind,
                RiskLevel = level,
                DetectedAt = DateTime.UtcNow,
                ExpectedAt = windTime,
                EndsAt = windTime.AddHours(6),
                Latitude = weather.Latitude,
                Longitude = weather.Longitude,
                LocationName = weather.LocationName,
                Title = maxWind >= 60.0 ? "⚠️ High Wind Warning" : "💨 Strong Wind Alert",
                Description = $"Wind speeds expected to reach {maxWind:F1} km/h.",
                RecommendedAction = maxWind >= 60.0
                    ? "Secure loose structures. Stake tall plants. Risk of crop lodging."
                    : "Stake vulnerable plants. Check windbreaks. Monitor for damage.",
                TriggerValue = maxWind,
                ThresholdValue = thresholds.StrongWindThreshold,
                Unit = "km/h",
                IsActive = true,
                CropImpact = "Can cause crop lodging, branch breakage, and fruit drop.",
                ConfidenceScore = 82
            };
        }

        return null;
    }
}
