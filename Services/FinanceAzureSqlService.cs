using KrishiAI.App.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace KrishiAI.App.Services;

/// <summary>
/// Azure SQL Server implementation of finance data access.
/// Used when the device has internet connectivity.
/// Falls back gracefully when connection string is not configured.
/// </summary>
public class FinanceAzureSqlService : IFinanceAzureSqlService
{
    private readonly string _connectionString;

    public FinanceAzureSqlService()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__KrishiSql")
                 ?? Environment.GetEnvironmentVariable("KRISHI_SQL_CONNECTION_STRING");

        if (!string.IsNullOrWhiteSpace(cs))
        {
            _connectionString = cs;
            Debug.WriteLine("FinanceAzureSqlService: initialized from full connection string.");
            return;
        }

        var server   = Environment.GetEnvironmentVariable("KRISHI_SQL_SERVER");
        var database = Environment.GetEnvironmentVariable("KRISHI_SQL_DATABASE");
        var userId   = Environment.GetEnvironmentVariable("KRISHI_SQL_USER");
        var password = Environment.GetEnvironmentVariable("KRISHI_SQL_PASSWORD");

        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(userId)  || string.IsNullOrWhiteSpace(password))
        {
            // HARDCODED FALLBACK FOR EMULATOR TESTING
            _connectionString = "Data Source=azuredemodb.database.windows.net;Initial Catalog=free-sql-db-4227077;Persist Security Info=True;User ID=sqladmin;Password=Amazon@810649;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0;Connect Timeout=60";
            Debug.WriteLine("FinanceAzureSqlService: initialized from hardcoded connection string.");
            return;
        }

        _connectionString =
            $"Server={server};Database={database};User Id={userId};Password={password};" +
            "Encrypt=true;Connection Timeout=30;";
        Debug.WriteLine("FinanceAzureSqlService: initialized from individual env vars.");
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

    private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    // ================================================================
    // INCOME
    // ================================================================

    public async Task<int> AddIncomeAsync(IncomeTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO IncomeTransactions
                (UserId, TransactionType, Category, Description, Amount, TransactionDate,
                 CreatedDate, Notes, IsDeleted, CropName, Quantity, QuantityUnit, PricePerUnit, BuyerName)
            VALUES
                (@UserId, @TransactionType, @Category, @Description, @Amount, @TransactionDate,
                 @CreatedDate, @Notes, 0, @CropName, @Quantity, @QuantityUnit, @PricePerUnit, @BuyerName);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        AddIncomeParams(cmd, t);
        var id = (int)(await cmd.ExecuteScalarAsync())!;
        Debug.WriteLine($"FinanceAzureSqlService: Income {id} inserted.");
        return id;
    }

    public async Task<bool> UpdateIncomeAsync(IncomeTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE IncomeTransactions SET
                Category=@Category, Description=@Description, Amount=@Amount,
                TransactionDate=@TransactionDate, UpdatedDate=GETDATE(), Notes=@Notes,
                CropName=@CropName, Quantity=@Quantity, QuantityUnit=@QuantityUnit,
                PricePerUnit=@PricePerUnit, BuyerName=@BuyerName
            WHERE Id=@Id";
        AddIncomeParams(cmd, t);
        cmd.Parameters.AddWithValue("@Id", t.Id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteIncomeAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE IncomeTransactions SET IsDeleted=1, UpdatedDate=GETDATE() WHERE Id=@Id";
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<IncomeTransaction>> GetAllIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var list = new List<IncomeTransaction>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM IncomeTransactions
            WHERE UserId=@UserId AND IsDeleted=0
              AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
              AND (@EndDate   IS NULL OR TransactionDate <= @EndDate)
            ORDER BY TransactionDate DESC";
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EndDate",   (object?)endDate   ?? DBNull.Value);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapIncome(r));
        return list;
    }

    // ================================================================
    // EXPENSE
    // ================================================================

    public async Task<int> AddExpenseAsync(ExpenseTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ExpenseTransactions
                (UserId, TransactionType, Category, Description, Amount, TransactionDate,
                 CreatedDate, Notes, IsDeleted, ExpenseCategory, ExpenseName)
            VALUES
                (@UserId, @TransactionType, @Category, @Description, @Amount, @TransactionDate,
                 @CreatedDate, @Notes, 0, @ExpenseCategory, @ExpenseName);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        AddExpenseParams(cmd, t);
        var id = (int)(await cmd.ExecuteScalarAsync())!;
        Debug.WriteLine($"FinanceAzureSqlService: Expense {id} inserted.");
        return id;
    }

    public async Task<bool> UpdateExpenseAsync(ExpenseTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE ExpenseTransactions SET
                Category=@Category, Description=@Description, Amount=@Amount,
                TransactionDate=@TransactionDate, UpdatedDate=GETDATE(), Notes=@Notes,
                ExpenseCategory=@ExpenseCategory, ExpenseName=@ExpenseName
            WHERE Id=@Id";
        AddExpenseParams(cmd, t);
        cmd.Parameters.AddWithValue("@Id", t.Id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE ExpenseTransactions SET IsDeleted=1, UpdatedDate=GETDATE() WHERE Id=@Id";
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<ExpenseTransaction>> GetAllExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var list = new List<ExpenseTransaction>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM ExpenseTransactions
            WHERE UserId=@UserId AND IsDeleted=0
              AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
              AND (@EndDate   IS NULL OR TransactionDate <= @EndDate)
            ORDER BY TransactionDate DESC";
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EndDate",   (object?)endDate   ?? DBNull.Value);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapExpense(r));
        return list;
    }

    // ================================================================
    // LOAN
    // ================================================================

    public async Task<int> AddLoanAsync(LoanTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO LoanTransactions
                (UserId, TransactionType, Category, Description, Amount, TransactionDate,
                 CreatedDate, Notes, IsDeleted, LoanType, LenderName, InterestRate,
                 DueDate, IsRepaid, RemainingAmount)
            VALUES
                (@UserId, @TransactionType, @Category, @Description, @Amount, @TransactionDate,
                 @CreatedDate, @Notes, 0, @LoanType, @LenderName, @InterestRate,
                 @DueDate, 0, @RemainingAmount);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        AddLoanParams(cmd, t);
        var id = (int)(await cmd.ExecuteScalarAsync())!;
        Debug.WriteLine($"FinanceAzureSqlService: Loan {id} inserted.");
        return id;
    }

    public async Task<bool> UpdateLoanAsync(LoanTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE LoanTransactions SET
                Amount=@Amount, TransactionDate=@TransactionDate, UpdatedDate=GETDATE(),
                Notes=@Notes, LoanType=@LoanType, LenderName=@LenderName,
                InterestRate=@InterestRate, DueDate=@DueDate,
                IsRepaid=@IsRepaid, RemainingAmount=@RemainingAmount
            WHERE Id=@Id";
        AddLoanParams(cmd, t);
        cmd.Parameters.AddWithValue("@Id", t.Id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteLoanAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE LoanTransactions SET IsDeleted=1, UpdatedDate=GETDATE() WHERE Id=@Id";
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<LoanTransaction>> GetAllLoansAsync(string userId)
    {
        var list = new List<LoanTransaction>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM LoanTransactions WHERE UserId=@UserId AND IsDeleted=0 ORDER BY TransactionDate DESC";
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapLoan(r));
        return list;
    }

    public async Task<int> AddLoanRepaymentAsync(LoanRepayment t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        // Insert repayment
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO LoanRepayments (LoanTransactionId, AmountRepaid, RepaymentDate, Notes)
            VALUES (@LoanTransactionId, @AmountRepaid, @RepaymentDate, @Notes);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        cmd.Parameters.AddWithValue("@LoanTransactionId", t.LoanTransactionId);
        cmd.Parameters.AddWithValue("@AmountRepaid",      t.AmountRepaid);
        cmd.Parameters.AddWithValue("@RepaymentDate",     t.RepaymentDate);
        cmd.Parameters.AddWithValue("@Notes",             (object?)t.Notes ?? DBNull.Value);
        var id = (int)(await cmd.ExecuteScalarAsync())!;

        // Update remaining amount on the loan
        var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = @"
            UPDATE LoanTransactions
            SET RemainingAmount = RemainingAmount - @AmountRepaid,
                UpdatedDate = GETDATE()
            WHERE Id = @LoanId";
        updateCmd.Parameters.AddWithValue("@AmountRepaid", t.AmountRepaid);
        updateCmd.Parameters.AddWithValue("@LoanId",       t.LoanTransactionId);
        await updateCmd.ExecuteNonQueryAsync();

        return id;
    }

    public async Task<List<LoanRepayment>> GetLoanRepaymentsAsync(int loanId)
    {
        var list = new List<LoanRepayment>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM LoanRepayments WHERE LoanTransactionId=@LoanId ORDER BY RepaymentDate DESC";
        cmd.Parameters.AddWithValue("@LoanId", loanId);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapRepayment(r));
        return list;
    }

    // ================================================================
    // SUBSIDY
    // ================================================================

    public async Task<int> AddSubsidyAsync(SubsidyTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO SubsidyTransactions
                (UserId, TransactionType, Category, Description, Amount, TransactionDate,
                 CreatedDate, Notes, IsDeleted, SchemeName, SubsidyType, ReceivedDate)
            VALUES
                (@UserId, @TransactionType, @Category, @Description, @Amount, @TransactionDate,
                 @CreatedDate, @Notes, 0, @SchemeName, @SubsidyType, @ReceivedDate);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        AddSubsidyParams(cmd, t);
        var id = (int)(await cmd.ExecuteScalarAsync())!;
        Debug.WriteLine($"FinanceAzureSqlService: Subsidy {id} inserted.");
        return id;
    }

    public async Task<bool> UpdateSubsidyAsync(SubsidyTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE SubsidyTransactions SET
                Amount=@Amount, TransactionDate=@TransactionDate, UpdatedDate=GETDATE(),
                Notes=@Notes, SchemeName=@SchemeName, SubsidyType=@SubsidyType, ReceivedDate=@ReceivedDate
            WHERE Id=@Id";
        AddSubsidyParams(cmd, t);
        cmd.Parameters.AddWithValue("@Id", t.Id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteSubsidyAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE SubsidyTransactions SET IsDeleted=1, UpdatedDate=GETDATE() WHERE Id=@Id";
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<SubsidyTransaction>> GetAllSubsidiesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var list = new List<SubsidyTransaction>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM SubsidyTransactions
            WHERE UserId=@UserId AND IsDeleted=0
              AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
              AND (@EndDate   IS NULL OR TransactionDate <= @EndDate)
            ORDER BY TransactionDate DESC";
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EndDate",   (object?)endDate   ?? DBNull.Value);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapSubsidy(r));
        return list;
    }

    // ================================================================
    // MISCELLANEOUS
    // ================================================================

    public async Task<int> AddMiscTransactionAsync(MiscellaneousTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO MiscellaneousTransactions
                (UserId, TransactionType, Category, Description, Amount, TransactionDate,
                 CreatedDate, Notes, IsDeleted, TransactionDirection, MiscCategory)
            VALUES
                (@UserId, @TransactionType, @Category, @Description, @Amount, @TransactionDate,
                 @CreatedDate, @Notes, 0, @TransactionDirection, @MiscCategory);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        AddMiscParams(cmd, t);
        var id = (int)(await cmd.ExecuteScalarAsync())!;
        Debug.WriteLine($"FinanceAzureSqlService: Misc {id} inserted.");
        return id;
    }

    public async Task<bool> UpdateMiscTransactionAsync(MiscellaneousTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE MiscellaneousTransactions SET
                Amount=@Amount, TransactionDate=@TransactionDate, UpdatedDate=GETDATE(),
                Notes=@Notes, TransactionDirection=@TransactionDirection, MiscCategory=@MiscCategory
            WHERE Id=@Id";
        AddMiscParams(cmd, t);
        cmd.Parameters.AddWithValue("@Id", t.Id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteMiscTransactionAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE MiscellaneousTransactions SET IsDeleted=1, UpdatedDate=GETDATE() WHERE Id=@Id";
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<MiscellaneousTransaction>> GetAllMiscTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var list = new List<MiscellaneousTransaction>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM MiscellaneousTransactions
            WHERE UserId=@UserId AND IsDeleted=0
              AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
              AND (@EndDate   IS NULL OR TransactionDate <= @EndDate)
            ORDER BY TransactionDate DESC";
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EndDate",   (object?)endDate   ?? DBNull.Value);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapMisc(r));
        return list;
    }

    // ================================================================
    // SUMMARY
    // ================================================================

    public async Task<FinancialSummary> GetFinancialSummaryAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var incomes    = await GetAllIncomeAsync(userId, startDate, endDate);
        var expenses   = await GetAllExpensesAsync(userId, startDate, endDate);
        var subsidies  = await GetAllSubsidiesAsync(userId, startDate, endDate);
        var loans      = await GetAllLoansAsync(userId);
        var miscItems  = await GetAllMiscTransactionsAsync(userId, startDate, endDate);

        return new FinancialSummary
        {
            TotalIncome        = incomes.Sum(x => x.Amount),
            TotalExpense       = expenses.Sum(x => x.Amount),
            TotalSubsidy       = subsidies.Sum(x => x.Amount),
            TotalLoanTaken     = loans.Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate).Sum(x => x.Amount),
            TotalLoanRepaid    = 0,
            CropsSold          = incomes.Sum(x => x.Quantity),
            AveragePricePerUnit = incomes.Count > 0 ? incomes.Average(x => x.PricePerUnit) : 0,
            PeriodStart        = startDate,
            PeriodEnd          = endDate
        };
    }

    public async Task<List<FinanceTransaction>> GetAllTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var list = new List<FinanceTransaction>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM FinanceTransactions
            WHERE UserId=@UserId AND IsDeleted=0
              AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
              AND (@EndDate   IS NULL OR TransactionDate <= @EndDate)
            ORDER BY TransactionDate DESC";
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EndDate",   (object?)endDate   ?? DBNull.Value);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            list.Add(new FinanceTransaction
            {
                Id              = (int)r["Id"],
                UserId          = (string)r["UserId"],
                TransactionType = (string)r["TransactionType"],
                Category        = r["Category"]    == DBNull.Value ? string.Empty : (string)r["Category"],
                Description     = r["Description"] == DBNull.Value ? string.Empty : (string)r["Description"],
                Amount          = (decimal)r["Amount"],
                TransactionDate = (DateTime)r["TransactionDate"],
                CreatedDate     = (DateTime)r["CreatedDate"],
                UpdatedDate     = r["UpdatedDate"] == DBNull.Value ? null : (DateTime?)r["UpdatedDate"],
                Notes           = r["Notes"]       == DBNull.Value ? string.Empty : (string)r["Notes"],
                IsDeleted       = (bool)r["IsDeleted"]
            });
        }
        return list;
    }

    // ================================================================
    // PARAMETER HELPERS
    // ================================================================


    private static void AddIncomeParams(SqlCommand cmd, IncomeTransaction t)
    {
        cmd.Parameters.AddWithValue("@UserId",          t.UserId);
        cmd.Parameters.AddWithValue("@TransactionType", t.TransactionType);
        cmd.Parameters.AddWithValue("@Category",        string.IsNullOrEmpty(t.Category) ? (object)DBNull.Value : t.Category);
        cmd.Parameters.AddWithValue("@Description",     string.IsNullOrEmpty(t.Description) ? (object)DBNull.Value : t.Description);
        cmd.Parameters.AddWithValue("@Amount",          t.Amount);
        cmd.Parameters.AddWithValue("@TransactionDate", t.TransactionDate);
        cmd.Parameters.AddWithValue("@CreatedDate",     t.CreatedDate);
        cmd.Parameters.AddWithValue("@Notes",           string.IsNullOrEmpty(t.Notes) ? (object)DBNull.Value : t.Notes);
        cmd.Parameters.AddWithValue("@CropName",        t.CropName);
        cmd.Parameters.AddWithValue("@Quantity",        t.Quantity);
        cmd.Parameters.AddWithValue("@QuantityUnit",    t.QuantityUnit);
        cmd.Parameters.AddWithValue("@PricePerUnit",    t.PricePerUnit);
        cmd.Parameters.AddWithValue("@BuyerName",       string.IsNullOrEmpty(t.BuyerName) ? (object)DBNull.Value : t.BuyerName);
    }

    private static void AddExpenseParams(SqlCommand cmd, ExpenseTransaction t)
    {
        cmd.Parameters.AddWithValue("@UserId",          t.UserId);
        cmd.Parameters.AddWithValue("@TransactionType", t.TransactionType);
        cmd.Parameters.AddWithValue("@Category",        string.IsNullOrEmpty(t.Category) ? (object)DBNull.Value : t.Category);
        cmd.Parameters.AddWithValue("@Description",     string.IsNullOrEmpty(t.Description) ? (object)DBNull.Value : t.Description);
        cmd.Parameters.AddWithValue("@Amount",          t.Amount);
        cmd.Parameters.AddWithValue("@TransactionDate", t.TransactionDate);
        cmd.Parameters.AddWithValue("@CreatedDate",     t.CreatedDate);
        cmd.Parameters.AddWithValue("@Notes",           string.IsNullOrEmpty(t.Notes) ? (object)DBNull.Value : t.Notes);
        cmd.Parameters.AddWithValue("@ExpenseCategory", t.ExpenseCategory);
        cmd.Parameters.AddWithValue("@ExpenseName",     t.ExpenseName);
    }

    private static void AddLoanParams(SqlCommand cmd, LoanTransaction t)
    {
        cmd.Parameters.AddWithValue("@UserId",          t.UserId);
        cmd.Parameters.AddWithValue("@TransactionType", t.TransactionType);
        cmd.Parameters.AddWithValue("@Category",        string.IsNullOrEmpty(t.Category) ? (object)DBNull.Value : t.Category);
        cmd.Parameters.AddWithValue("@Description",     string.IsNullOrEmpty(t.Description) ? (object)DBNull.Value : t.Description);
        cmd.Parameters.AddWithValue("@Amount",          t.Amount);
        cmd.Parameters.AddWithValue("@TransactionDate", t.TransactionDate);
        cmd.Parameters.AddWithValue("@CreatedDate",     t.CreatedDate);
        cmd.Parameters.AddWithValue("@Notes",           string.IsNullOrEmpty(t.Notes) ? (object)DBNull.Value : t.Notes);
        cmd.Parameters.AddWithValue("@LoanType",        t.LoanType);
        cmd.Parameters.AddWithValue("@LenderName",      t.LenderName);
        cmd.Parameters.AddWithValue("@InterestRate",    t.InterestRate);
        cmd.Parameters.AddWithValue("@DueDate",         t.DueDate);
        cmd.Parameters.AddWithValue("@IsRepaid",        t.IsRepaid);
        cmd.Parameters.AddWithValue("@RemainingAmount", t.RemainingAmount);
    }

    private static void AddSubsidyParams(SqlCommand cmd, SubsidyTransaction t)
    {
        cmd.Parameters.AddWithValue("@UserId",          t.UserId);
        cmd.Parameters.AddWithValue("@TransactionType", t.TransactionType);
        cmd.Parameters.AddWithValue("@Category",        string.IsNullOrEmpty(t.Category) ? (object)DBNull.Value : t.Category);
        cmd.Parameters.AddWithValue("@Description",     string.IsNullOrEmpty(t.Description) ? (object)DBNull.Value : t.Description);
        cmd.Parameters.AddWithValue("@Amount",          t.Amount);
        cmd.Parameters.AddWithValue("@TransactionDate", t.TransactionDate);
        cmd.Parameters.AddWithValue("@CreatedDate",     t.CreatedDate);
        cmd.Parameters.AddWithValue("@Notes",           string.IsNullOrEmpty(t.Notes) ? (object)DBNull.Value : t.Notes);
        cmd.Parameters.AddWithValue("@SchemeName",      t.SchemeName);
        cmd.Parameters.AddWithValue("@SubsidyType",     t.SubsidyType);
        cmd.Parameters.AddWithValue("@ReceivedDate",    t.ReceivedDate);
    }

    private static void AddMiscParams(SqlCommand cmd, MiscellaneousTransaction t)
    {
        cmd.Parameters.AddWithValue("@UserId",               t.UserId);
        cmd.Parameters.AddWithValue("@TransactionType",      t.TransactionType);
        cmd.Parameters.AddWithValue("@Category",             string.IsNullOrEmpty(t.Category) ? (object)DBNull.Value : t.Category);
        cmd.Parameters.AddWithValue("@Description",          string.IsNullOrEmpty(t.Description) ? (object)DBNull.Value : t.Description);
        cmd.Parameters.AddWithValue("@Amount",               t.Amount);
        cmd.Parameters.AddWithValue("@TransactionDate",      t.TransactionDate);
        cmd.Parameters.AddWithValue("@CreatedDate",          t.CreatedDate);
        cmd.Parameters.AddWithValue("@Notes",                string.IsNullOrEmpty(t.Notes) ? (object)DBNull.Value : t.Notes);
        cmd.Parameters.AddWithValue("@TransactionDirection", t.TransactionDirection);
        cmd.Parameters.AddWithValue("@MiscCategory",         t.MiscCategory);
    }

    // ================================================================
    // MAPPING HELPERS
    // ================================================================

    private static IncomeTransaction MapIncome(IDataRecord r) => new()
    {
        Id              = (int)r["Id"],
        UserId          = (string)r["UserId"],
        TransactionType = (string)r["TransactionType"],
        Category        = r["Category"]    == DBNull.Value ? string.Empty : (string)r["Category"],
        Description     = r["Description"] == DBNull.Value ? string.Empty : (string)r["Description"],
        Amount          = (decimal)r["Amount"],
        TransactionDate = (DateTime)r["TransactionDate"],
        CreatedDate     = (DateTime)r["CreatedDate"],
        UpdatedDate     = r["UpdatedDate"] == DBNull.Value ? null : (DateTime?)r["UpdatedDate"],
        Notes           = r["Notes"]       == DBNull.Value ? string.Empty : (string)r["Notes"],
        IsDeleted       = (bool)r["IsDeleted"],
        IsSynced        = true,
        CropName        = (string)r["CropName"],
        Quantity        = (decimal)r["Quantity"],
        QuantityUnit    = (string)r["QuantityUnit"],
        PricePerUnit    = (decimal)r["PricePerUnit"],
        BuyerName       = r["BuyerName"] == DBNull.Value ? string.Empty : (string)r["BuyerName"]
    };

    private static ExpenseTransaction MapExpense(IDataRecord r) => new()
    {
        Id              = (int)r["Id"],
        UserId          = (string)r["UserId"],
        TransactionType = (string)r["TransactionType"],
        Category        = r["Category"]    == DBNull.Value ? string.Empty : (string)r["Category"],
        Description     = r["Description"] == DBNull.Value ? string.Empty : (string)r["Description"],
        Amount          = (decimal)r["Amount"],
        TransactionDate = (DateTime)r["TransactionDate"],
        CreatedDate     = (DateTime)r["CreatedDate"],
        UpdatedDate     = r["UpdatedDate"] == DBNull.Value ? null : (DateTime?)r["UpdatedDate"],
        Notes           = r["Notes"]       == DBNull.Value ? string.Empty : (string)r["Notes"],
        IsDeleted       = (bool)r["IsDeleted"],
        IsSynced        = true,
        ExpenseCategory = (string)r["ExpenseCategory"],
        ExpenseName     = (string)r["ExpenseName"]
    };

    private static LoanTransaction MapLoan(IDataRecord r) => new()
    {
        Id              = (int)r["Id"],
        UserId          = (string)r["UserId"],
        TransactionType = (string)r["TransactionType"],
        Category        = r["Category"]    == DBNull.Value ? string.Empty : (string)r["Category"],
        Description     = r["Description"] == DBNull.Value ? string.Empty : (string)r["Description"],
        Amount          = (decimal)r["Amount"],
        TransactionDate = (DateTime)r["TransactionDate"],
        CreatedDate     = (DateTime)r["CreatedDate"],
        UpdatedDate     = r["UpdatedDate"] == DBNull.Value ? null : (DateTime?)r["UpdatedDate"],
        Notes           = r["Notes"]       == DBNull.Value ? string.Empty : (string)r["Notes"],
        IsDeleted       = (bool)r["IsDeleted"],
        IsSynced        = true,
        LoanType        = (string)r["LoanType"],
        LenderName      = (string)r["LenderName"],
        InterestRate    = (decimal)r["InterestRate"],
        DueDate         = (DateTime)r["DueDate"],
        IsRepaid        = (bool)r["IsRepaid"],
        RemainingAmount = (decimal)r["RemainingAmount"]
    };

    private static LoanRepayment MapRepayment(IDataRecord r) => new()
    {
        Id                = (int)r["Id"],
        LoanTransactionId = (int)r["LoanTransactionId"],
        AmountRepaid      = (decimal)r["AmountRepaid"],
        RepaymentDate     = (DateTime)r["RepaymentDate"],
        Notes             = r["Notes"] == DBNull.Value ? string.Empty : (string)r["Notes"],
        IsSynced          = true
    };

    private static SubsidyTransaction MapSubsidy(IDataRecord r) => new()
    {
        Id              = (int)r["Id"],
        UserId          = (string)r["UserId"],
        TransactionType = (string)r["TransactionType"],
        Category        = r["Category"]    == DBNull.Value ? string.Empty : (string)r["Category"],
        Description     = r["Description"] == DBNull.Value ? string.Empty : (string)r["Description"],
        Amount          = (decimal)r["Amount"],
        TransactionDate = (DateTime)r["TransactionDate"],
        CreatedDate     = (DateTime)r["CreatedDate"],
        UpdatedDate     = r["UpdatedDate"] == DBNull.Value ? null : (DateTime?)r["UpdatedDate"],
        Notes           = r["Notes"]       == DBNull.Value ? string.Empty : (string)r["Notes"],
        IsDeleted       = (bool)r["IsDeleted"],
        IsSynced        = true,
        SchemeName      = (string)r["SchemeName"],
        SubsidyType     = (string)r["SubsidyType"],
        ReceivedDate    = (DateTime)r["ReceivedDate"]
    };

    private static MiscellaneousTransaction MapMisc(IDataRecord r) => new()
    {
        Id                   = (int)r["Id"],
        UserId               = (string)r["UserId"],
        TransactionType      = (string)r["TransactionType"],
        Category             = r["Category"]    == DBNull.Value ? string.Empty : (string)r["Category"],
        Description          = r["Description"] == DBNull.Value ? string.Empty : (string)r["Description"],
        Amount               = (decimal)r["Amount"],
        TransactionDate      = (DateTime)r["TransactionDate"],
        CreatedDate          = (DateTime)r["CreatedDate"],
        UpdatedDate          = r["UpdatedDate"] == DBNull.Value ? null : (DateTime?)r["UpdatedDate"],
        Notes                = r["Notes"]       == DBNull.Value ? string.Empty : (string)r["Notes"],
        IsDeleted            = (bool)r["IsDeleted"],
        IsSynced             = true,
        TransactionDirection = (string)r["TransactionDirection"],
        MiscCategory         = (string)r["MiscCategory"]
    };
}
