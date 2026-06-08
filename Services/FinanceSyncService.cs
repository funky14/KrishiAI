using KrishiAI.App.Models;
using SQLite;
using System.Diagnostics;

namespace KrishiAI.App.Services;

/// <summary>
/// Pushes unsynced SQLite finance records to Azure SQL when internet is available.
/// After a successful sync, the local SQLite copy is DELETED (SQLite is a temp buffer only).
/// </summary>
public class FinanceSyncService
{
    private readonly IFinanceAzureSqlService _azureService;
    private readonly IConnectivityService _connectivity;
    private SQLiteAsyncConnection? _db;
    private bool _isSyncing = false;

    public FinanceSyncService(IFinanceAzureSqlService azureService, IConnectivityService connectivity)
    {
        _azureService = azureService;
        _connectivity = connectivity;
    }

    // ----------------------------------------------------------------
    // Entry point — called on connectivity restored and on app resume
    // ----------------------------------------------------------------
    public async Task SyncPendingAsync()
    {
        if (_isSyncing)
        {
            Debug.WriteLine("FinanceSyncService: sync already in progress, skipping.");
            return;
        }

        if (!_connectivity.IsConnected())
        {
            Debug.WriteLine("FinanceSyncService: no internet, skipping sync.");
            return;
        }

        if (!_azureService.IsConfigured)
        {
            Debug.WriteLine("FinanceSyncService: Azure SQL not configured, skipping sync.");
            return;
        }

        _isSyncing = true;
        try
        {
            await EnsureDbAsync();

            Debug.WriteLine("FinanceSyncService: starting sync of pending offline records...");

            int synced = 0;
            synced += await SyncIncomeAsync();
            synced += await SyncExpensesAsync();
            synced += await SyncLoansAsync();
            synced += await SyncLoanRepaymentsAsync();
            synced += await SyncSubsidiesAsync();
            synced += await SyncMiscAsync();

            Debug.WriteLine($"FinanceSyncService: sync complete — {synced} record(s) pushed and removed from SQLite.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FinanceSyncService: sync error — {ex.Message}");
        }
        finally
        {
            _isSyncing = false;
        }
    }

    // ----------------------------------------------------------------
    // Per-table sync helpers
    // ----------------------------------------------------------------

    private async Task<int> SyncIncomeAsync()
    {
        var pending = await _db!.Table<IncomeTransaction>()
            .Where(x => !x.IsSynced && !x.IsDeleted)
            .ToListAsync();

        int count = 0;
        foreach (var record in pending)
        {
            try
            {
                await _azureService.AddIncomeAsync(record);
                await _db.DeleteAsync(record);  // remove from SQLite after successful push
                count++;
                Debug.WriteLine($"FinanceSyncService: Income SQLite#{record.Id} synced and deleted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FinanceSyncService: Income SQLite#{record.Id} failed — {ex.Message}");
            }
        }
        return count;
    }

    private async Task<int> SyncExpensesAsync()
    {
        var pending = await _db!.Table<ExpenseTransaction>()
            .Where(x => !x.IsSynced && !x.IsDeleted)
            .ToListAsync();

        int count = 0;
        foreach (var record in pending)
        {
            try
            {
                await _azureService.AddExpenseAsync(record);
                await _db.DeleteAsync(record);
                count++;
                Debug.WriteLine($"FinanceSyncService: Expense SQLite#{record.Id} synced and deleted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FinanceSyncService: Expense SQLite#{record.Id} failed — {ex.Message}");
            }
        }
        return count;
    }

    private async Task<int> SyncLoansAsync()
    {
        var pending = await _db!.Table<LoanTransaction>()
            .Where(x => !x.IsSynced && !x.IsDeleted)
            .ToListAsync();

        int count = 0;
        foreach (var record in pending)
        {
            try
            {
                await _azureService.AddLoanAsync(record);
                await _db.DeleteAsync(record);
                count++;
                Debug.WriteLine($"FinanceSyncService: Loan SQLite#{record.Id} synced and deleted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FinanceSyncService: Loan SQLite#{record.Id} failed — {ex.Message}");
            }
        }
        return count;
    }

    private async Task<int> SyncLoanRepaymentsAsync()
    {
        var pending = await _db!.Table<LoanRepayment>()
            .Where(x => !x.IsSynced)
            .ToListAsync();

        int count = 0;
        foreach (var record in pending)
        {
            try
            {
                await _azureService.AddLoanRepaymentAsync(record);
                await _db.DeleteAsync(record);
                count++;
                Debug.WriteLine($"FinanceSyncService: LoanRepayment SQLite#{record.Id} synced and deleted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FinanceSyncService: LoanRepayment SQLite#{record.Id} failed — {ex.Message}");
            }
        }
        return count;
    }

    private async Task<int> SyncSubsidiesAsync()
    {
        var pending = await _db!.Table<SubsidyTransaction>()
            .Where(x => !x.IsSynced && !x.IsDeleted)
            .ToListAsync();

        int count = 0;
        foreach (var record in pending)
        {
            try
            {
                await _azureService.AddSubsidyAsync(record);
                await _db.DeleteAsync(record);
                count++;
                Debug.WriteLine($"FinanceSyncService: Subsidy SQLite#{record.Id} synced and deleted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FinanceSyncService: Subsidy SQLite#{record.Id} failed — {ex.Message}");
            }
        }
        return count;
    }

    private async Task<int> SyncMiscAsync()
    {
        var pending = await _db!.Table<MiscellaneousTransaction>()
            .Where(x => !x.IsSynced && !x.IsDeleted)
            .ToListAsync();

        int count = 0;
        foreach (var record in pending)
        {
            try
            {
                await _azureService.AddMiscTransactionAsync(record);
                await _db.DeleteAsync(record);
                count++;
                Debug.WriteLine($"FinanceSyncService: Misc SQLite#{record.Id} synced and deleted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FinanceSyncService: Misc SQLite#{record.Id} failed — {ex.Message}");
            }
        }
        return count;
    }

    // ----------------------------------------------------------------
    // Shared SQLite connection (same db file as FinanceService)
    // ----------------------------------------------------------------
    private async Task EnsureDbAsync()
    {
        if (_db != null) return;
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "krishiai.db3");
        _db = new SQLiteAsyncConnection(dbPath);

        // Ensure tables exist (idempotent — safe to call even if already created)
        await _db.CreateTableAsync<FinanceTransaction>();
        await _db.CreateTableAsync<IncomeTransaction>();
        await _db.CreateTableAsync<ExpenseTransaction>();
        await _db.CreateTableAsync<LoanTransaction>();
        await _db.CreateTableAsync<LoanRepayment>();
        await _db.CreateTableAsync<SubsidyTransaction>();
        await _db.CreateTableAsync<MiscellaneousTransaction>();
    }
}
