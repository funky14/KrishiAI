using KrishiAI.App.Models;
using SQLite;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _database;

    public async Task InitializeAsync()
    {
        try
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "krishiai.db3");
            Debug.WriteLine($"💾 Database Path: {dbPath}");
            Debug.WriteLine($"💾 AppDataDirectory: {FileSystem.AppDataDirectory}");

            _database = new SQLiteAsyncConnection(dbPath);

            Debug.WriteLine("📊 Creating User table...");
            await _database.CreateTableAsync<User>();
            Debug.WriteLine("📊 User table created successfully");

            Debug.WriteLine("📊 Creating DiseaseDetectionResult table...");
            await _database.CreateTableAsync<DiseaseDetectionResult>();
            Debug.WriteLine("📊 DiseaseDetectionResult table created successfully");

            Debug.WriteLine($"✅ Database initialized at: {dbPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ InitializeAsync Error: {ex.Message}");
            Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }

    // ===== USER MANAGEMENT =====

    public async Task<int> SaveUserAsync(User user)
    {
        await InitializeAsync();

        if (user.Id == 0)
        {
            return await _database!.InsertAsync(user);
        }
        else
        {
            return await _database!.UpdateAsync(user);
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitializeAsync();
        return await _database!.Table<User>()
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitializeAsync();
        return await _database!.GetAsync<User>(id);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await InitializeAsync();
        return await _database!.Table<User>().ToListAsync();
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        await InitializeAsync();
        var user = await _database!.Table<User>()
            .FirstOrDefaultAsync(x => x.Email == email);
        return user != null;
    }

    // ===== DETECTION HISTORY =====

    public async Task<int> SaveDetectionAsync(DiseaseDetectionResult result)
    {
        await InitializeAsync();

        if (result.Id == 0)
        {
            return await _database!.InsertAsync(result);
        }
        else
        {
            return await _database!.UpdateAsync(result);
        }
    }

    public async Task<List<DiseaseDetectionResult>> GetHistoryAsync()
    {
        await InitializeAsync();
        return await _database!.Table<DiseaseDetectionResult>()
            .OrderByDescending(x => x.DetectedDate)
            .ToListAsync();
    }

    public async Task<int> DeleteDetectionAsync(DiseaseDetectionResult result)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync(result);
    }

    public async Task<int> ClearHistoryAsync()
    {
        await InitializeAsync();
        return await _database!.DeleteAllAsync<DiseaseDetectionResult>();
    }

    // ===== SYNC IMPLEMENTATIONS (Phase 2) =====

    public async Task<List<DiseaseDetectionResult>> GetPendingSyncRecordsAsync()
    {
        await InitializeAsync();
        return await _database!.Table<DiseaseDetectionResult>()
            .Where(x => !x.IsSynced && !x.IsDeleted)
            .OrderBy(x => x.SyncRetryCount)
            .ToListAsync();
    }

    public async Task<List<DiseaseDetectionResult>> GetDeletedRecordsPendingSyncAsync()
    {
        await InitializeAsync();
        return await _database!.Table<DiseaseDetectionResult>()
            .Where(x => x.IsDeleted && !string.IsNullOrEmpty(x.RemoteId))
            .ToListAsync();
    }

    public async Task UpdateSyncStatusAsync(DiseaseDetectionResult result, bool isSynced, string? remoteId, string? error = null)
    {
        await InitializeAsync();
        result.IsSynced = isSynced;
        result.LastSyncTime = isSynced ? DateTime.UtcNow : null;
        result.SyncError = error;
        
        if (isSynced)
        {
            result.SyncRetryCount = 0;
            if (!string.IsNullOrEmpty(remoteId))
                result.RemoteId = remoteId;
        }
        else
        {
            result.SyncRetryCount++;
        }

        await _database!.UpdateAsync(result);
    }

    public async Task<int> MergeRemoteChangesAsync(List<DiseaseDetectionResult> remoteRecords)
    {
        await InitializeAsync();
        int merged = 0;

        foreach (var remoteRecord in remoteRecords)
        {
            if (string.IsNullOrEmpty(remoteRecord.RemoteId))
                continue;

            // Find existing local record by remote ID
            var localRecord = await GetByRemoteIdAsync(remoteRecord.RemoteId);

            if (localRecord == null)
            {
                // New record from server - insert locally
                remoteRecord.Id = 0; // Reset local ID so SQLite auto-generates
                remoteRecord.IsSynced = true;
                await _database!.InsertAsync(remoteRecord);
                merged++;
            }
            else
            {
                // Last-write-wins conflict resolution by UTC timestamp
                if (remoteRecord.LastModifiedDateUtc > localRecord.LastModifiedDateUtc)
                {
                    // Server version is newer - update local record
                    remoteRecord.Id = localRecord.Id; // Keep local ID
                    await _database!.UpdateAsync(remoteRecord);
                    merged++;
                }
            }
        }

        return merged;
    }

    public async Task<DateTime?> GetLastSyncAnchorAsync()
    {
        var anchor = await SecureStorage.GetAsync("LastSyncAnchor");
        if (!string.IsNullOrEmpty(anchor) && DateTime.TryParse(anchor, out var result))
            return result;
        return null;
    }

    public async Task SetLastSyncAnchorAsync(DateTime timestamp)
    {
        await SecureStorage.SetAsync("LastSyncAnchor", timestamp.ToUniversalTime().ToString("O"));
    }

    public async Task UpdateRemoteIdAsync(int localId, string remoteId)
    {
        await InitializeAsync();
        var record = await _database!.GetAsync<DiseaseDetectionResult>(localId);
        if (record != null)
        {
            record.RemoteId = remoteId;
            await _database!.UpdateAsync(record);
        }
    }

    public async Task<DiseaseDetectionResult?> GetByRemoteIdAsync(string remoteId)
    {
        await InitializeAsync();
        return await _database!.Table<DiseaseDetectionResult>()
            .FirstOrDefaultAsync(x => x.RemoteId == remoteId);
    }

    public async Task SoftDeleteAsync(DiseaseDetectionResult result)
    {
        await InitializeAsync();
        result.IsDeleted = true;
        result.LastModifiedDateUtc = DateTime.UtcNow;
        await _database!.UpdateAsync(result);
    }
}
