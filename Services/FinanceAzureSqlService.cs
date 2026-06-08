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
    // COMMON HELPER
    // ================================================================

    private static void AddTransactionParams(SqlCommand cmd, FinanceTransaction t)
    {
        cmd.Parameters.AddWithValue("@UserId",          t.UserId);
        cmd.Parameters.AddWithValue("@TransactionType", t.TransactionType);
        cmd.Parameters.AddWithValue("@Category",        string.IsNullOrEmpty(t.Category) ? (object)DBNull.Value : t.Category);
        cmd.Parameters.AddWithValue("@Description",     string.IsNullOrEmpty(t.Description) ? (object)DBNull.Value : t.Description);
        cmd.Parameters.AddWithValue("@Amount",          t.Amount);
        cmd.Parameters.AddWithValue("@TransactionDate", t.TransactionDate);
        cmd.Parameters.AddWithValue("@CreatedDate",     t.CreatedDate);
        cmd.Parameters.AddWithValue("@Notes",           string.IsNullOrEmpty(t.Notes) ? (object)DBNull.Value : t.Notes);
        
        cmd.Parameters.AddWithValue("@CropName",        string.IsNullOrEmpty(t.CropName) ? (object)DBNull.Value : t.CropName);
        cmd.Parameters.AddWithValue("@Quantity",        t.Quantity);
        cmd.Parameters.AddWithValue("@QuantityUnit",    string.IsNullOrEmpty(t.QuantityUnit) ? (object)DBNull.Value : t.QuantityUnit);
        cmd.Parameters.AddWithValue("@PricePerUnit",    t.PricePerUnit);
        cmd.Parameters.AddWithValue("@BuyerName",       string.IsNullOrEmpty(t.BuyerName) ? (object)DBNull.Value : t.BuyerName);

        cmd.Parameters.AddWithValue("@ExpenseCategory", string.IsNullOrEmpty(t.ExpenseCategory) ? (object)DBNull.Value : t.ExpenseCategory);
        cmd.Parameters.AddWithValue("@ExpenseName",     string.IsNullOrEmpty(t.ExpenseName) ? (object)DBNull.Value : t.ExpenseName);

        cmd.Parameters.AddWithValue("@LoanType",        string.IsNullOrEmpty(t.LoanType) ? (object)DBNull.Value : t.LoanType);
        cmd.Parameters.AddWithValue("@LenderName",      string.IsNullOrEmpty(t.LenderName) ? (object)DBNull.Value : t.LenderName);
        cmd.Parameters.AddWithValue("@InterestRate",    t.InterestRate);
        cmd.Parameters.AddWithValue("@DueDate",         (object?)t.DueDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsRepaid",        t.IsRepaid);
        cmd.Parameters.AddWithValue("@RemainingAmount", t.RemainingAmount);

        cmd.Parameters.AddWithValue("@SchemeName",      string.IsNullOrEmpty(t.SchemeName) ? (object)DBNull.Value : t.SchemeName);
        cmd.Parameters.AddWithValue("@SubsidyType",     string.IsNullOrEmpty(t.SubsidyType) ? (object)DBNull.Value : t.SubsidyType);
        cmd.Parameters.AddWithValue("@ReceivedDate",    (object?)t.ReceivedDate ?? DBNull.Value);

        cmd.Parameters.AddWithValue("@TransactionDirection", string.IsNullOrEmpty(t.TransactionDirection) ? (object)DBNull.Value : t.TransactionDirection);
        cmd.Parameters.AddWithValue("@MiscCategory",         string.IsNullOrEmpty(t.MiscCategory) ? (object)DBNull.Value : t.MiscCategory);
    }

    private static FinanceTransaction MapTransaction(IDataRecord r) => new()
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

        CropName        = r["CropName"] == DBNull.Value ? string.Empty : (string)r["CropName"],
        Quantity        = r["Quantity"] == DBNull.Value ? 0 : (decimal)r["Quantity"],
        QuantityUnit    = r["QuantityUnit"] == DBNull.Value ? string.Empty : (string)r["QuantityUnit"],
        PricePerUnit    = r["PricePerUnit"] == DBNull.Value ? 0 : (decimal)r["PricePerUnit"],
        BuyerName       = r["BuyerName"] == DBNull.Value ? string.Empty : (string)r["BuyerName"],

        ExpenseCategory = r["ExpenseCategory"] == DBNull.Value ? string.Empty : (string)r["ExpenseCategory"],
        ExpenseName     = r["ExpenseName"] == DBNull.Value ? string.Empty : (string)r["ExpenseName"],

        LoanType        = r["LoanType"] == DBNull.Value ? string.Empty : (string)r["LoanType"],
        LenderName      = r["LenderName"] == DBNull.Value ? string.Empty : (string)r["LenderName"],
        InterestRate    = r["InterestRate"] == DBNull.Value ? 0 : (decimal)r["InterestRate"],
        DueDate         = r["DueDate"] == DBNull.Value ? null : (DateTime?)r["DueDate"],
        IsRepaid        = r["IsRepaid"] == DBNull.Value ? false : (bool)r["IsRepaid"],
        RemainingAmount = r["RemainingAmount"] == DBNull.Value ? 0 : (decimal)r["RemainingAmount"],

        SchemeName      = r["SchemeName"] == DBNull.Value ? string.Empty : (string)r["SchemeName"],
        SubsidyType     = r["SubsidyType"] == DBNull.Value ? string.Empty : (string)r["SubsidyType"],
        ReceivedDate    = r["ReceivedDate"] == DBNull.Value ? null : (DateTime?)r["ReceivedDate"],

        TransactionDirection = r["TransactionDirection"] == DBNull.Value ? string.Empty : (string)r["TransactionDirection"],
        MiscCategory         = r["MiscCategory"] == DBNull.Value ? string.Empty : (string)r["MiscCategory"]
    };

    private async Task<int> InsertTransactionAsync(FinanceTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO FinanceTransactions
                (UserId, TransactionType, Category, Description, Amount, TransactionDate,
                 CreatedDate, Notes, IsDeleted, CropName, Quantity, QuantityUnit, PricePerUnit, BuyerName,
                 ExpenseCategory, ExpenseName, LoanType, LenderName, InterestRate, DueDate, IsRepaid, RemainingAmount,
                 SchemeName, SubsidyType, ReceivedDate, TransactionDirection, MiscCategory)
            VALUES
                (@UserId, @TransactionType, @Category, @Description, @Amount, @TransactionDate,
                 @CreatedDate, @Notes, 0, @CropName, @Quantity, @QuantityUnit, @PricePerUnit, @BuyerName,
                 @ExpenseCategory, @ExpenseName, @LoanType, @LenderName, @InterestRate, @DueDate, @IsRepaid, @RemainingAmount,
                 @SchemeName, @SubsidyType, @ReceivedDate, @TransactionDirection, @MiscCategory);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        AddTransactionParams(cmd, t);
        var id = (int)(await cmd.ExecuteScalarAsync())!;
        Debug.WriteLine($"FinanceAzureSqlService: {t.TransactionType} {id} inserted.");
        return id;
    }

    private async Task<bool> UpdateTransactionAsync(FinanceTransaction t)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE FinanceTransactions SET
                Category=@Category, Description=@Description, Amount=@Amount,
                TransactionDate=@TransactionDate, UpdatedDate=GETDATE(), Notes=@Notes,
                CropName=@CropName, Quantity=@Quantity, QuantityUnit=@QuantityUnit, PricePerUnit=@PricePerUnit, BuyerName=@BuyerName,
                ExpenseCategory=@ExpenseCategory, ExpenseName=@ExpenseName,
                LoanType=@LoanType, LenderName=@LenderName, InterestRate=@InterestRate, DueDate=@DueDate, IsRepaid=@IsRepaid, RemainingAmount=@RemainingAmount,
                SchemeName=@SchemeName, SubsidyType=@SubsidyType, ReceivedDate=@ReceivedDate,
                TransactionDirection=@TransactionDirection, MiscCategory=@MiscCategory
            WHERE Id=@Id";
        AddTransactionParams(cmd, t);
        cmd.Parameters.AddWithValue("@Id", t.Id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private async Task<bool> DeleteTransactionAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE FinanceTransactions SET IsDeleted=1, UpdatedDate=GETDATE() WHERE Id=@Id";
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private async Task<List<FinanceTransaction>> GetTransactionsByTypeAsync(string type, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var list = new List<FinanceTransaction>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM FinanceTransactions
            WHERE UserId=@UserId AND TransactionType=@Type AND IsDeleted=0
              AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
              AND (@EndDate   IS NULL OR TransactionDate <= @EndDate)
            ORDER BY TransactionDate DESC";
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Type", type);
        cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EndDate",   (object?)endDate   ?? DBNull.Value);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapTransaction(r));
        return list;
    }

    // ================================================================
    // INCOME
    // ================================================================

    public Task<int> AddIncomeAsync(FinanceTransaction t) => InsertTransactionAsync(t);
    public Task<bool> UpdateIncomeAsync(FinanceTransaction t) => UpdateTransactionAsync(t);
    public Task<bool> DeleteIncomeAsync(int id) => DeleteTransactionAsync(id);
    public Task<List<FinanceTransaction>> GetAllIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null) 
        => GetTransactionsByTypeAsync("Income", userId, startDate, endDate);

    // ================================================================
    // EXPENSE
    // ================================================================

    public Task<int> AddExpenseAsync(FinanceTransaction t) => InsertTransactionAsync(t);
    public Task<bool> UpdateExpenseAsync(FinanceTransaction t) => UpdateTransactionAsync(t);
    public Task<bool> DeleteExpenseAsync(int id) => DeleteTransactionAsync(id);
    public Task<List<FinanceTransaction>> GetAllExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null) 
        => GetTransactionsByTypeAsync("Expense", userId, startDate, endDate);

    // ================================================================
    // LOAN
    // ================================================================

    public Task<int> AddLoanAsync(FinanceTransaction t) => InsertTransactionAsync(t);
    public Task<bool> UpdateLoanAsync(FinanceTransaction t) => UpdateTransactionAsync(t);
    public Task<bool> DeleteLoanAsync(int id) => DeleteTransactionAsync(id);
    public Task<List<FinanceTransaction>> GetAllLoansAsync(string userId) 
        => GetTransactionsByTypeAsync("Loan", userId);

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
            UPDATE FinanceTransactions
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
        while (await r.ReadAsync())
        {
            list.Add(new LoanRepayment
            {
                Id                = (int)r["Id"],
                LoanTransactionId = (int)r["LoanTransactionId"],
                AmountRepaid      = (decimal)r["AmountRepaid"],
                RepaymentDate     = (DateTime)r["RepaymentDate"],
                Notes             = r["Notes"] == DBNull.Value ? string.Empty : (string)r["Notes"],
                IsSynced          = true
            });
        }
        return list;
    }

    // ================================================================
    // SUBSIDY
    // ================================================================

    public Task<int> AddSubsidyAsync(FinanceTransaction t) => InsertTransactionAsync(t);
    public Task<bool> UpdateSubsidyAsync(FinanceTransaction t) => UpdateTransactionAsync(t);
    public Task<bool> DeleteSubsidyAsync(int id) => DeleteTransactionAsync(id);
    public Task<List<FinanceTransaction>> GetAllSubsidiesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null) 
        => GetTransactionsByTypeAsync("Subsidy", userId, startDate, endDate);

    // ================================================================
    // MISCELLANEOUS
    // ================================================================

    public Task<int> AddMiscTransactionAsync(FinanceTransaction t) => InsertTransactionAsync(t);
    public Task<bool> UpdateMiscTransactionAsync(FinanceTransaction t) => UpdateTransactionAsync(t);
    public Task<bool> DeleteMiscTransactionAsync(int id) => DeleteTransactionAsync(id);
    public Task<List<FinanceTransaction>> GetAllMiscTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null) 
        => GetTransactionsByTypeAsync("Miscellaneous", userId, startDate, endDate);

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
            list.Add(MapTransaction(r));
        }
        return list;
    }
}
