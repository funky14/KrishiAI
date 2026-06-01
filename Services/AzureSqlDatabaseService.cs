using KrishiAI.App.Models;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace KrishiAI.App.Services;

/// <summary>Azure SQL database service for Detection History sync</summary>
public class AzureSqlDatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public AzureSqlDatabaseService()
    {
        // Preferred source: full connection string from environment variable.
        var fullConnectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__KrishiSql") ??
            Environment.GetEnvironmentVariable("KRISHI_SQL_CONNECTION_STRING");

        if (!string.IsNullOrWhiteSpace(fullConnectionString))
        {
            _connectionString = fullConnectionString;
            Debug.WriteLine("Azure SQL Service initialized");
            return;
        }

        // Backward-compatible fallback for split environment variables.
        var server = Environment.GetEnvironmentVariable("KRISHI_SQL_SERVER");
        var database = Environment.GetEnvironmentVariable("KRISHI_SQL_DATABASE");
        var userId = Environment.GetEnvironmentVariable("KRISHI_SQL_USER");
        var password = Environment.GetEnvironmentVariable("KRISHI_SQL_PASSWORD");

        if (string.IsNullOrWhiteSpace(server) ||
            string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(password))
        {
            _connectionString = string.Empty;
            Debug.WriteLine("Azure SQL Service not configured: set ConnectionStrings__KrishiSql or KRISHI_SQL_* environment variables.");
            return;
        }

        _connectionString =
            $"Server={server};" +
            $"Database={database};" +
            $"User Id={userId};" +
            $"Password={password};" +
            "Encrypt=true;" +
            "Connection Timeout=30;";

        Debug.WriteLine("Azure SQL Service initialized");
    }

    public async Task InitializeAsync()
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                // Create DiseaseHistory table if not exists
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                                   WHERE TABLE_NAME = 'DiseaseHistory')
                    CREATE TABLE DiseaseHistory (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        ImagePath NVARCHAR(MAX),
                        DiseaseName NVARCHAR(255),
                        Confidence FLOAT,
                        Severity NVARCHAR(50),
                        DetectedDate DATETIME2,
                        Description NVARCHAR(MAX),
                        AffectedCropPart NVARCHAR(255),
                        RemoteId UNIQUEIDENTIFIER NULL,
                        IsSynced BIT DEFAULT 0,
                        LastSyncTime DATETIME2 NULL,
                        IsDeleted BIT DEFAULT 0,
                        SyncRetryCount INT DEFAULT 0,
                        SyncError NVARCHAR(MAX),
                        CloudImageUrl NVARCHAR(MAX),
                        ImageUploaded BIT DEFAULT 0,
                        CreatedDateUtc DATETIME2,
                        LastModifiedDateUtc DATETIME2,
                        Version INT DEFAULT 1,
                        DeviceId NVARCHAR(128) NULL,
                        DeviceName NVARCHAR(256) NULL
                    );
                    
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.INDEXES 
                                   WHERE TABLE_NAME='DiseaseHistory' AND INDEX_NAME='IX_IsSynced')
                    CREATE INDEX IX_IsSynced ON DiseaseHistory(IsSynced, IsDeleted);
                    
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.INDEXES 
                                   WHERE TABLE_NAME='DiseaseHistory' AND INDEX_NAME='IX_RemoteId')
                    CREATE INDEX IX_RemoteId ON DiseaseHistory(RemoteId);

                    IF COL_LENGTH('dbo.DiseaseHistory', 'DeviceId') IS NULL
                    ALTER TABLE dbo.DiseaseHistory ADD DeviceId NVARCHAR(128) NULL;

                    IF COL_LENGTH('dbo.DiseaseHistory', 'DeviceName') IS NULL
                    ALTER TABLE dbo.DiseaseHistory ADD DeviceName NVARCHAR(256) NULL;
                ";
                
                await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine("✅ DiseaseHistory table initialized");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ InitializeAsync Error: {ex.Message}");
            throw;
        }
    }

    public async Task<int> SaveDetectionAsync(DiseaseDetectionResult result)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                if (result.Id == 0)
                {
                    // Insert new
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO DiseaseHistory (
                            ImagePath, DiseaseName, Confidence, Severity, DetectedDate,
                            Description, AffectedCropPart, CreatedDateUtc, LastModifiedDateUtc,
                            DeviceId, DeviceName
                        ) VALUES (
                            @ImagePath, @DiseaseName, @Confidence, @Severity, @DetectedDate,
                            @Description, @AffectedCropPart, @CreatedDateUtc, @LastModifiedDateUtc,
                            @DeviceId, @DeviceName
                        );
                        SELECT SCOPE_IDENTITY();
                    ";
                    
                    AddParameters(cmd, result);
                    var id = (int)(decimal)await cmd.ExecuteScalarAsync();
                    result.Id = id;
                    Debug.WriteLine($"✅ Record {id} saved to Azure SQL");
                    return id;
                }
                else
                {
                    // Update existing
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        UPDATE DiseaseHistory SET
                            ImagePath = @ImagePath,
                            DiseaseName = @DiseaseName,
                            Confidence = @Confidence,
                            Severity = @Severity,
                            DetectedDate = @DetectedDate,
                            Description = @Description,
                            AffectedCropPart = @AffectedCropPart,
                            LastModifiedDateUtc = @LastModifiedDateUtc,
                            DeviceId = @DeviceId,
                            DeviceName = @DeviceName,
                            Version = Version + 1
                        WHERE Id = @Id
                    ";
                    
                    AddParameters(cmd, result);
                    cmd.Parameters.AddWithValue("@Id", result.Id);
                    
                    await cmd.ExecuteNonQueryAsync();
                    Debug.WriteLine($"✅ Record {result.Id} updated in Azure SQL");
                    return result.Id;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ SaveDetectionAsync Error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<DiseaseDetectionResult>> GetHistoryAsync()
    {
        var results = new List<DiseaseDetectionResult>();
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT * FROM DiseaseHistory 
                    WHERE IsDeleted = 0
                    ORDER BY DetectedDate DESC
                ";
                
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(MapFromReader(reader));
                }
                reader.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ GetHistoryAsync Error: {ex.Message}");
        }
        return results;
    }

    public async Task<int> DeleteDetectionAsync(DiseaseDetectionResult result)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM DiseaseHistory WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", result.Id);
                
                await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"✅ Record {result.Id} hard-deleted from Azure SQL");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ DeleteDetectionAsync Error: {ex.Message}");
            throw;
        }
    }

    public async Task<int> ClearHistoryAsync()
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM DiseaseHistory";
                
                var count = await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"✅ Cleared {count} records from Azure SQL");
                return count;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ClearHistoryAsync Error: {ex.Message}");
            throw;
        }
    }

    // ===== SYNC METHODS =====

    public async Task<List<DiseaseDetectionResult>> GetPendingSyncRecordsAsync()
    {
        var results = new List<DiseaseDetectionResult>();
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT * FROM DiseaseHistory 
                    WHERE IsSynced = 0 AND IsDeleted = 0
                    ORDER BY SyncRetryCount ASC, CreatedDateUtc ASC
                ";
                
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(MapFromReader(reader));
                }
                reader.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ GetPendingSyncRecordsAsync Error: {ex.Message}");
        }
        return results;
    }

    public async Task<List<DiseaseDetectionResult>> GetDeletedRecordsPendingSyncAsync()
    {
        var results = new List<DiseaseDetectionResult>();
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT * FROM DiseaseHistory 
                    WHERE IsDeleted = 1 AND RemoteId IS NOT NULL
                    ORDER BY LastModifiedDateUtc ASC
                ";
                
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(MapFromReader(reader));
                }
                reader.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ GetDeletedRecordsPendingSyncAsync Error: {ex.Message}");
        }
        return results;
    }

    public async Task UpdateSyncStatusAsync(DiseaseDetectionResult record, bool isSynced, string? remoteId, string? error = null)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                
                if (isSynced)
                {
                    cmd.CommandText = @"
                        UPDATE DiseaseHistory SET
                            IsSynced = 1,
                            LastSyncTime = GETUTCDATE(),
                            RemoteId = @RemoteId,
                            SyncRetryCount = 0,
                            SyncError = NULL
                        WHERE Id = @Id
                    ";
                }
                else
                {
                    cmd.CommandText = @"
                        UPDATE DiseaseHistory SET
                            IsSynced = 0,
                            LastSyncTime = NULL,
                            SyncRetryCount = SyncRetryCount + 1,
                            SyncError = @Error
                        WHERE Id = @Id
                    ";
                }
                
                cmd.Parameters.AddWithValue("@Id", record.Id);
                if (!string.IsNullOrEmpty(remoteId))
                    cmd.Parameters.AddWithValue("@RemoteId", remoteId);
                cmd.Parameters.AddWithValue("@Error", error ?? (object)DBNull.Value);
                
                await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"✅ Sync status updated for record {record.Id}: isSynced={isSynced}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ UpdateSyncStatusAsync Error: {ex.Message}");
        }
    }

    public async Task<int> MergeRemoteChangesAsync(List<DiseaseDetectionResult> remoteRecords)
    {
        int merged = 0;
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                foreach (var remote in remoteRecords)
                {
                    // Find local by RemoteId
                    var findCmd = conn.CreateCommand();
                    findCmd.CommandText = "SELECT * FROM DiseaseHistory WHERE RemoteId = @RemoteId";
                    findCmd.Parameters.AddWithValue("@RemoteId", remote.RemoteId ?? Guid.NewGuid().ToString());
                    
                    var reader = await findCmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        // Record exists locally
                        var local = MapFromReader(reader);
                        reader.Close();
                        
                        // Last-write-wins: compare timestamps
                        if (remote.LastModifiedDateUtc > local.LastModifiedDateUtc)
                        {
                            // Remote is newer - update local
                            var updateCmd = conn.CreateCommand();
                            updateCmd.CommandText = @"
                                UPDATE DiseaseHistory SET
                                    DiseaseName = @DiseaseName,
                                    Confidence = @Confidence,
                                    Severity = @Severity,
                                    Description = @Description,
                                    AffectedCropPart = @AffectedCropPart,
                                    CloudImageUrl = @CloudImageUrl,
                                    DeviceId = @DeviceId,
                                    DeviceName = @DeviceName,
                                    LastModifiedDateUtc = @LastModifiedDateUtc,
                                    Version = @Version
                                WHERE Id = @Id
                            ";
                            
                            updateCmd.Parameters.AddWithValue("@Id", local.Id);
                            updateCmd.Parameters.AddWithValue("@DiseaseName", remote.DiseaseName);
                            updateCmd.Parameters.AddWithValue("@Confidence", remote.Confidence);
                            updateCmd.Parameters.AddWithValue("@Severity", remote.Severity);
                            updateCmd.Parameters.AddWithValue("@Description", remote.Description ?? (object)DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@AffectedCropPart", remote.AffectedCropPart ?? (object)DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@CloudImageUrl", remote.CloudImageUrl ?? (object)DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@DeviceId", remote.DeviceId ?? (object)DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@DeviceName", remote.DeviceName ?? (object)DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@LastModifiedDateUtc", remote.LastModifiedDateUtc);
                            updateCmd.Parameters.AddWithValue("@Version", remote.Version);
                            
                            await updateCmd.ExecuteNonQueryAsync();
                            merged++;
                        }
                    }
                    else
                    {
                        // New record from server - insert locally
                        reader.Close();
                        var insertCmd = conn.CreateCommand();
                        insertCmd.CommandText = @"
                            INSERT INTO DiseaseHistory (
                                RemoteId, DiseaseName, Confidence, Severity, Description,
                                AffectedCropPart, CloudImageUrl, CreatedDateUtc, LastModifiedDateUtc, Version,
                                DeviceId, DeviceName
                            ) VALUES (
                                @RemoteId, @DiseaseName, @Confidence, @Severity, @Description,
                                @AffectedCropPart, @CloudImageUrl, @CreatedDateUtc, @LastModifiedDateUtc, @Version,
                                @DeviceId, @DeviceName
                            );
                            SELECT SCOPE_IDENTITY();
                        ";
                        
                        insertCmd.Parameters.AddWithValue("@RemoteId", remote.RemoteId ?? Guid.NewGuid().ToString());
                        insertCmd.Parameters.AddWithValue("@DiseaseName", remote.DiseaseName);
                        insertCmd.Parameters.AddWithValue("@Confidence", remote.Confidence);
                        insertCmd.Parameters.AddWithValue("@Severity", remote.Severity);
                        insertCmd.Parameters.AddWithValue("@Description", remote.Description ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@AffectedCropPart", remote.AffectedCropPart ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@CloudImageUrl", remote.CloudImageUrl ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@CreatedDateUtc", remote.CreatedDateUtc);
                        insertCmd.Parameters.AddWithValue("@LastModifiedDateUtc", remote.LastModifiedDateUtc);
                        insertCmd.Parameters.AddWithValue("@Version", remote.Version);
                        insertCmd.Parameters.AddWithValue("@DeviceId", remote.DeviceId ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@DeviceName", remote.DeviceName ?? (object)DBNull.Value);
                        
                        await insertCmd.ExecuteNonQueryAsync();
                        merged++;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ MergeRemoteChangesAsync Error: {ex.Message}");
        }
        return merged;
    }

    public async Task<DateTime?> GetLastSyncAnchorAsync()
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT MAX(LastModifiedDateUtc) FROM DiseaseHistory 
                    WHERE IsSynced = 1 AND IsDeleted = 0
                ";
                
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return (DateTime?)result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ GetLastSyncAnchorAsync Error: {ex.Message}");
        }
        return null;
    }

    public async Task SetLastSyncAnchorAsync(DateTime timestamp)
    {
        // Anchor is maintained via GetLastSyncAnchorAsync (uses DB timestamps)
        await Task.CompletedTask;
    }

    public async Task UpdateRemoteIdAsync(int localId, string remoteId)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE DiseaseHistory SET RemoteId = @RemoteId WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", localId);
                cmd.Parameters.AddWithValue("@RemoteId", remoteId);
                
                await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"✅ RemoteId updated for record {localId}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ UpdateRemoteIdAsync Error: {ex.Message}");
        }
    }

    public async Task<DiseaseDetectionResult?> GetByRemoteIdAsync(string remoteId)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM DiseaseHistory WHERE RemoteId = @RemoteId";
                cmd.Parameters.AddWithValue("@RemoteId", remoteId);
                
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapFromReader(reader);
                }
                reader.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ GetByRemoteIdAsync Error: {ex.Message}");
        }
        return null;
    }

    public async Task SoftDeleteAsync(DiseaseDetectionResult record)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE DiseaseHistory SET
                        IsDeleted = 1,
                        LastModifiedDateUtc = GETUTCDATE()
                    WHERE Id = @Id
                ";
                cmd.Parameters.AddWithValue("@Id", record.Id);
                
                await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine($"✅ Record {record.Id} soft-deleted");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ SoftDeleteAsync Error: {ex.Message}");
        }
    }

    // ===== HELPER METHODS =====

    private void AddParameters(IDbCommand cmd, DiseaseDetectionResult result)
    {
        cmd.Parameters.AddWithValue("@ImagePath", result.ImagePath ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@DiseaseName", result.DiseaseName);
        cmd.Parameters.AddWithValue("@Confidence", result.Confidence);
        cmd.Parameters.AddWithValue("@Severity", result.Severity);
        cmd.Parameters.AddWithValue("@DetectedDate", result.DetectedDate);
        cmd.Parameters.AddWithValue("@Description", result.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@AffectedCropPart", result.AffectedCropPart ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedDateUtc", result.CreatedDateUtc.ToUniversalTime());
        cmd.Parameters.AddWithValue("@LastModifiedDateUtc", result.LastModifiedDateUtc.ToUniversalTime());
        cmd.Parameters.AddWithValue("@DeviceId", result.DeviceId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@DeviceName", result.DeviceName ?? (object)DBNull.Value);
    }

    private DiseaseDetectionResult MapFromReader(IDataRecord reader)
    {
        return new DiseaseDetectionResult
        {
            Id = (int)reader["Id"],
            ImagePath = reader["ImagePath"] == DBNull.Value ? string.Empty : (string)reader["ImagePath"],
            DiseaseName = (string)reader["DiseaseName"],
            Confidence = (double)reader["Confidence"],
            Severity = (string)reader["Severity"],
            DetectedDate = (DateTime)reader["DetectedDate"],
            Description = reader["Description"] == DBNull.Value ? string.Empty : (string)reader["Description"],
            AffectedCropPart = reader["AffectedCropPart"] == DBNull.Value ? string.Empty : (string)reader["AffectedCropPart"],
            RemoteId = reader["RemoteId"] == DBNull.Value ? null : reader["RemoteId"].ToString(),
            IsSynced = (bool)reader["IsSynced"],
            LastSyncTime = reader["LastSyncTime"] == DBNull.Value ? null : (DateTime?)reader["LastSyncTime"],
            IsDeleted = (bool)reader["IsDeleted"],
            SyncRetryCount = (int)reader["SyncRetryCount"],
            SyncError = reader["SyncError"] == DBNull.Value ? null : (string)reader["SyncError"],
            CloudImageUrl = reader["CloudImageUrl"] == DBNull.Value ? null : (string)reader["CloudImageUrl"],
            ImageUploaded = (bool)reader["ImageUploaded"],
            CreatedDateUtc = (DateTime)reader["CreatedDateUtc"],
            LastModifiedDateUtc = (DateTime)reader["LastModifiedDateUtc"],
            Version = (int)reader["Version"],
            DeviceId = reader["DeviceId"] == DBNull.Value ? null : (string)reader["DeviceId"],
            DeviceName = reader["DeviceName"] == DBNull.Value ? null : (string)reader["DeviceName"]
        };
    }
}
