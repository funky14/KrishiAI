namespace KrishiAI.App.Services;

/// <summary>
/// Service for generating and managing unique device identifiers.
/// Each device gets a persistent GUID stored in SecureStorage.
/// </summary>
public class DeviceIdentifierService
{
    private const string DeviceIdKey = "krishi_device_id";
    private const string DeviceNameKey = "krishi_device_name";
    
    /// <summary>
    /// Gets or creates a unique device identifier.
    /// On first call, generates a new GUID and stores it.
    /// Subsequent calls return the same stored GUID.
    /// </summary>
    public async Task<string> GetOrCreateDeviceIdAsync()
    {
        try
        {
            // Check if device ID already exists in secure storage
            var existing = await SecureStorage.Default.GetAsync(DeviceIdKey);
            if (!string.IsNullOrWhiteSpace(existing))
                return existing;

            // Generate new device ID
            var newDeviceId = Guid.NewGuid().ToString();
            await SecureStorage.Default.SetAsync(DeviceIdKey, newDeviceId);
            return newDeviceId;
        }
        catch
        {
            // Fallback if secure storage fails - generate a new GUID
            return Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Gets the stored device ID without creating a new one.
    /// Returns null if not yet initialized.
    /// </summary>
    public async Task<string?> GetDeviceIdAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(DeviceIdKey);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets or sets a human-readable device name.
    /// Defaults to device model if not explicitly set.
    /// </summary>
    public async Task<string> GetOrCreateDeviceNameAsync()
    {
        try
        {
            var existing = await SecureStorage.Default.GetAsync(DeviceNameKey);
            if (!string.IsNullOrWhiteSpace(existing))
                return existing;

            // Use device model as default name
            var defaultName = $"{DeviceInfo.Manufacturer} {DeviceInfo.Model}";
            await SecureStorage.Default.SetAsync(DeviceNameKey, defaultName);
            return defaultName;
        }
        catch
        {
            return $"{DeviceInfo.Manufacturer} {DeviceInfo.Model}";
        }
    }

    /// <summary>
    /// Sets a custom device name.
    /// </summary>
    public async Task SetDeviceNameAsync(string deviceName)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(deviceName))
                await SecureStorage.Default.SetAsync(DeviceNameKey, deviceName);
        }
        catch
        {
            // Silently fail if secure storage is unavailable
        }
    }

    /// <summary>
    /// Gets device information for sync operations.
    /// </summary>
    public async Task<(string DeviceId, string DeviceName)> GetDeviceInfoAsync()
    {
        var deviceId = await GetOrCreateDeviceIdAsync();
        var deviceName = await GetOrCreateDeviceNameAsync();
        return (deviceId, deviceName);
    }

    /// <summary>
    /// Resets the stored device ID (for testing or factory reset).
    /// Next call to GetOrCreateDeviceIdAsync will generate a new ID.
    /// </summary>
    public async Task ResetDeviceIdAsync()
    {
        try
        {
            SecureStorage.Default.Remove(DeviceIdKey);
        }
        catch
        {
            // Silently fail if secure storage is unavailable
        }
        
        // Await to satisfy async contract
        await Task.CompletedTask;
    }
}
