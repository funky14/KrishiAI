using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

/// <summary>
/// Azure SQL Server backend for Finance data.
/// Used when the device has internet connectivity.
/// </summary>
public interface IFinanceAzureSqlService
{
    /// <summary>True when a valid Azure SQL connection string is available.</summary>
    bool IsConfigured { get; }

    // Income
    Task<int> AddIncomeAsync(FinanceTransaction income);
    Task<bool> UpdateIncomeAsync(FinanceTransaction income);
    Task<bool> DeleteIncomeAsync(int incomeId);
    Task<List<FinanceTransaction>> GetAllIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

    // Expense
    Task<int> AddExpenseAsync(FinanceTransaction expense);
    Task<bool> UpdateExpenseAsync(FinanceTransaction expense);
    Task<bool> DeleteExpenseAsync(int expenseId);
    Task<List<FinanceTransaction>> GetAllExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

    // Loan
    Task<int> AddLoanAsync(FinanceTransaction loan);
    Task<bool> UpdateLoanAsync(FinanceTransaction loan);
    Task<bool> DeleteLoanAsync(int loanId);
    Task<List<FinanceTransaction>> GetAllLoansAsync(string userId);
    Task<int> AddLoanRepaymentAsync(LoanRepayment repayment);
    Task<List<LoanRepayment>> GetLoanRepaymentsAsync(int loanId);

    // Subsidy
    Task<int> AddSubsidyAsync(FinanceTransaction subsidy);
    Task<bool> UpdateSubsidyAsync(FinanceTransaction subsidy);
    Task<bool> DeleteSubsidyAsync(int subsidyId);
    Task<List<FinanceTransaction>> GetAllSubsidiesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

    // Miscellaneous
    Task<int> AddMiscTransactionAsync(FinanceTransaction misc);
    Task<bool> UpdateMiscTransactionAsync(FinanceTransaction misc);
    Task<bool> DeleteMiscTransactionAsync(int miscId);
    Task<List<FinanceTransaction>> GetAllMiscTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

    // Summary
    Task<FinancialSummary> GetFinancialSummaryAsync(string userId, DateTime startDate, DateTime endDate);
    Task<List<FinanceTransaction>> GetAllTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
}
