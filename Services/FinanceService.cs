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
        _userId       = Preferences.Default.Get("user_id", Guid.NewGuid().ToString());
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
        await _db.CreateTableAsync<IncomeTransaction>();
        await _db.CreateTableAsync<ExpenseTransaction>();
        await _db.CreateTableAsync<LoanTransaction>();
        await _db.CreateTableAsync<LoanRepayment>();
        await _db.CreateTableAsync<SubsidyTransaction>();
        await _db.CreateTableAsync<MiscellaneousTransaction>();
    }

    private bool IsOnline => _connectivity.IsConnected() && _azure.IsConfigured;

    // ================================================================
    // INCOME
    // ================================================================

    public async Task<int> AddIncomeAsync(IncomeTransaction income)
    {
        income.UserId          = _userId;
        income.TransactionType = "Income";
        income.CreatedDate     = DateTime.Now;

        if (IsOnline)
        {
            Debug.WriteLine("FinanceService: AddIncome → Azure SQL");
            return await _azure.AddIncomeAsync(income);
        }

        Debug.WriteLine("FinanceService: AddIncome → SQLite (offline)");
        await InitSqliteAsync();
        income.IsSynced = false;
        return await _db!.InsertAsync(income);
    }

    public async Task<bool> UpdateIncomeAsync(IncomeTransaction income)
    {
        income.UpdatedDate = DateTime.Now;

        if (IsOnline)
            return await _azure.UpdateIncomeAsync(income);

        await InitSqliteAsync();
        income.IsSynced = false;
        return await _db!.UpdateAsync(income) > 0;
    }

    public async Task<bool> DeleteIncomeAsync(int incomeId)
    {
        if (IsOnline)
            return await _azure.DeleteIncomeAsync(incomeId);

        await InitSqliteAsync();
        return await _db!.DeleteAsync<IncomeTransaction>(incomeId) > 0;
    }

    public async Task<IncomeTransaction?> GetIncomeByIdAsync(int incomeId)
    {
        if (IsOnline)
        {
            var all = await _azure.GetAllIncomeAsync(_userId);
            return all.FirstOrDefault(x => x.Id == incomeId);
        }

        await InitSqliteAsync();
        return await _db!.Table<IncomeTransaction>()
            .Where(x => x.Id == incomeId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<IncomeTransaction>> GetAllIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (IsOnline)
            return await _azure.GetAllIncomeAsync(_userId, startDate, endDate);

        await InitSqliteAsync();
        var q = _db!.Table<IncomeTransaction>().Where(x => x.UserId == _userId && !x.IsDeleted);
        if (startDate.HasValue) q = q.Where(x => x.TransactionDate >= startDate.Value);
        if (endDate.HasValue)   q = q.Where(x => x.TransactionDate <= endDate.Value);
        return await q.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    // ================================================================
    // EXPENSE
    // ================================================================

    public async Task<int> AddExpenseAsync(ExpenseTransaction expense)
    {
        expense.UserId          = _userId;
        expense.TransactionType = "Expense";
        expense.CreatedDate     = DateTime.Now;

        if (IsOnline)
        {
            Debug.WriteLine("FinanceService: AddExpense → Azure SQL");
            return await _azure.AddExpenseAsync(expense);
        }

        Debug.WriteLine("FinanceService: AddExpense → SQLite (offline)");
        await InitSqliteAsync();
        expense.IsSynced = false;
        return await _db!.InsertAsync(expense);
    }

    public async Task<bool> UpdateExpenseAsync(ExpenseTransaction expense)
    {
        expense.UpdatedDate = DateTime.Now;

        if (IsOnline)
            return await _azure.UpdateExpenseAsync(expense);

        await InitSqliteAsync();
        expense.IsSynced = false;
        return await _db!.UpdateAsync(expense) > 0;
    }

    public async Task<bool> DeleteExpenseAsync(int expenseId)
    {
        if (IsOnline)
            return await _azure.DeleteExpenseAsync(expenseId);

        await InitSqliteAsync();
        return await _db!.DeleteAsync<ExpenseTransaction>(expenseId) > 0;
    }

    public async Task<ExpenseTransaction?> GetExpenseByIdAsync(int expenseId)
    {
        if (IsOnline)
        {
            var all = await _azure.GetAllExpensesAsync(_userId);
            return all.FirstOrDefault(x => x.Id == expenseId);
        }

        await InitSqliteAsync();
        return await _db!.Table<ExpenseTransaction>()
            .Where(x => x.Id == expenseId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ExpenseTransaction>> GetAllExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (IsOnline)
            return await _azure.GetAllExpensesAsync(_userId, startDate, endDate);

        await InitSqliteAsync();
        var q = _db!.Table<ExpenseTransaction>().Where(x => x.UserId == _userId && !x.IsDeleted);
        if (startDate.HasValue) q = q.Where(x => x.TransactionDate >= startDate.Value);
        if (endDate.HasValue)   q = q.Where(x => x.TransactionDate <= endDate.Value);
        return await q.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    // ================================================================
    // LOAN
    // ================================================================

    public async Task<int> AddLoanAsync(LoanTransaction loan)
    {
        loan.UserId          = _userId;
        loan.TransactionType = "Loan";
        loan.CreatedDate     = DateTime.Now;
        loan.RemainingAmount = loan.Amount;

        if (IsOnline)
        {
            Debug.WriteLine("FinanceService: AddLoan → Azure SQL");
            return await _azure.AddLoanAsync(loan);
        }

        Debug.WriteLine("FinanceService: AddLoan → SQLite (offline)");
        await InitSqliteAsync();
        loan.IsSynced = false;
        return await _db!.InsertAsync(loan);
    }

    public async Task<bool> UpdateLoanAsync(LoanTransaction loan)
    {
        loan.UpdatedDate = DateTime.Now;

        if (IsOnline)
            return await _azure.UpdateLoanAsync(loan);

        await InitSqliteAsync();
        loan.IsSynced = false;
        return await _db!.UpdateAsync(loan) > 0;
    }

    public async Task<bool> DeleteLoanAsync(int loanId)
    {
        if (IsOnline)
            return await _azure.DeleteLoanAsync(loanId);

        await InitSqliteAsync();
        return await _db!.DeleteAsync<LoanTransaction>(loanId) > 0;
    }

    public async Task<LoanTransaction?> GetLoanByIdAsync(int loanId)
    {
        if (IsOnline)
        {
            var all = await _azure.GetAllLoansAsync(_userId);
            return all.FirstOrDefault(x => x.Id == loanId);
        }

        await InitSqliteAsync();
        return await _db!.Table<LoanTransaction>()
            .Where(x => x.Id == loanId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LoanTransaction>> GetAllLoansAsync()
    {
        if (IsOnline)
            return await _azure.GetAllLoansAsync(_userId);

        await InitSqliteAsync();
        return await _db!.Table<LoanTransaction>()
            .Where(x => x.UserId == _userId && !x.IsDeleted)
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();
    }

    public async Task<int> AddLoanRepaymentAsync(LoanRepayment repayment)
    {
        repayment.RepaymentDate = DateTime.Now;

        if (IsOnline)
            return await _azure.AddLoanRepaymentAsync(repayment);

        await InitSqliteAsync();
        repayment.IsSynced = false;
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

    public async Task<int> AddSubsidyAsync(SubsidyTransaction subsidy)
    {
        subsidy.UserId          = _userId;
        subsidy.TransactionType = "Subsidy";
        subsidy.CreatedDate     = DateTime.Now;

        if (IsOnline)
        {
            Debug.WriteLine("FinanceService: AddSubsidy → Azure SQL");
            return await _azure.AddSubsidyAsync(subsidy);
        }

        Debug.WriteLine("FinanceService: AddSubsidy → SQLite (offline)");
        await InitSqliteAsync();
        subsidy.IsSynced = false;
        return await _db!.InsertAsync(subsidy);
    }

    public async Task<bool> UpdateSubsidyAsync(SubsidyTransaction subsidy)
    {
        subsidy.UpdatedDate = DateTime.Now;

        if (IsOnline)
            return await _azure.UpdateSubsidyAsync(subsidy);

        await InitSqliteAsync();
        subsidy.IsSynced = false;
        return await _db!.UpdateAsync(subsidy) > 0;
    }

    public async Task<bool> DeleteSubsidyAsync(int subsidyId)
    {
        if (IsOnline)
            return await _azure.DeleteSubsidyAsync(subsidyId);

        await InitSqliteAsync();
        return await _db!.DeleteAsync<SubsidyTransaction>(subsidyId) > 0;
    }

    public async Task<SubsidyTransaction?> GetSubsidyByIdAsync(int subsidyId)
    {
        if (IsOnline)
        {
            var all = await _azure.GetAllSubsidiesAsync(_userId);
            return all.FirstOrDefault(x => x.Id == subsidyId);
        }

        await InitSqliteAsync();
        return await _db!.Table<SubsidyTransaction>()
            .Where(x => x.Id == subsidyId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SubsidyTransaction>> GetAllSubsidiesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (IsOnline)
            return await _azure.GetAllSubsidiesAsync(_userId, startDate, endDate);

        await InitSqliteAsync();
        var q = _db!.Table<SubsidyTransaction>().Where(x => x.UserId == _userId && !x.IsDeleted);
        if (startDate.HasValue) q = q.Where(x => x.TransactionDate >= startDate.Value);
        if (endDate.HasValue)   q = q.Where(x => x.TransactionDate <= endDate.Value);
        return await q.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    // ================================================================
    // MISCELLANEOUS
    // ================================================================

    public async Task<int> AddMiscTransactionAsync(MiscellaneousTransaction misc)
    {
        misc.UserId          = _userId;
        misc.TransactionType = "Miscellaneous";
        misc.CreatedDate     = DateTime.Now;

        if (IsOnline)
        {
            Debug.WriteLine("FinanceService: AddMisc → Azure SQL");
            return await _azure.AddMiscTransactionAsync(misc);
        }

        Debug.WriteLine("FinanceService: AddMisc → SQLite (offline)");
        await InitSqliteAsync();
        misc.IsSynced = false;
        return await _db!.InsertAsync(misc);
    }

    public async Task<bool> UpdateMiscTransactionAsync(MiscellaneousTransaction misc)
    {
        misc.UpdatedDate = DateTime.Now;

        if (IsOnline)
            return await _azure.UpdateMiscTransactionAsync(misc);

        await InitSqliteAsync();
        misc.IsSynced = false;
        return await _db!.UpdateAsync(misc) > 0;
    }

    public async Task<bool> DeleteMiscTransactionAsync(int miscId)
    {
        if (IsOnline)
            return await _azure.DeleteMiscTransactionAsync(miscId);

        await InitSqliteAsync();
        return await _db!.DeleteAsync<MiscellaneousTransaction>(miscId) > 0;
    }

    public async Task<MiscellaneousTransaction?> GetMiscTransactionByIdAsync(int miscId)
    {
        if (IsOnline)
        {
            var all = await _azure.GetAllMiscTransactionsAsync(_userId);
            return all.FirstOrDefault(x => x.Id == miscId);
        }

        await InitSqliteAsync();
        return await _db!.Table<MiscellaneousTransaction>()
            .Where(x => x.Id == miscId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<MiscellaneousTransaction>> GetAllMiscTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (IsOnline)
            return await _azure.GetAllMiscTransactionsAsync(_userId, startDate, endDate);

        await InitSqliteAsync();
        var q = _db!.Table<MiscellaneousTransaction>().Where(x => x.UserId == _userId && !x.IsDeleted);
        if (startDate.HasValue) q = q.Where(x => x.TransactionDate >= startDate.Value);
        if (endDate.HasValue)   q = q.Where(x => x.TransactionDate <= endDate.Value);
        return await q.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

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
            .GroupBy(x => x.ExpenseCategory)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
    }
}
