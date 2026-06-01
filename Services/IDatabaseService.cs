using KrishiAI.App.Models;

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
}
