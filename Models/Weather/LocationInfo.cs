namespace KrishiAI.App.Models.Weather;

/// <summary>
/// Location information for weather forecasts
/// </summary>
public class LocationInfo
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsCurrentLocation { get; set; }
}
