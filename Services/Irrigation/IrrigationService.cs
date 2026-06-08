using KrishiAI.App.Models.Irrigation;
using KrishiAI.App.Models.Weather;
using System.Diagnostics;

namespace KrishiAI.App.Services.Irrigation;

/// <summary>
/// AI-powered irrigation recommendation engine
/// </summary>
public class IrrigationService : IIrrigationService
{
    private readonly IDatabaseService _database;
    private readonly IAIChatService _aiService;

    public IrrigationService(IDatabaseService database, IAIChatService aiService)
    {
        _database = database;
        _aiService = aiService;
    }

    public async Task<IrrigationRecommendation?> GenerateRecommendationAsync(
        CropInfo crop,
        WeatherForecast weather,
        double? currentSoilMoisture = null)
    {
        try
        {
            if (weather.Current == null || weather.DailyForecasts.Count == 0)
            {
                Debug.WriteLine("Insufficient weather data for irrigation recommendation");
                return null;
            }

            // Use provided soil moisture or crop's current value
            var soilMoisture = currentSoilMoisture ?? crop.SoilMoisturePercentage;

            // Calculate expected rainfall in next 24 hours
            var expected24hRainfall = weather.HourlyForecasts
                .Take(24)
                .Sum(h => h.Rainfall);

            // Get weather conditions
            var temperature = weather.Current.Temperature;
            var humidity = weather.Current.Humidity;

            // Calculate water requirement
            var waterRequired = CalculateWaterRequirement(crop, temperature, humidity, expected24hRainfall);

            // Apply growth stage multiplier
            var stageMultiplier = crop.GrowthStage.GetWaterMultiplier();
            waterRequired *= stageMultiplier;

            // Decision logic
            var shouldIrrigate = ShouldIrrigate(
                soilMoisture,
                crop.OptimalSoilMoistureMin,
                expected24hRainfall,
                temperature,
                crop);

            // Calculate irrigation amount
            var irrigationAmount = CalculateIrrigationAmount(
                soilMoisture,
                crop.OptimalSoilMoistureMax,
                waterRequired,
                expected24hRainfall);

            // Convert to hours (assuming ~5mm/hour irrigation rate)
            var irrigationHours = irrigationAmount / 5.0;

            // Determine best time
            var (scheduledTime, timeDescription) = DetermineBestIrrigationTime(weather, shouldIrrigate && soilMoisture < crop.MinSoilMoisture);

            // Generate messages
            var (message, explanation, reason, priority) = GenerateRecommendationMessages(
                shouldIrrigate,
                irrigationAmount,
                irrigationHours,
                soilMoisture,
                crop,
                expected24hRainfall,
                temperature,
                weather);

            // Create recommendation
            var recommendation = new IrrigationRecommendation
            {
                CropProfileId = crop.Id,
                GeneratedAt = DateTime.UtcNow,
                ShouldIrrigate = shouldIrrigate,
                IrrigationDurationHours = Math.Round(irrigationHours, 1),
                IrrigationAmountMm = Math.Round(irrigationAmount, 1),
                BestIrrigationTime = timeDescription,
                ScheduledTime = scheduledTime,
                RecommendationMessage = message,
                DetailedExplanation = explanation,
                Reason = reason,
                CurrentSoilMoisture = soilMoisture,
                TargetSoilMoisture = crop.OptimalSoilMoistureMax,
                ExpectedRainfall24h = Math.Round(expected24hRainfall, 1),
                CurrentTemperature = temperature,
                WeatherCondition = weather.Current.Description,
                Priority = priority,
                WaterSavingPercentage = CalculateWaterSaving(irrigationAmount, waterRequired),
                ConfidenceScore = CalculateConfidenceScore(weather, soilMoisture, crop),
                ValidUntil = DateTime.UtcNow.AddHours(12)
            };

            // Optionally enrich recommendation with AI-driven explanation / adjustments
            try
            {
                var lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
                var aiPrompt = BuildAIPromptForIrrigation(recommendation, crop, weather);
                var aiResponse = await _aiService.ProcessQueryAsync(aiPrompt, lang);

                var json = ExtractJson(aiResponse);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        var map = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        if (map != null)
                        {
                            if (map.TryGetValue("recommendation", out var rec)) recommendation.RecommendationMessage = rec;
                            if (map.TryGetValue("explanation", out var expl)) recommendation.DetailedExplanation = expl;
                            if (map.TryGetValue("confidence", out var conf) && double.TryParse(conf, out var confVal))
                                recommendation.ConfidenceScore = Math.Max(0, Math.Min(100, confVal));
                        }
                    }
                    catch
                    {
                        // ignore parse errors and keep generated recommendation
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AI enrichment failed: {ex.Message}");
            }

            // Save to database
            await _database.SaveIrrigationRecommendationAsync(recommendation);

            return recommendation;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GenerateRecommendationAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task<IrrigationRecommendation?> GetLatestRecommendationAsync()
    {
        try
        {
            var activeCrop = await _database.GetActiveCropProfileAsync();
            if (activeCrop == null)
                return null;

            return await _database.GetLatestIrrigationRecommendationAsync(activeCrop.Id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetLatestRecommendationAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<IrrigationRecommendation>> GetHistoryAsync(int cropProfileId, int limit = 30)
    {
        return await _database.GetIrrigationHistoryAsync(cropProfileId, limit);
    }

    public async Task MarkAsActionedAsync(int recommendationId, string? feedback = null)
    {
        await _database.UpdateRecommendationActionAsync(recommendationId, true, feedback);
    }

    public double CalculateWaterRequirement(
        CropInfo crop,
        double temperature,
        double humidity,
        double expectedRainfall)
    {
        // Base water requirement from crop
        var baseRequirement = crop.BaseWaterRequirement;

        // Temperature adjustment (higher temp = more water)
        var tempFactor = temperature > 30 ? 1.3 : temperature > 25 ? 1.1 : 1.0;

        // Humidity adjustment (lower humidity = more water)
        var humidityFactor = humidity < 40 ? 1.2 : humidity < 60 ? 1.0 : 0.9;

        // Calculate adjusted requirement
        var adjustedRequirement = baseRequirement * tempFactor * humidityFactor;

        // Subtract expected rainfall
        var netRequirement = Math.Max(0, adjustedRequirement - expectedRainfall);

        return netRequirement;
    }

    public (DateTime scheduledTime, string timeDescription) DetermineBestIrrigationTime(
        WeatherForecast weather,
        bool urgentIrrigation)
    {
        var now = DateTime.Now;

        if (urgentIrrigation)
        {
            // Irrigate as soon as possible
            if (now.Hour < 6 || now.Hour > 18)
            {
                // It's night, recommend early morning
                var tomorrow = now.Date.AddDays(1).AddHours(6);
                return (tomorrow, "Tomorrow morning (6 AM)");
            }
            else
            {
                // During day, recommend evening
                var today = now.Date.AddHours(18);
                return (today, "This evening (6 PM)");
            }
        }

        // Check for rain in next 24 hours
        var rainHours = weather.HourlyForecasts
            .Take(24)
            .Where(h => h.Rainfall > 2.0)
            .Select(h => h.Timestamp)
            .ToList();

        if (rainHours.Any())
        {
            // Rain expected, schedule after rain
            var lastRain = rainHours.Max();
            var afterRain = lastRain.AddHours(3);
            return (afterRain, $"After expected rain on {afterRain:MMM dd, h tt}");
        }

        // No urgent need, schedule for tomorrow morning
        var nextMorning = now.Date.AddDays(1).AddHours(6);
        return (nextMorning, "Tomorrow morning (6 AM)");
    }

    private bool ShouldIrrigate(
        double soilMoisture,
        double optimalMin,
        double expectedRainfall,
        double temperature,
        CropInfo crop)
    {
        // Critical: Below minimum threshold
        if (soilMoisture < crop.MinSoilMoisture)
            return true;

        // Below optimal and no significant rain expected
        if (soilMoisture < optimalMin && expectedRainfall < 5.0)
            return true;

        // Hot weather and below optimal
        if (temperature > 35 && soilMoisture < optimalMin + 5)
            return true;

        return false;
    }

    private double CalculateIrrigationAmount(
        double currentMoisture,
        double targetMoisture,
        double baseRequirement,
        double expectedRainfall)
    {
        // Calculate moisture deficit
        var deficit = Math.Max(0, targetMoisture - currentMoisture);

        // Convert deficit to mm (rough approximation: 1% moisture ≈ 10mm water for top 1m soil)
        var deficitMm = deficit * 10;

        // Add base requirement and subtract expected rainfall
        var totalRequired = deficitMm + baseRequirement - expectedRainfall;

        return Math.Max(0, totalRequired);
    }

    private (string message, string explanation, string reason, string priority) GenerateRecommendationMessages(
        bool shouldIrrigate,
        double amount,
        double hours,
        double soilMoisture,
        CropInfo crop,
        double expectedRain,
        double temperature,
        WeatherForecast weather)
    {
        string message, explanation, reason, priority;

        if (!shouldIrrigate)
        {
            if (expectedRain > 10)
            {
                message = $"Do not irrigate. Heavy rain expected ({expectedRain:F1} mm).";
                reason = "Sufficient rainfall expected";
                priority = "Low";
            }
            else if (soilMoisture >= crop.OptimalSoilMoistureMin)
            {
                message = "Soil moisture is optimal. No irrigation needed.";
                reason = "Adequate soil moisture";
                priority = "Low";
            }
            else
            {
                message = "Monitor soil moisture. Irrigation may be needed soon.";
                reason = "Approaching irrigation threshold";
                priority = "Medium";
            }

            explanation = $"Current soil moisture: {soilMoisture:F0}%. " +
                         $"Optimal range: {crop.OptimalSoilMoistureMin:F0}-{crop.OptimalSoilMoistureMax:F0}%. " +
                         $"Expected rainfall: {expectedRain:F1} mm.";
        }
        else
        {
            if (soilMoisture < crop.MinSoilMoisture)
            {
                message = $"⚠️ URGENT: Irrigate immediately for {hours:F1} hours.";
                priority = "Critical";
                reason = "Critical soil moisture level";
            }
            else if (temperature > 38)
            {
                message = $"Irrigate for {hours:F1} hours due to extreme heat.";
                priority = "High";
                reason = "Heatwave conditions";
            }
            else
            {
                message = $"Irrigate for {hours:F1} hours ({amount:F1} mm).";
                priority = "Medium";
                reason = "Below optimal soil moisture";
            }

            explanation = $"Current soil moisture: {soilMoisture:F0}%. " +
                         $"Target: {crop.OptimalSoilMoistureMax:F0}%. " +
                         $"Water required: {amount:F1} mm. " +
                         $"Temperature: {temperature:F1}°C. " +
                         $"{crop.CropType.GetDisplayName()} at {crop.GrowthStage.GetDisplayName()} stage.";
        }

        return (message, explanation, reason, priority);
    }

    private double CalculateWaterSaving(double recommended, double standard)
    {
        if (standard == 0) return 0;
        var saving = ((standard - recommended) / standard) * 100;
        return Math.Max(0, Math.Round(saving, 1));
    }

    private double CalculateConfidenceScore(WeatherForecast weather, double soilMoisture, CropInfo crop)
    {
        double score = 85.0; // Base confidence

        // Higher confidence if weather data is recent
        var dataAge = DateTime.UtcNow - weather.FetchedAt;
        if (dataAge.TotalHours < 6) score += 10;
        else if (dataAge.TotalHours > 12) score -= 10;

        // Higher confidence if we have good hourly forecast data
        if (weather.HourlyForecasts.Count >= 24) score += 5;

        return Math.Min(100, Math.Max(60, score));
    }

    private string BuildAIPromptForIrrigation(IrrigationRecommendation rec, CropInfo crop, WeatherForecast weather)
    {
        // Ask AI to produce a concise JSON with recommendation, explanation, and confidence
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("You are an expert agricultural assistant. Given the following irrigation recommendation data, return a pure JSON object with keys: recommendation, explanation, confidence (0-100). Do not add other text.");
        sb.AppendLine("Data:");
        sb.AppendLine(System.Text.Json.JsonSerializer.Serialize(new
        {
            crop = crop.CropType.ToString(),
            growthStage = crop.GrowthStage.ToString(),
            soilMoisture = rec.CurrentSoilMoisture,
            recommendedHours = rec.IrrigationDurationHours,
            recommendedAmountMm = rec.IrrigationAmountMm,
            expectedRain = rec.ExpectedRainfall24h,
            temperature = rec.CurrentTemperature,
            priority = rec.Priority,
            reason = rec.Reason
        }));

        return sb.ToString();
    }

    private static string? ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var first = text.IndexOf('{');
        var last = text.LastIndexOf('}');
        if (first >= 0 && last > first)
        {
            return text.Substring(first, last - first + 1);
        }
        return null;
    }
}
