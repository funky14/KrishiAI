using KrishiAI.App.Models.Weather;

namespace KrishiAI.App.Services.Location;

/// <summary>
/// Location service for GPS and location-based features
/// </summary>
public interface ILocationService
{
    /// <summary>Get current device location</summary>
    Task<LocationInfo?> GetCurrentLocationAsync();

    /// <summary>Get last known location (faster, may be cached)</summary>
    Task<LocationInfo?> GetLastKnownLocationAsync();

    /// <summary>Check if location services are enabled</summary>
    Task<bool> IsLocationEnabledAsync();

    /// <summary>Request location permissions</summary>
    Task<bool> RequestLocationPermissionAsync();
}
