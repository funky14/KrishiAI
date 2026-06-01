# Offline/Online Sync Implementation - Detection History

## Overview

Complete two-way offline/online sync infrastructure for Detection History records with:
- **Local-first**: SQLite database for immediate saves
- **Last-write-wins**: Conflict resolution via `LastModifiedDateUtc` (UTC)
- **Soft-delete**: `IsDeleted` flag for cloud sync instead of hard-delete
- **Sync triggers**: Save, app start, connectivity restored
- **Retry strategy**: Exponential backoff with `SyncRetryCount`
- **Image handling**: Lazy upload to blob storage on sync

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    User Interaction Layer                       │
│  CropDiseaseViewModel (analyze) │ HistoryViewModel (view/delete)│
└─────────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────────┐
│                   Sync Orchestration Layer                      │
│              SyncQueueManager                                   │
│  • Push pending creates/updates → IHistorySyncService          │
│  • Push soft-deletes         → IHistorySyncService             │
│  • Pull remote updates       → IHistorySyncService             │
│  • Merge with last-write-wins → IDatabaseService               │
└─────────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Data Access Layer                            │
│              DatabaseService (IDatabaseService)                │
│  • SaveDetectionAsync - Local SQLite save                      │
│  • GetPendingSyncRecordsAsync - WHERE !IsSynced               │
│  • UpdateSyncStatusAsync - Post-sync status update             │
│  • MergeRemoteChangesAsync - Last-write-wins logic             │
│  • SoftDeleteAsync - Set IsDeleted=true                        │
└─────────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────────┐
│              Sync Service Layer (IHistorySyncService)          │
│           HistorySyncService (HTTP API integration)            │
│  • SyncDetectionAsync → POST /api/detection-history/{create|update}
│  • SyncDeletionAsync → DELETE /api/detection-history/{remoteId}
│  • FetchRemoteUpdatesAsync → GET /api/detection-history/list?since=
│  • UploadImageAsync → POST /api/images/upload                 │
├─────────────────────────────────────────────────────────────────┤
│              Remote API / Azure Backend                        │
│  Database: azuredemodb.database.windows.net                   │
│  Storage: Blob storage for images                             │
└─────────────────────────────────────────────────────────────────┘
```

## Data Model Extensions (DiseaseDetectionResult)

### New Sync Fields

```csharp
// Remote sync state
public string? RemoteId { get; set; }           // Server GUID for this record
public bool IsSynced { get; set; } = false;     // Last sync succeeded
public DateTime? LastSyncTime { get; set; }     // UTC timestamp of last sync

// Local state for retry/recovery
public bool IsDeleted { get; set; } = false;    // Soft-delete flag for cloud
public int SyncRetryCount { get; set; } = 0;    // Retry attempt counter
public string? SyncError { get; set; }          // Latest sync error message

// Image and conflict resolution
public string? CloudImageUrl { get; set; }      // Blob storage URL (null=local only)
public bool ImageUploaded { get; set; } = false;// Image sent to cloud
public DateTime CreatedDateUtc { get; set; }    // Immutable creation time (UTC)
public DateTime LastModifiedDateUtc { get; set; } // Updated on every change (UTC)
public int Version { get; set; } = 1;           // Version number for optimistic locking
```

### Sync Metadata Semantics

| Field | Purpose | When Updated | Notes |
|-------|---------|--------------|-------|
| `RemoteId` | Server ID mapping | First successful sync | Maps local int ID ↔ server GUID |
| `IsSynced` | Sync status | After sync attempt | true=success, false=pending/failed |
| `LastSyncTime` | Last success | After successful sync | null if never synced or failed |
| `IsDeleted` | Soft delete | DeleteItem clicked | Set to true, stays pending until synced |
| `SyncRetryCount` | Backoff counter | After failed sync | Incremented on failure, reset on success |
| `SyncError` | Error details | After failed sync | Latest error message for debugging |
| `CloudImageUrl` | Image location | UploadImageAsync | URL in blob storage, null if local only |
| `ImageUploaded` | Image sent | After image upload | Sync will not retry upload if true |
| `CreatedDateUtc` | Immutable birth | SaveDetectionAsync | Never changes after creation |
| `LastModifiedDateUtc` | Conflict resolver | Every change | UTC timestamp, used for last-write-wins |
| `Version` | Optimistic lock | With LastModifiedDateUtc | For advanced conflict handling |

## Sync Flow

### 1. Local Save (Immediate)
```
CropDiseaseViewModel.AnalyzeImage()
  ↓
DatabaseService.SaveDetectionAsync(detection)
  ↓
SQLite insert/update (IsSynced=false, SyncRetryCount=0)
  ↓
Return to user (instant, even offline)
```

### 2. Attempt Sync (Async, non-blocking)
```
CropDiseaseViewModel.AnalyzeImage() calls SyncQueueManager.ProcessQueueAsync()
  ↓
SyncQueueManager.PushPendingChangesAsync()
  ↓
For each record with !IsSynced AND SyncRetryCount < 5:
  ├─ Check connectivity
  ├─ Upload image if !ImageUploaded
  └─ POST to /api/detection-history/{create|update}
       ├─ Success: DatabaseService.UpdateSyncStatusAsync(true, remoteId)
       │           (IsSynced=true, LastSyncTime=UtcNow, SyncRetryCount=0)
       │
       ├─ Retryable error (4xx, network): 
       │  DatabaseService.UpdateSyncStatusAsync(false, null, error)
       │  (IsSynced=false, SyncError=error, SyncRetryCount++)
       │
       └─ Permanent error: Don't retry
```

### 3. Pull Remote Updates (After push)
```
SyncQueueManager.PullRemoteUpdatesAsync()
  ↓
lastAnchor = await DatabaseService.GetLastSyncAnchorAsync()
  ↓
GET /api/detection-history/list?since={lastAnchor:O}
  ↓
DatabaseService.MergeRemoteChangesAsync(remoteRecords)
  ├─ For each remote record:
  │  ├─ Look up local by RemoteId
  │  ├─ If not found: INSERT (local ID auto-generated)
  │  └─ If found: Compare LastModifiedDateUtc
  │     ├─ Remote newer → Update local record (last-write-wins)
  │     └─ Local newer → Keep local, next push will update server
  └─ Return merge count
```

### 4. Update Sync Anchor (After merge)
```
DatabaseService.SetLastSyncAnchorAsync(DateTime.UtcNow)
  ↓
SecureStorage.SetAsync("LastSyncAnchor", timestamp.ToString("O"))
  ↓
Enables delta queries on next sync (only fetch records since this time)
```

## Sync Triggers

### Trigger 1: Save with Online Check
**When**: User analyzes image and result is saved
**Where**: `CropDiseaseViewModel.AnalyzeImage()`
```csharp
await _databaseService.SaveDetectionAsync(result); // Local-first
_ = _syncQueueManager.ProcessQueueAsync();         // Async push attempt
```
**Behavior**: 
- Local save completes immediately (even offline)
- Sync attempt in background, no wait
- If online: push pending records, pull updates, merge
- If offline: queued for next trigger

### Trigger 2: App Start
**When**: HistoryPage appears (OnAppearing)
**Where**: `HistoryViewModel.OnAppearing()`
```csharp
await _syncQueueManager.ProcessQueueAsync();  // Process queued items
await LoadHistory();                          // Refresh display
```
**Behavior**:
- Checks for unsynced records from previous sessions
- Pushes any pending changes
- Pulls remote updates since last successful sync
- Updates display with current state

### Trigger 3: Connectivity Restored
**When**: Device reconnects to network
**Where**: `HistoryViewModel.OnConnectivityChanged(object, bool isConnected)`
```csharp
if (isConnected)
    await _syncQueueManager.ProcessQueueAsync();
```
**Behavior**:
- Automatically triggered by `IConnectivityService.ConnectivityChanged` event
- Processes queue immediately on reconnect
- No user action required
- Seamless background sync

## Services

### IHistorySyncService

Interface for remote API integration:

```csharp
public interface IHistorySyncService
{
    Task<SyncResult> SyncDetectionAsync(DiseaseDetectionResult detection);
    Task<SyncResult> SyncDeletionAsync(int localId, string? remoteId);
    Task<List<DiseaseDetectionResult>> FetchRemoteUpdatesAsync(DateTime? sinceLast);
    Task<string?> UploadImageAsync(string localPath);
    Task<bool> IsNetworkAvailableAsync();
}
```

**Endpoints Called**:
- `POST /api/detection-history/create` - New record
- `POST /api/detection-history/update` - Update existing
- `DELETE /api/detection-history/{remoteId}` - Soft delete
- `GET /api/detection-history/list?since={timestamp}` - Delta pull
- `POST /api/images/upload` - Image upload

### SyncQueueManager

Orchestrates complete sync cycle:

```csharp
public class SyncQueueManager
{
    public async Task ProcessQueueAsync()
    {
        // Phase 1: Push pending creates/updates/deletes
        // Phase 2: Pull remote updates
        // Phase 3: Update sync anchor
    }
}
```

**Features**:
- Prevents concurrent syncs (`_isSyncing` flag)
- Exponential backoff (skips records with `SyncRetryCount > 5`)
- Logs each phase with debug output
- Publishes `SyncProgress` events for UI updates
- Graceful handling of network disconnections

## Error Handling

### Error Categories

```
Network Error (Retryable)
├─ HttpRequestException → SyncResult.OfflineResult
├─ HTTP 500 → SyncResult.RetryResult
└─ Connection timeout → SyncResult.OfflineResult

Client Error (Permanent)
├─ HTTP 400 (Bad Request) → SyncResult.RetryResult (still retry)
└─ HTTP 401/403 (Auth) → SyncResult.FailureResult

Retry Strategy
├─ Max Retries: 5 attempts
├─ Backoff: Next sync cycle
├─ Reset Counter: On successful sync
└─ Behavior: Records skipped when SyncRetryCount >= 5
```

### Error Recovery

1. **Offline**: Record stays pending, no error increment
2. **Temporary Failure**: SyncRetryCount++, wait for next sync
3. **Max Retries**: Stop retrying, log error, manual intervention needed
4. **Network Restored**: Automatic retry via ConnectivityChanged trigger

## Conflict Resolution

### Last-Write-Wins Strategy

When merging remote updates, use `LastModifiedDateUtc` (UTC timestamps):

```csharp
// In DatabaseService.MergeRemoteChangesAsync()
if (remoteRecord.LastModifiedDateUtc > localRecord.LastModifiedDateUtc)
{
    // Remote is newer: use remote version
    await _database!.UpdateAsync(remoteRecord);
}
else
{
    // Local is newer: keep local, next push will update server
}
```

### Requirements for Correct Conflict Resolution

1. **All timestamps must be UTC**: Use `DateTime.UtcNow`
2. **Update on every change**: Modify `LastModifiedDateUtc` in every update
3. **Never edit CreatedDateUtc**: Immutable field for identity
4. **Server must accept UTC timestamps**: API validation

### Example Conflict Scenario

```
Timeline:
T1: User edits record on mobile (LastModifiedDateUtc = T1)
T2: User edits same record on server (LastModifiedDateUtc = T2 > T1)
T3: Mobile attempts sync (fetches remote updates)

Resolution (T3):
- Local: LastModifiedDateUtc = T1
- Remote: LastModifiedDateUtc = T2
- T2 > T1 → Use remote version (server wins)
- Local record updated with server values
```

## Image Handling

### Upload Strategy

Images are uploaded lazily during sync:

```csharp
// In HistorySyncService.SyncDetectionAsync()
if (!detection.ImageUploaded && !string.IsNullOrEmpty(detection.ImagePath))
{
    detection.CloudImageUrl = await UploadImageAsync(detection.ImagePath);
    detection.ImageUploaded = true;  // Mark to prevent re-upload
}
```

### Fields Used

- `ImagePath`: Local file path (e.g., `/data/data/.../image.jpg`)
- `CloudImageUrl`: Blob storage URL (e.g., `https://storage.blob.core.windows.net/...`)
- `ImageUploaded`: Flag to skip re-upload attempts

### Failure Handling

If image upload fails during sync:
- `CloudImageUrl` remains null
- `ImageUploaded` stays false
- Next sync attempt will retry upload
- Sync continues with null URL if upload fails (graceful degrade)

## Database State

### Sync Status Indicators

| Condition | Status | Display |
|-----------|--------|---------|
| `IsSynced=true` AND `LastSyncTime!=null` | ✅ Synced | Green badge "✓ Synced" |
| `IsSynced=false` AND `SyncRetryCount < 5` | ⏳ Pending | Orange badge "⏳ Pending" |
| `IsSynced=false` AND `SyncRetryCount >= 5` | ❌ Failed | Red text (needs manual fix) |
| `IsDeleted=true` | 🗑️ Deleted | Excluded from display (soft-delete) |

### Queries

**Pending records** (WHERE clause):
```sql
WHERE !IsSynced AND !IsDeleted
ORDER BY SyncRetryCount ASC
```

**Deleted records pending sync**:
```sql
WHERE IsDeleted AND RemoteId IS NOT NULL
```

**Fetch for display** (exclude soft-deleted):
```sql
WHERE !IsDeleted
```

**Delta pull since anchor**:
```sql
WHERE LastModifiedDateUtc >= @anchor
```

## Configuration

### API Base URL

Configured in `HistorySyncService` constructor:

```csharp
_apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android 
    ? "http://10.0.2.2:5000"  // Android emulator localhost redirect
    : "http://localhost:5000";  // Physical device or Windows
```

**Environment-Specific**:
- Emulator: `10.0.2.2:5000` (special routing)
- Android device: Update to actual server IP
- Windows device: `localhost:5000` or service URL

### HTTP Client Configuration

Registered in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<HttpClient>();
```

### Runtime Data Store Registration

Current app runtime is configured for offline-first SQLite:

```csharp
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
```

Sync to cloud happens through `SyncQueueManager` + `HistorySyncService` when network is available.

### Optional Direct Azure SQL Service (Security)

`AzureSqlDatabaseService` is optional and should only be used when explicitly needed.
Credentials are read from environment variables instead of hardcoded values:

- `KRISHI_SQL_SERVER`
- `KRISHI_SQL_DATABASE`
- `KRISHI_SQL_USER`
- `KRISHI_SQL_PASSWORD`

### SecureStorage Keys

| Key | Purpose |
|-----|---------|
| `LastSyncAnchor` | Timestamp of last successful sync (ISO 8601 format) |

## View Integration

### HistoryPage Sync Indicators

1. **Sync Status Header** (Phase 4)
   - Shows "🔄 Syncing detection history..." when `IsSyncing=true`
   - Visible only during active sync

2. **Per-Item Status Badge** (Phase 4)
   - ✅ Synced (green) - `IsSynced=true`
   - ⏳ Pending (orange) - `!IsSynced AND SyncRetryCount < 5`
   - Deleted items excluded from view

3. **Manual Refresh** (Phase 4)
   - "🔄 Refresh" button triggers `LoadHistoryCommand`
   - Loads latest from local SQLite

## Testing Checklist

### Unit Tests (Local)
- [ ] LocalSaveOffline: Create record offline → verify in SQLite
- [ ] LocalDeleteOffline: Delete record offline → verify soft-deleted
- [ ] PendingQuery: GetPendingSyncRecordsAsync returns unsynced only
- [ ] SyncAnchor: SecureStorage persists and retrieves datetime

### Integration Tests (With API)
- [ ] SyncOnline: Save → sync → verify RemoteId set, IsSynced=true
- [ ] SyncImage: Save with image → sync → verify CloudImageUrl set
- [ ] MergeConflict: Edit local + remote → fetch → last-write-wins
- [ ] SyncDelete: Soft-delete → sync → verify remote deletion
- [ ] Retry: API fails → SyncRetryCount++ → Next cycle succeeds
- [ ] DeltaPull: Insert on server → sync → local merged, no duplicates

### End-to-End Tests
- [ ] **Offline Create**: Turn off network → Create 2 detections → Turn on network → Auto-sync
- [ ] **Reconnect Sync**: Kill app → Delete records server-side → Restart app → Deltas fetched
- [ ] **Conflict Last-Write-Wins**: Edit on mobile (T1) and server (T2>T1) → Sync → Server wins
- [ ] **Image Lazy Upload**: Save with local image → Sync → Image uploaded, URL stored
- [ ] **Max Retry**: Force API to return 500 → 5 sync attempts → Stop retrying
- [ ] **Recovery**: API down → Retry after 5 syncs → Finally succeeds

## Implementation Timeline (Phases)

| Phase | Task | Status |
|-------|------|--------|
| 1 | Extend data model with sync fields | ✅ Complete |
| 2 | Create sync service interfaces & DTOs | ✅ Complete |
| 3 | Implement database sync methods | ✅ Complete |
| 4 | Implement HistorySyncService (API calls) | ✅ Complete |
| 5 | Create SyncQueueManager (orchestration) | ✅ Complete |
| 6 | Register services in MauiProgram | ✅ Complete |
| 7 | Integrate CropDiseaseViewModel (save trigger) | ✅ Complete |
| 8 | Integrate HistoryViewModel (app start + connectivity) | ✅ Complete |
| 9 | Update HistoryPage UI (sync indicators) | ✅ Complete |
| 10 | Implement backend API endpoints | ⏳ Pending |
| 11 | End-to-end testing | ⏳ Pending |

## Pending Work

### Backend API (Vault.API or similar)

Must implement these endpoints:

```http
POST /api/detection-history/create
{
  "localId": 0,
  "remoteId": null,
  "diseaseName": "Powdery Mildew",
  "confidence": 0.95,
  "severity": "High",
  "detectedDate": "2025-01-20T10:30:00Z",
  "description": "...",
  "affectedCropPart": "Leaves",
  "cloudImageUrl": "https://...",
  "lastModifiedDateUtc": "2025-01-20T10:30:00Z",
  "version": 1
}
Response: { "id": "550e8400-e29b-41d4-a716-446655440000" }

POST /api/detection-history/update
(Same payload with remoteId set)

DELETE /api/detection-history/{remoteId}
Response: 204 No Content

GET /api/detection-history/list?since=2025-01-20T10:00:00Z
Response: [{ ...DiseaseDetectionResult... }]

POST /api/images/upload
Content-Type: multipart/form-data
Body: file (binary image)
Response: { "url": "https://storage.blob.core.windows.net/..." }
```

### Azure Database

Use provided connection:
- Server: `azuredemodb.database.windows.net`
- Database: `free-sql-db-4227077`
- Create table for `DetectionHistory` with fields matching `DiseaseDetectionResult`

## Files Modified

### Models
- `DiseaseDetectionResult.cs` - Added 11 sync fields

### Services (Interfaces)
- `IHistorySyncService.cs` - 5 sync methods (new)

### Services (Implementations)
- `HistorySyncService.cs` - Full API integration (new)
- `SyncQueueManager.cs` - Orchestration (new)
- `DatabaseService.cs` - Extended with 9 sync methods

### ViewModels
- `CropDiseaseViewModel.cs` - Added sync trigger after save
- `HistoryViewModel.cs` - Added sync on appear and connectivity changed

### Views
- `HistoryPage.xaml` - Added sync status indicators

### Configuration
- `MauiProgram.cs` - Registered sync services

## Debug Output Format

All sync operations logged to Debug console:

```
✅ Success: "✅ Detection 1 synced successfully. RemoteId: guid"
❌ Failure: "❌ Sync failed for detection 1: HTTP 500 - error"
🔌 Network: "🔌 Network error syncing detection 1: Connection timeout"
⚠️ Exception: "⚠️ Error syncing detection 1: NullReferenceException"
📤 Push: "📤 Found 3 records pending sync"
📥 Pull: "📥 Fetched 2 remote updates"
🔄 Cycle: "🔄 Starting sync cycle..." / "✅ Sync cycle completed successfully"
⏳ Skip: "⏳ Skipping record 1 (too many retries)"
🗑️ Delete: "🗑️ Found 1 records pending deletion sync"
```

Use `Debug.WriteLine()` in VS Debug output or check logcat on Android.

## Quick Reference

### Sync Lifecycle for Single Record

```
1. CREATE: User captures image → AnalyzeImage()
2. LOCAL SAVE: SaveDetectionAsync() → SQLite {IsSynced: false}
3. ASYNC SYNC: SyncQueueManager.ProcessQueueAsync()
   a. IsOnline? 
   b. Upload image → CloudImageUrl
   c. POST to API → RemoteId
   d. UpdateSyncStatusAsync() → {IsSynced: true, LastSyncTime: now}
4. PULL: FetchRemoteUpdatesAsync() → Any deltas
5. MERGE: MergeRemoteChangesAsync() → Last-write-wins
6. ANCHOR: SetLastSyncAnchorAsync() → Enables next delta query
7. DISPLAY: HistoryViewModel.LoadHistory() → Show with ✅ badge
```

### How to Debug Sync Issues

1. **Check pending records**: Query SQLite where `IsSynced = 0`
2. **Check sync errors**: Look at `SyncError` field
3. **Check retry count**: If `SyncRetryCount >= 5`, sync stopped
4. **Check RemoteId**: null = never synced, guid = synced
5. **Check connectivity**: `IConnectivityService.IsConnected()`
6. **Check logs**: Watch Debug output for "❌" or "⚠️"
7. **Manual trigger**: Call `await _syncQueueManager.ProcessQueueAsync()` in app startup
