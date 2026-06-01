using KrishiAI.App.Models;
using System.Diagnostics;

namespace KrishiAI.App.Services;

/// <summary>Orchestrates the complete sync flow: push local changes, pull remote updates, merge, and persist state</summary>
public class SyncQueueManager
{
    private readonly IDatabaseService _databaseService;
    private readonly IHistorySyncService _syncService;
    private readonly IConnectivityService _connectivityService;
    private bool _isSyncing = false;

    public event EventHandler<SyncProgressEventArgs>? SyncProgress;

    public SyncQueueManager(IDatabaseService databaseService, IHistorySyncService syncService, IConnectivityService connectivityService)
    {
        _databaseService = databaseService;
        _syncService = syncService;
        _connectivityService = connectivityService;
    }

    /// <summary>Execute full sync cycle: push pending changes, pull remote updates, merge</summary>
    public async Task ProcessQueueAsync()
    {
        if (_isSyncing)
        {
            Debug.WriteLine("⏳ Sync already in progress, skipping");
            return;
        }

        _isSyncing = true;
        try
        {
            if (!await _syncService.IsNetworkAvailableAsync())
            {
                Debug.WriteLine("🔌 No network connection, deferring sync");
                OnSyncProgress("No network connection");
                return;
            }

            Debug.WriteLine("🔄 Starting sync cycle...");
            OnSyncProgress("Syncing...");

            // Phase 1: Push pending local changes to server
            await PushPendingChangesAsync();

            // Phase 2: Pull remote updates since last sync
            await PullRemoteUpdatesAsync();

            // Phase 3: Update sync anchor
            await _databaseService.SetLastSyncAnchorAsync(DateTime.UtcNow);

            Debug.WriteLine("✅ Sync cycle completed successfully");
            OnSyncProgress("Sync complete");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Sync cycle failed: {ex.Message}");
            OnSyncProgress($"Sync failed: {ex.Message}");
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>Push all pending local changes to server</summary>
    private async Task PushPendingChangesAsync()
    {
        Debug.WriteLine("📤 Phase 1: Pushing local changes...");

        // Sync create/update records
        var pendingRecords = await _databaseService.GetPendingSyncRecordsAsync();
        Debug.WriteLine($"📤 Found {pendingRecords.Count} records pending sync");

        foreach (var record in pendingRecords)
        {
            var result = await _syncService.SyncDetectionAsync(record);

            if (result.Success)
            {
                // Mark as synced and store remote ID
                await _databaseService.UpdateSyncStatusAsync(record, true, result.RemoteId);
                Debug.WriteLine($"✅ Record {record.Id} synced with remote ID {result.RemoteId}");
            }
            else if (result.ShouldRetry)
            {
                // Keep record pending, will retry next cycle
                await _databaseService.UpdateSyncStatusAsync(record, false, null, result.ErrorMessage);
                Debug.WriteLine($"⏳ Record {record.Id} will retry: {result.ErrorMessage}");
            }
            else
            {
                // Permanent error - don't retry
                await _databaseService.UpdateSyncStatusAsync(record, false, null, result.ErrorMessage);
                Debug.WriteLine($"❌ Record {record.Id} permanent error: {result.ErrorMessage}");
            }
        }

        // Sync soft-deleted records
        var deletedRecords = await _databaseService.GetDeletedRecordsPendingSyncAsync();
        Debug.WriteLine($"🗑️ Found {deletedRecords.Count} records pending deletion sync");

        foreach (var record in deletedRecords)
        {
            var result = await _syncService.SyncDeletionAsync(record.Id, record.RemoteId);
            if (result.Success)
            {
                // Hard delete from local DB (record was soft-deleted)
                await _databaseService.DeleteDetectionAsync(record);
                Debug.WriteLine($"✅ Record {record.Id} deletion synced");
            }
        }
    }

    /// <summary>Pull remote updates and merge into local store</summary>
    private async Task PullRemoteUpdatesAsync()
    {
        Debug.WriteLine("📥 Phase 2: Pulling remote updates...");

        var lastAnchor = await _databaseService.GetLastSyncAnchorAsync();
        var remoteUpdates = await _syncService.FetchRemoteUpdatesAsync(lastAnchor);

        if (remoteUpdates.Any())
        {
            var mergedCount = await _databaseService.MergeRemoteChangesAsync(remoteUpdates);
            Debug.WriteLine($"📥 Merged {mergedCount} remote changes");
        }
        else
        {
            Debug.WriteLine("📥 No remote updates");
        }
    }

    protected void OnSyncProgress(string message)
    {
        SyncProgress?.Invoke(this, new SyncProgressEventArgs { Message = message });
    }
}

public class SyncProgressEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
}
