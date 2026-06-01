using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

/// <summary>Service for syncing detection history with remote SQL database</summary>
public interface IHistorySyncService
{
    /// <summary>Sync a single detection record to the server</summary>
    /// <param name="detection">The detection result to sync</param>
    /// <returns>Sync result with remote ID if successful</returns>
    Task<SyncResult> SyncDetectionAsync(DiseaseDetectionResult detection);

    /// <summary>Sync deletion of a record to the server</summary>
    /// <param name="localId">Local SQLite ID</param>
    /// <param name="remoteId">Remote server ID</param>
    Task<SyncResult> SyncDeletionAsync(int localId, string? remoteId);

    /// <summary>Fetch remote changes since last sync (delta pull)</summary>
    /// <param name="sinceLast">Anchor timestamp - only fetch records modified after this</param>
    /// <returns>List of remote records to merge locally</returns>
    Task<List<DiseaseDetectionResult>> FetchRemoteUpdatesAsync(DateTime? sinceLast);

    /// <summary>Check if network is available</summary>
    Task<bool> IsNetworkAvailableAsync();

    /// <summary>Upload image to cloud blob storage</summary>
    /// <param name="localPath">Path to local image file</param>
    /// <returns>Cloud URL of uploaded image</returns>
    Task<string?> UploadImageAsync(string localPath);
}
