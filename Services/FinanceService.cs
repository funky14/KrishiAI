using KrishiAI.App.Models;
using SQLite;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class FinanceService : IFinanceService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _userId;

    public FinanceService()
    {
        _userId = Preferences.Default.Get("user_id", Guid.NewGuid().ToString());
    }

    private async Task InitializeAsync()
    {
        try
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "krishiai.db3");
            
            // Ensure directory exists
            var dbDir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir!);

            _database = new SQLiteAsyncConnection(dbPath);

            // Create tables with explicit CreateTablesAsync
            await _database.CreateTableAsync<FinanceTransaction>();
            await _database.CreateTableAsync<IncomeTransaction>();
            await _database.CreateTableAsync<ExpenseTransaction>();
            await _database.CreateTableAsync<LoanTransaction>();
            await _database.CreateTableAsync<LoanRepayment>();
            await _database.CreateTableAsync<SubsidyTransaction>();
            await _database.CreateTableAsync<MiscellaneousTransaction>();

            Debug.WriteLine($"Finance tables initialized successfully at {dbPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"InitializeAsync Error: {ex.Message}");
            Debug.WriteLine($"InitializeAsync Stack: {ex.StackTrace}");
            throw; // Re-throw to see the error
        }
    }

    // ========== INCOME OPERATIONS ==========
    public async Task<int> AddIncomeAsync(IncomeTransaction income)
    {
        await InitializeAsync();
        income.UserId = _userId;
        income.TransactionType = "Income";
        income.CreatedDate = DateTime.Now;
        return await _database!.InsertAsync(income);
    }

    public async Task<bool> UpdateIncomeAsync(IncomeTransaction income)
    {
        await InitializeAsync();
        income.UpdatedDate = DateTime.Now;
        return await _database!.UpdateAsync(income) > 0;
    }

    public async Task<bool> DeleteIncomeAsync(int incomeId)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync<IncomeTransaction>(incomeId) > 0;
    }

    public async Task<IncomeTransaction?> GetIncomeByIdAsync(int incomeId)
    {
        await InitializeAsync();
        return await _database!.Table<IncomeTransaction>()
            .Where(x => x.Id == incomeId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<IncomeTransaction>> GetAllIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitializeAsync();
        var query = _database!.Table<IncomeTransaction>()
            .Where(x => x.UserId == _userId && !x.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(x => x.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.TransactionDate <= endDate.Value);

        return await query.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    // ========== EXPENSE OPERATIONS ==========
    public async Task<int> AddExpenseAsync(ExpenseTransaction expense)
    {
        await InitializeAsync();
        expense.UserId = _userId;
        expense.TransactionType = "Expense";
        expense.CreatedDate = DateTime.Now;
        return await _database!.InsertAsync(expense);
    }

    public async Task<bool> UpdateExpenseAsync(ExpenseTransaction expense)
    {
        await InitializeAsync();
        expense.UpdatedDate = DateTime.Now;
        return await _database!.UpdateAsync(expense) > 0;
    }

    public async Task<bool> DeleteExpenseAsync(int expenseId)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync<ExpenseTransaction>(expenseId) > 0;
    }

    public async Task<ExpenseTransaction?> GetExpenseByIdAsync(int expenseId)
    {
        await InitializeAsync();
        return await _database!.Table<ExpenseTransaction>()
            .Where(x => x.Id == expenseId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ExpenseTransaction>> GetAllExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitializeAsync();
        var query = _database!.Table<ExpenseTransaction>()
            .Where(x => x.UserId == _userId && !x.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(x => x.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.TransactionDate <= endDate.Value);

        return await query.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    // ========== LOAN OPERATIONS ==========
    public async Task<int> AddLoanAsync(LoanTransaction loan)
    {
        await InitializeAsync();
        loan.UserId = _userId;
        loan.TransactionType = "Loan";
        loan.CreatedDate = DateTime.Now;
        loan.RemainingAmount = loan.Amount;
        return await _database!.InsertAsync(loan);
    }

    public async Task<bool> UpdateLoanAsync(LoanTransaction loan)
    {
        await InitializeAsync();
        loan.UpdatedDate = DateTime.Now;
        return await _database!.UpdateAsync(loan) > 0;
    }

    public async Task<bool> DeleteLoanAsync(int loanId)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync<LoanTransaction>(loanId) > 0;
    }

    public async Task<LoanTransaction?> GetLoanByIdAsync(int loanId)
    {
        await InitializeAsync();
        return await _database!.Table<LoanTransaction>()
            .Where(x => x.Id == loanId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LoanTransaction>> GetAllLoansAsync()
    {
        await InitializeAsync();
        return await _database!.Table<LoanTransaction>()
            .Where(x => x.UserId == _userId && !x.IsDeleted)
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();
    }

    public async Task<int> AddLoanRepaymentAsync(LoanRepayment repayment)
    {
        await InitializeAsync();
        repayment.RepaymentDate = DateTime.Now;
        return await _database!.InsertAsync(repayment);
    }

    public async Task<List<LoanRepayment>> GetLoanRepaymentsAsync(int loanId)
    {
        await InitializeAsync();
        return await _database!.Table<LoanRepayment>()
            .Where(x => x.LoanTransactionId == loanId)
            .OrderByDescending(x => x.RepaymentDate)
            .ToListAsync();
    }

    // ========== SUBSIDY OPERATIONS ==========
    public async Task<int> AddSubsidyAsync(SubsidyTransaction subsidy)
    {
        await InitializeAsync();
        subsidy.UserId = _userId;
        subsidy.TransactionType = "Subsidy";
        subsidy.CreatedDate = DateTime.Now;
        return await _database!.InsertAsync(subsidy);
    }

    public async Task<bool> UpdateSubsidyAsync(SubsidyTransaction subsidy)
    {
        await InitializeAsync();
        subsidy.UpdatedDate = DateTime.Now;
        return await _database!.UpdateAsync(subsidy) > 0;
    }

    public async Task<bool> DeleteSubsidyAsync(int subsidyId)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync<SubsidyTransaction>(subsidyId) > 0;
    }

    public async Task<SubsidyTransaction?> GetSubsidyByIdAsync(int subsidyId)
    {
        await InitializeAsync();
        return await _database!.Table<SubsidyTransaction>()
            .Where(x => x.Id == subsidyId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SubsidyTransaction>> GetAllSubsidiesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitializeAsync();
        var query = _database!.Table<SubsidyTransaction>()
            .Where(x => x.UserId == _userId && !x.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(x => x.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.TransactionDate <= endDate.Value);

        return await query.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    // ========== MISCELLANEOUS OPERATIONS ==========
    public async Task<int> AddMiscTransactionAsync(MiscellaneousTransaction misc)
    {
        await InitializeAsync();
        misc.UserId = _userId;
        misc.TransactionType = "Miscellaneous";
        misc.CreatedDate = DateTime.Now;
        return await _database!.InsertAsync(misc);
    }

    public async Task<bool> UpdateMiscTransactionAsync(MiscellaneousTransaction misc)
    {
        await InitializeAsync();
        misc.UpdatedDate = DateTime.Now;
        return await _database!.UpdateAsync(misc) > 0;
    }

    public async Task<bool> DeleteMiscTransactionAsync(int miscId)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync<MiscellaneousTransaction>(miscId) > 0;
    }

    public async Task<MiscellaneousTransaction?> GetMiscTransactionByIdAsync(int miscId)
    {
        await InitializeAsync();
        return await _database!.Table<MiscellaneousTransaction>()
            .Where(x => x.Id == miscId && x.UserId == _userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<MiscellaneousTransaction>> GetAllMiscTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitializeAsync();
        var query = _database!.Table<MiscellaneousTransaction>()
            .Where(x => x.UserId == _userId && !x.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(x => x.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.TransactionDate <= endDate.Value);

        return await query.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    // ========== SUMMARY AND ANALYTICS ==========
    public async Task<FinancialSummary> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate)
    {
        await InitializeAsync();

        var incomes = await GetAllIncomeAsync(startDate, endDate);
        var expenses = await GetAllExpensesAsync(startDate, endDate);
        var subsidies = await GetAllSubsidiesAsync(startDate, endDate);
        var loans = await GetAllLoansAsync();
        var miscTransactions = await GetAllMiscTransactionsAsync(startDate, endDate);

        var summary = new FinancialSummary
        {
            TotalIncome = incomes.Sum(x => x.Amount),
            TotalExpense = expenses.Sum(x => x.Amount),
            TotalSubsidy = subsidies.Sum(x => x.Amount),
            TotalLoanTaken = loans.Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate).Sum(x => x.Amount),
            TotalLoanRepaid = 0, // Will calculate from repayments
            CropsSold = incomes.Sum(x => x.Quantity),
            AveragePricePerUnit = incomes.Count > 0 ? incomes.Average(x => x.PricePerUnit) : 0,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };

        return summary;
    }

    public async Task<List<FinanceTransaction>> GetAllTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitializeAsync();
        var query = _database!.Table<FinanceTransaction>()
            .Where(x => x.UserId == _userId && !x.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(x => x.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.TransactionDate <= endDate.Value);

        return await query.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    public async Task<List<FinanceTransaction>> GetTransactionsByCategoryAsync(string category, DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitializeAsync();
        var query = _database!.Table<FinanceTransaction>()
            .Where(x => x.UserId == _userId && x.Category == category && !x.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(x => x.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.TransactionDate <= endDate.Value);

        return await query.OrderByDescending(x => x.TransactionDate).ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var expenses = await GetAllExpensesAsync(startDate, endDate);
        return expenses
            .GroupBy(x => x.ExpenseCategory)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
    }
}
