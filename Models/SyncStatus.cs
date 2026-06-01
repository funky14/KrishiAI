namespace KrishiAI.App.Models;

/// <summary>Enumeration of possible sync states for a detection record</summary>
public enum SyncStatus
{
    /// <summary>Record pending initial sync to server</summary>
    PendingCreate,

    /// <summary>Record changes pending sync to server</summary>
    PendingUpdate,

    /// <summary>Record marked for deletion, pending sync</summary>
    PendingDelete,

    /// <summary>Record successfully synced to server</summary>
    Synced,

    /// <summary>Sync failed and will retry</summary>
    SyncFailed,

    /// <summary>Unable to sync (connection issue or permanent error)</summary>
    SyncError
}
