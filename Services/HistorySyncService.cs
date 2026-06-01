using KrishiAI.App.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace KrishiAI.App.Services;

/// <summary>Implementation of history sync service that calls remote API</summary>
public class HistorySyncService : IHistorySyncService
{
    private readonly HttpClient _httpClient;
    private readonly IConnectivityService _connectivityService;
    private readonly DeviceIdentifierService _deviceIdentifierService;
    private readonly string _apiBaseUrl;

    public HistorySyncService(HttpClient httpClient, IConnectivityService connectivityService, DeviceIdentifierService deviceIdentifierService)
    {
        _httpClient = httpClient;
        _connectivityService = connectivityService;
        _deviceIdentifierService = deviceIdentifierService;
        
        // Configure API base URL - adjust to your backend
        _apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android 
            ? "http://10.0.2.2:5000"  // Emulator localhost redirect
            : "http://localhost:5000";  // Physical device or Windows
    }

    public async Task<bool> IsNetworkAvailableAsync()
    {
        // Emulator network state can report false negatives; let HTTP calls decide real reachability.
        return await Task.FromResult(true);
    }

    public async Task<SyncResult> SyncDetectionAsync(DiseaseDetectionResult detection)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(detection.DeviceId) || string.IsNullOrWhiteSpace(detection.DeviceName))
            {
                var deviceInfo = await _deviceIdentifierService.GetDeviceInfoAsync();
                detection.DeviceId ??= deviceInfo.DeviceId;
                detection.DeviceName ??= deviceInfo.DeviceName;
            }

            // Upload image if needed
            if (!detection.ImageUploaded && !string.IsNullOrEmpty(detection.ImagePath) && File.Exists(detection.ImagePath))
            {
                detection.CloudImageUrl = await UploadImageAsync(detection.ImagePath);
                detection.ImageUploaded = true;
            }

            // Prepare sync payload (post to API)
            var payload = new
            {
                localId = detection.Id,
                remoteId = detection.RemoteId,
                diseaseName = detection.DiseaseName,
                confidence = detection.Confidence,
                severity = detection.Severity,
                detectedDate = detection.DetectedDate.ToUniversalTime(),
                description = detection.Description,
                affectedCropPart = detection.AffectedCropPart,
                cloudImageUrl = detection.CloudImageUrl,
                lastModifiedDateUtc = detection.LastModifiedDateUtc.ToUniversalTime(),
                version = detection.Version,
                deviceId = detection.DeviceId,
                deviceName = detection.DeviceName
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = detection.RemoteId == null
                ? $"{_apiBaseUrl}/api/detection-history/create"
                : $"{_apiBaseUrl}/api/detection-history/update";

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    string remoteId = doc.RootElement.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                    Debug.WriteLine($"✅ Detection {detection.Id} synced successfully. RemoteId: {remoteId}");
                    return SyncResult.SuccessResult(remoteId, detection.Id);
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"❌ Sync failed for detection {detection.Id}: {response.StatusCode} - {error}");
                return SyncResult.RetryResult(detection.Id, $"HTTP {response.StatusCode}: {error}");
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"🔌 Network error syncing detection {detection.Id}: {ex.Message}");
            return SyncResult.RetryResult(detection.Id, ex.Message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Error syncing detection {detection.Id}: {ex.Message}");
            return SyncResult.RetryResult(detection.Id, ex.Message);
        }
    }

    public async Task<SyncResult> SyncDeletionAsync(int localId, string? remoteId)
    {
        try
        {
            if (string.IsNullOrEmpty(remoteId))
            {
                // Never synced to server, just delete locally
                return SyncResult.SuccessResult(string.Empty, localId);
            }

            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/detection-history/{remoteId}");

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"✅ Deletion synced for remote record {remoteId}");
                return SyncResult.SuccessResult(remoteId, localId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"❌ Delete sync failed for {remoteId}: {response.StatusCode}");
                return SyncResult.RetryResult(localId, $"HTTP {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Error syncing deletion for {remoteId}: {ex.Message}");
            return SyncResult.OfflineResult(localId);
        }
    }

    public async Task<List<DiseaseDetectionResult>> FetchRemoteUpdatesAsync(DateTime? sinceLast)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/detection-history/list";
            if (sinceLast.HasValue)
                url += $"?since={sinceLast:O}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var records = JsonSerializer.Deserialize<List<DiseaseDetectionResult>>(responseBody, options);
                Debug.WriteLine($"📥 Fetched {records?.Count ?? 0} remote updates");
                return records ?? new();
            }
            else
            {
                Debug.WriteLine($"❌ Failed to fetch updates: {response.StatusCode}");
                return new();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Error fetching remote updates: {ex.Message}");
            return new();
        }
    }

    public async Task<string?> UploadImageAsync(string localPath)
    {
        try
        {
            if (!File.Exists(localPath))
            {
                Debug.WriteLine($"⚠️ Image file not found: {localPath}");
                return null;
            }

            using (var content = new MultipartFormDataContent())
            {
                var fileBytes = await File.ReadAllBytesAsync(localPath);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new("image/jpeg");
                
                content.Add(fileContent, "file", Path.GetFileName(localPath));

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/images/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(result))
                    {
                        string? imageUrl = doc.RootElement.GetProperty("url").GetString();
                        Debug.WriteLine($"✅ Image uploaded: {imageUrl}");
                        return imageUrl;
                    }
                }
                else
                {
                    Debug.WriteLine($"❌ Image upload failed: {response.StatusCode}");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Error uploading image: {ex.Message}");
            return null;
        }
    }
}
