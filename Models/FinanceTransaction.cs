using SQLite;

namespace KrishiAI.App.Models;

/// <summary>
/// Base model for all financial transactions
/// </summary>
[Table("FinanceTransactions")]
public class FinanceTransaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    public string TransactionType { get; set; } = string.Empty; // Income, Expense, Loan, Subsidy, Misc

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? UpdatedDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    /// <summary>False until this record has been successfully pushed to Azure SQL.</summary>
    public bool IsSynced { get; set; } = false;
}

/// <summary>
/// Income/Sales transaction
/// </summary>
[Table("IncomeTransactions")]
public class IncomeTransaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    public string TransactionType { get; set; } = "Income";

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? UpdatedDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    /// <summary>False until this record has been successfully pushed to Azure SQL.</summary>
    public bool IsSynced { get; set; } = false;

    // Income-specific fields
    public string CropName { get; set; } = string.Empty;

    public decimal Quantity { get; set; } // in Quintals

    public string QuantityUnit { get; set; } = "Quintal";

    public decimal PricePerUnit { get; set; }

    public string BuyerName { get; set; } = string.Empty;

    [Ignore]
    public decimal TotalAmount => Quantity * PricePerUnit;
}

/// <summary>
/// Expense transaction
/// </summary>
[Table("ExpenseTransactions")]
public class ExpenseTransaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    public string TransactionType { get; set; } = "Expense";

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? UpdatedDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    /// <summary>False until this record has been successfully pushed to Azure SQL.</summary>
    public bool IsSynced { get; set; } = false;

    // Expense-specific fields
    public string ExpenseCategory { get; set; } = string.Empty; // Seeds, Fertilizer, Water, Labor, etc.

    public string ExpenseName { get; set; } = string.Empty;
}

/// <summary>
/// Loan transaction
/// </summary>
[Table("LoanTransactions")]
public class LoanTransaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    public string TransactionType { get; set; } = "Loan";

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? UpdatedDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    /// <summary>False until this record has been successfully pushed to Azure SQL.</summary>
    public bool IsSynced { get; set; } = false;

    // Loan-specific fields
    public string LoanType { get; set; } = string.Empty; // Bank, Cooperative, Private, Government

    public string LenderName { get; set; } = string.Empty;

    public decimal InterestRate { get; set; } // Annual percentage

    public DateTime DueDate { get; set; }

    public bool IsRepaid { get; set; } = false;

    public decimal RemainingAmount { get; set; }

    [Ignore]
    public List<LoanRepayment> Repayments { get; set; } = new List<LoanRepayment>();
}

/// <summary>
/// Loan repayment record
/// </summary>
[Table("LoanRepayments")]
public class LoanRepayment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int LoanTransactionId { get; set; }

    public decimal AmountRepaid { get; set; }

    public DateTime RepaymentDate { get; set; } = DateTime.Now;

    public string Notes { get; set; } = string.Empty;

    /// <summary>False until this record has been successfully pushed to Azure SQL.</summary>
    public bool IsSynced { get; set; } = false;
}

/// <summary>
/// Subsidy transaction
/// </summary>
[Table("SubsidyTransactions")]
public class SubsidyTransaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    public string TransactionType { get; set; } = "Subsidy";

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? UpdatedDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    /// <summary>False until this record has been successfully pushed to Azure SQL.</summary>
    public bool IsSynced { get; set; } = false;

    // Subsidy-specific fields
    public string SchemeName { get; set; } = string.Empty;

    public string SubsidyType { get; set; } = string.Empty; // Seeds, Fertilizer, Equipment, etc.

    public DateTime ReceivedDate { get; set; } = DateTime.Now;
}

/// <summary>
/// Miscellaneous transaction
/// </summary>
[Table("MiscellaneousTransactions")]
public class MiscellaneousTransaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    public string TransactionType { get; set; } = "Miscellaneous";

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? UpdatedDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    /// <summary>False until this record has been successfully pushed to Azure SQL.</summary>
    public bool IsSynced { get; set; } = false;

    // Misc-specific fields
    public string TransactionDirection { get; set; } = "Outgoing"; // Incoming or Outgoing

    public string MiscCategory { get; set; } = string.Empty; // Equipment, Maintenance, Transport, etc.
}

/// <summary>
/// Financial summary and analytics
/// </summary>
public class FinancialSummary
{
    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal TotalSubsidy { get; set; }

    public decimal TotalLoanTaken { get; set; }

    public decimal TotalLoanRepaid { get; set; }

    public decimal OutstandingLoan => TotalLoanTaken - TotalLoanRepaid;

    public decimal NetProfit => TotalIncome + TotalSubsidy - TotalExpense;

    public decimal CropsSold { get; set; }

    public decimal AveragePricePerUnit { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }
}
