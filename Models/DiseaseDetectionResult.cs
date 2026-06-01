using SQLite;

namespace KrishiAI.App.Models;

[Table("DiseaseHistory")]
public class DiseaseDetectionResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    public string DiseaseName { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public string Severity { get; set; } = string.Empty;

    public DateTime DetectedDate { get; set; } = DateTime.Now;

    public string Description { get; set; } = string.Empty;

    [Ignore]
    public List<string> TreatmentRecommendations { get; set; } = new();

    public string AffectedCropPart { get; set; } = string.Empty;

    // ===== SYNC METADATA (Phase 1) =====
    /// <summary>Cloud/Remote record ID - used to link local record to server</summary>
    public string? RemoteId { get; set; }

    /// <summary>Whether this record has been successfully synced to the server</summary>
    public bool IsSynced { get; set; } = false;

    /// <summary>UTC timestamp of last successful sync</summary>
    public DateTime? LastSyncTime { get; set; }

    /// <summary>Soft delete flag - record marked for deletion on server</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Number of sync retry attempts</summary>
    public int SyncRetryCount { get; set; } = 0;

    /// <summary>Last sync error message (if any)</summary>
    public string? SyncError { get; set; }

    /// <summary>URL of image in cloud blob storage (if uploaded)</summary>
    public string? CloudImageUrl { get; set; }

    /// <summary>Whether image has been uploaded to cloud storage</summary>
    public bool ImageUploaded { get; set; } = false;

    /// <summary>UTC creation timestamp (for conflict resolution)</summary>
    public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

    /// <summary>UTC modification timestamp (for last-write-wins conflict resolution)</summary>
    public DateTime LastModifiedDateUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Version number for optimistic locking</summary>
    public int Version { get; set; } = 1;

    /// <summary>Unique identifier for the device that created this record</summary>
    public string? DeviceId { get; set; }

    /// <summary>Human-readable name of the device (e.g., "John's Phone")</summary>
    public string? DeviceName { get; set; }
}
