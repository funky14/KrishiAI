using KrishiAI.App.Models;
using SQLite;
using System.Diagnostics;

namespace KrishiAI.App.Services;

/// <summary>
/// Connectivity-aware router for finance data.
///
/// ONLINE  → all reads/writes go directly to Azure SQL via IFinanceAzureSqlService.
/// OFFLINE → all reads/writes go to local SQLite (IsSynced=false).
///           FinanceSyncService will push them to Azure SQL when internet is restored
///           and delete the SQLite copies after a successful push.
/// </summary>
public class FinanceService : IFinanceService
{
    private readonly IFinanceAzureSqlService _azure;
    private readonly IConnectivityService _connectivity;
    private SQLiteAsyncConnection? _db;

    private readonly string _userId;

    public FinanceService(IFinanceAzureSqlService azure, IConnectivityService connectivity)
    {
        _azure        = azure;
        _connectivity = connectivity;
        // Hardcoded for hackathon demo to ensure mock data is always visible
        _userId       = "hackathon_demo_user";
    }

    // ----------------------------------------------------------------
    // SQLite initialisation (tables created idempotently on every call)
    // ----------------------------------------------------------------
    private async Task InitSqliteAsync()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "krishiai.db3");

        var dbDir = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(dbDir))
            Directory.CreateDirectory(dbDir!);

        // Create connection once; always run CreateTableAsync for auto-migration.
        _db ??= new SQLiteAsyncConnection(dbPath);

        await _db.CreateTableAsync<FinanceTransaction>();
        await _db.CreateTableAsync<LoanRepayment>();
    }

    private bool IsOnline => _connectivity.IsConnected() && _azure.IsConfigured;

    private async Task<int> InsertTransactionAsync(FinanceTransaction t, string type)
    {
        t.UserId          = _userId;
        t.TransactionType = type;
        t.CreatedDate     = DateTime.Now;

        if (IsOnline)
        {
            Debug.WriteLine($"FinanceService: Add{type} → Azure SQL");
            return type switch
            {
                "Income" => await _azure.AddIncomeAsync(t),
                "Expense" => await _azure.AddExpenseAsync(t),
                "Loan" => await _azure.AddLoanAsync(t),
                "Subsidy" => await _azure.AddSubsidyAsync(t),
                "Miscellaneous" => await _azure.AddMiscTransactionAsync(t),
                _ => throw new NotImplementedException()
            };
        }

        Debug.WriteLine($"FinanceService: Add{type} → SQLite (offline)");
        await InitSqliteAsync();
        t.IsSynced = false;
        return await _db!.InsertAsync(t);
    }

    private async Task<bool> UpdateTransactionAsync(FinanceTransaction t)
    {
        t.UpdatedDate = DateTime.Now;

        if (IsOnline)
        {
            return t.TransactionType switch
            {
                "Income" => await _azure.UpdateIncomeAsync(t),
                "Expense" => await _azure.UpdateExpenseAsync(t),
                "Loan" => await _azure.UpdateLoanAsync(t),
                "Subsidy" => await _azure.UpdateSubsidyAsync(t),
                "Miscellaneous" => await _azure.UpdateMiscTransactionAsync(t),
                _ => throw new NotImplementedException()
            };
        }

        await InitSqliteAsync();
        t.IsSynced = false;
        return await _db!.UpdateAsync(t) > 0;
    }

    private async Task<bool> DeleteTransactionAsync(int id, string type)
    {
        if (IsOnline)
        {
            return type switch
            {
                "Income" => await _azure.DeleteIncomeAsync(id),
                "Expense" => await _azure.DeleteExpenseAsync(id),
                "Loan" => await _azure.DeleteLoanAsync(id),
                "Subsidy" => await _azure.DeleteSubsidyAsync(id),
                "Miscellaneous" => await _azure.DeleteMiscTransactionAsync(id),
                _ => throw new NotImplementedException()
            };
        }

        await InitSqliteAsync();
        return await _db!.DeleteAsync<FinanceTransaction>(id) > 0;
    }

    private async Task<FinanceTransaction?> GetTransactionByIdAsync(int id, string type)
    {
        if (IsOnline)
        {
            var all = type switch
            {
                "Income" => await _azure.GetAllIncomeAsync(_userId),
                "Expense" => await _azure.GetAllExpensesAsync(_userId),
                "Loan" => await _azure.GetAllLoansAsync(_userId),
                "Subsidy" => await _azure.GetAllSubsidiesAsync(_userId),
                "Miscellaneous" => await _azure.GetAllMiscTransactionsAsync(_userId),
                _ => new List<FinanceTransaction>()
            };
            return all.FirstOrDefault(x => x.Id == id);
        }

        await InitSqliteAsync();
        return await _db!.Table<FinanceTransaction>()
            .Where(x => x.Id == id && x.UserId == _userId && x.TransactionType == type)
            .FirstOrDefaultAsync();
    }

    private async Task<List<FinanceTransaction>> GetTransactionsByTypeAsync(string type, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (IsOnline)
        {
            return type switch
            {
                "Income" => await _azure.GetAllIncomeAsync(_userId, startDate, endDate),
                "Expense" => await _azure.GetAllExpensesAsync(_userId, startDate, endDate),
                "Loan" => await _azure.GetAllLoansAsync(_userId), // Loans don't always use date filter in get all
                "Subsidy" => await _azure.GetAllSubsidiesAsync(_userId, startDate, endDate),
                "Miscellaneous" => await _azure.GetAllMiscTransactionsAsync(_userId, startDate, endDate),
                _ => new List<FinanceTransaction>()
            };
        }

        await InitSqliteAsync();
        var q = _db!.Table<FinanceTransaction>().Where(x => x.UserId == _userId && x.TransactionType == type && !x.IsDeleted);
        if (startDate.HasValue) q = q.Where(x => x.TransactionDate >= startDate.Value);
        if (endDate.HasValue)   q = q.Where(x => x.TransactionDate <= endDate.Value);
        return await q.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    // ================================================================
    // INCOME
    // ================================================================
    public Task<int> AddIncomeAsync(FinanceTransaction income) => InsertTransactionAsync(income, "Income");
    public Task<bool> UpdateIncomeAsync(FinanceTransaction income) => UpdateTransactionAsync(income);
    public Task<bool> DeleteIncomeAsync(int id) => DeleteTransactionAsync(id, "Income");
    public Task<FinanceTransaction?> GetIncomeByIdAsync(int id) => GetTransactionByIdAsync(id, "Income");
    public Task<List<FinanceTransaction>> GetAllIncomeAsync(DateTime? startDate = null, DateTime? endDate = null) => GetTransactionsByTypeAsync("Income", startDate, endDate);

    // ================================================================
    // EXPENSE
    // ================================================================
    public Task<int> AddExpenseAsync(FinanceTransaction expense) => InsertTransactionAsync(expense, "Expense");
    public Task<bool> UpdateExpenseAsync(FinanceTransaction expense) => UpdateTransactionAsync(expense);
    public Task<bool> DeleteExpenseAsync(int id) => DeleteTransactionAsync(id, "Expense");
    public Task<FinanceTransaction?> GetExpenseByIdAsync(int id) => GetTransactionByIdAsync(id, "Expense");
    public Task<List<FinanceTransaction>> GetAllExpensesAsync(DateTime? startDate = null, DateTime? endDate = null) => GetTransactionsByTypeAsync("Expense", startDate, endDate);

    // ================================================================
    // LOAN
    // ================================================================
    public Task<int> AddLoanAsync(FinanceTransaction loan)
    {
        loan.RemainingAmount = loan.Amount;
        return InsertTransactionAsync(loan, "Loan");
    }
    public Task<bool> UpdateLoanAsync(FinanceTransaction loan) => UpdateTransactionAsync(loan);
    public Task<bool> DeleteLoanAsync(int id) => DeleteTransactionAsync(id, "Loan");
    public Task<FinanceTransaction?> GetLoanByIdAsync(int id) => GetTransactionByIdAsync(id, "Loan");
    public Task<List<FinanceTransaction>> GetAllLoansAsync() => GetTransactionsByTypeAsync("Loan");

    public async Task<int> AddLoanRepaymentAsync(LoanRepayment repayment)
    {
        repayment.RepaymentDate = DateTime.Now;

        if (IsOnline)
            return await _azure.AddLoanRepaymentAsync(repayment);

        await InitSqliteAsync();
        repayment.IsSynced = false;
        
        // Also update remaining amount locally
        var loan = await _db!.Table<FinanceTransaction>().FirstOrDefaultAsync(x => x.Id == repayment.LoanTransactionId);
        if (loan != null)
        {
            loan.RemainingAmount -= repayment.AmountRepaid;
            await _db.UpdateAsync(loan);
        }

        return await _db!.InsertAsync(repayment);
    }

    public async Task<List<LoanRepayment>> GetLoanRepaymentsAsync(int loanId)
    {
        if (IsOnline)
            return await _azure.GetLoanRepaymentsAsync(loanId);

        await InitSqliteAsync();
        return await _db!.Table<LoanRepayment>()
            .Where(x => x.LoanTransactionId == loanId)
            .OrderByDescending(x => x.RepaymentDate)
            .ToListAsync();
    }

    // ================================================================
    // SUBSIDY
    // ================================================================
    public Task<int> AddSubsidyAsync(FinanceTransaction subsidy) => InsertTransactionAsync(subsidy, "Subsidy");
    public Task<bool> UpdateSubsidyAsync(FinanceTransaction subsidy) => UpdateTransactionAsync(subsidy);
    public Task<bool> DeleteSubsidyAsync(int id) => DeleteTransactionAsync(id, "Subsidy");
    public Task<FinanceTransaction?> GetSubsidyByIdAsync(int id) => GetTransactionByIdAsync(id, "Subsidy");
    public Task<List<FinanceTransaction>> GetAllSubsidiesAsync(DateTime? startDate = null, DateTime? endDate = null) => GetTransactionsByTypeAsync("Subsidy", startDate, endDate);

    // ================================================================
    // MISCELLANEOUS
    // ================================================================
    public Task<int> AddMiscTransactionAsync(FinanceTransaction misc) => InsertTransactionAsync(misc, "Miscellaneous");
    public Task<bool> UpdateMiscTransactionAsync(FinanceTransaction misc) => UpdateTransactionAsync(misc);
    public Task<bool> DeleteMiscTransactionAsync(int id) => DeleteTransactionAsync(id, "Miscellaneous");
    public Task<FinanceTransaction?> GetMiscTransactionByIdAsync(int id) => GetTransactionByIdAsync(id, "Miscellaneous");
    public Task<List<FinanceTransaction>> GetAllMiscTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null) => GetTransactionsByTypeAsync("Miscellaneous", startDate, endDate);

    // ================================================================
    // SUMMARY & ANALYTICS
    // ================================================================

    public async Task<FinancialSummary> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate)
    {
        if (IsOnline)
            return await _azure.GetFinancialSummaryAsync(_userId, startDate, endDate);

        await InitSqliteAsync();
        var incomes   = await GetAllIncomeAsync(startDate, endDate);
        var expenses  = await GetAllExpensesAsync(startDate, endDate);
        var subsidies = await GetAllSubsidiesAsync(startDate, endDate);
        var loans     = await GetAllLoansAsync();
        var misc      = await GetAllMiscTransactionsAsync(startDate, endDate);

        return new FinancialSummary
        {
            TotalIncome         = incomes.Sum(x => x.Amount),
            TotalExpense        = expenses.Sum(x => x.Amount),
            TotalSubsidy        = subsidies.Sum(x => x.Amount),
            TotalLoanTaken      = loans.Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate).Sum(x => x.Amount),
            TotalLoanRepaid     = 0,
            CropsSold           = incomes.Sum(x => x.Quantity),
            AveragePricePerUnit = incomes.Count > 0 ? incomes.Average(x => x.PricePerUnit) : 0,
            PeriodStart         = startDate,
            PeriodEnd           = endDate
        };
    }

    public async Task<List<FinanceTransaction>> GetAllTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (IsOnline)
            return await _azure.GetAllTransactionsAsync(_userId, startDate, endDate);

        await InitSqliteAsync();
        var q = _db!.Table<FinanceTransaction>().Where(x => x.UserId == _userId && !x.IsDeleted);
        if (startDate.HasValue) q = q.Where(x => x.TransactionDate >= startDate.Value);
        if (endDate.HasValue)   q = q.Where(x => x.TransactionDate <= endDate.Value);
        return await q.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    public async Task<List<FinanceTransaction>> GetTransactionsByCategoryAsync(string category, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (IsOnline)
        {
            var all = await _azure.GetAllTransactionsAsync(_userId, startDate, endDate);
            return all.Where(x => x.Category == category).ToList();
        }

        await InitSqliteAsync();
        var q = _db!.Table<FinanceTransaction>()
            .Where(x => x.UserId == _userId && x.Category == category && !x.IsDeleted);
        if (startDate.HasValue) q = q.Where(x => x.TransactionDate >= startDate.Value);
        if (endDate.HasValue)   q = q.Where(x => x.TransactionDate <= endDate.Value);
        return await q.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var expenses = await GetAllExpensesAsync(startDate, endDate);
        return expenses
            .GroupBy(x => string.IsNullOrEmpty(x.ExpenseCategory) ? x.Category : x.ExpenseCategory)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
    }
}
