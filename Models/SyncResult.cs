namespace KrishiAI.App.Models;

/// <summary>Result of a sync operation</summary>
public class SyncResult
{
    public bool Success { get; set; }
    public string? RemoteId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShouldRetry { get; set; }
    public int LocalId { get; set; }

    public static SyncResult SuccessResult(string remoteId, int localId)
        => new() { Success = true, RemoteId = remoteId, LocalId = localId };

    public static SyncResult OfflineResult(int localId)
        => new() { Success = false, ErrorMessage = "No network connection", ShouldRetry = true, LocalId = localId };

    public static SyncResult RetryResult(int localId, string error)
        => new() { Success = false, ErrorMessage = error, ShouldRetry = true, LocalId = localId };

    public static SyncResult FailureResult(int localId, string error)
        => new() { Success = false, ErrorMessage = error, ShouldRetry = false, LocalId = localId };
}
