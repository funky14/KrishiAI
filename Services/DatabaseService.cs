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
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<DiseaseDetectionResult>();
            Debug.WriteLine($"Database initialized at: {dbPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"InitializeAsync Error: {ex.Message}");
        }
    }

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
}
