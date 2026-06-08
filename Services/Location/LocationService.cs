using KrishiAI.App.Models.Weather;
using System.Diagnostics;
using System.Text.Json;

namespace KrishiAI.App.Services.Location;

/// <summary>
/// Location service implementation using MAUI Geolocation
/// </summary>
public class LocationService : ILocationService
{
    private readonly HttpClient _httpClient;

    public LocationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LocationInfo?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            };

            var location = await Geolocation.GetLocationAsync(request);

            if (location == null)
                return await GetLastKnownLocationAsync();

            var info = new LocationInfo
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                IsCurrentLocation = true,
                LastUpdated = DateTime.UtcNow
            };

            await PopulateReverseGeocodeAsync(info);

            return info;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCurrentLocationAsync Error: {ex.Message}");
            return await GetLastKnownLocationAsync();
        }
    }

    public async Task<LocationInfo?> GetLastKnownLocationAsync()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
                return null;

            var info = new LocationInfo
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                IsCurrentLocation = false,
                LastUpdated = location.Timestamp.UtcDateTime
            };

            await PopulateReverseGeocodeAsync(info);

            return info;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetLastKnownLocationAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> IsLocationEnabledAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"IsLocationEnabledAsync Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RequestLocationPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RequestLocationPermissionAsync Error: {ex.Message}");
            return false;
        }
    }

    private async Task PopulateReverseGeocodeAsync(LocationInfo info)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={info.Latitude:F6}&lon={info.Longitude:F6}&zoom=10";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "KrishiAI-MAUI-App/1.0");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("address", out var address))
            {
                var city = address.TryGetProperty("city", out var c) ? c.GetString() :
                           address.TryGetProperty("town", out var t) ? t.GetString() :
                           address.TryGetProperty("village", out var v) ? v.GetString() :
                           address.TryGetProperty("county", out var co) ? co.GetString() : null;

                var state = address.TryGetProperty("state", out var s) ? s.GetString() : null;
                var country = address.TryGetProperty("country", out var cn) ? cn.GetString() : null;

                info.City = city ?? string.Empty;
                info.State = state ?? string.Empty;
                info.Country = country ?? string.Empty;

                var parts = new List<string>();
                if (!string.IsNullOrEmpty(info.City)) parts.Add(info.City);
                if (!string.IsNullOrEmpty(info.State)) parts.Add(info.State);
                if (!string.IsNullOrEmpty(info.Country)) parts.Add(info.Country);

                info.LocationName = parts.Count > 0 ? string.Join(", ", parts) : $"{info.Latitude:F2}, {info.Longitude:F2}";
                return;
            }

            info.LocationName = $"{info.Latitude:F2}, {info.Longitude:F2}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Reverse geocoding in LocationService failed: {ex.Message}");
            info.LocationName = $"{info.Latitude:F2}, {info.Longitude:F2}";
        }
    }
}
