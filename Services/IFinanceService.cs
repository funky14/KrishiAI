using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface IFinanceService
{
    // Income operations
    Task<int> AddIncomeAsync(FinanceTransaction income);
    Task<bool> UpdateIncomeAsync(FinanceTransaction income);
    Task<bool> DeleteIncomeAsync(int incomeId);
    Task<FinanceTransaction?> GetIncomeByIdAsync(int incomeId);
    Task<List<FinanceTransaction>> GetAllIncomeAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Expense operations
    Task<int> AddExpenseAsync(FinanceTransaction expense);
    Task<bool> UpdateExpenseAsync(FinanceTransaction expense);
    Task<bool> DeleteExpenseAsync(int expenseId);
    Task<FinanceTransaction?> GetExpenseByIdAsync(int expenseId);
    Task<List<FinanceTransaction>> GetAllExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Loan operations
    Task<int> AddLoanAsync(FinanceTransaction loan);
    Task<bool> UpdateLoanAsync(FinanceTransaction loan);
    Task<bool> DeleteLoanAsync(int loanId);
    Task<FinanceTransaction?> GetLoanByIdAsync(int loanId);
    Task<List<FinanceTransaction>> GetAllLoansAsync();
    Task<int> AddLoanRepaymentAsync(LoanRepayment repayment);
    Task<List<LoanRepayment>> GetLoanRepaymentsAsync(int loanId);

    // Subsidy operations
    Task<int> AddSubsidyAsync(FinanceTransaction subsidy);
    Task<bool> UpdateSubsidyAsync(FinanceTransaction subsidy);
    Task<bool> DeleteSubsidyAsync(int subsidyId);
    Task<FinanceTransaction?> GetSubsidyByIdAsync(int subsidyId);
    Task<List<FinanceTransaction>> GetAllSubsidiesAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Miscellaneous operations
    Task<int> AddMiscTransactionAsync(FinanceTransaction misc);
    Task<bool> UpdateMiscTransactionAsync(FinanceTransaction misc);
    Task<bool> DeleteMiscTransactionAsync(int miscId);
    Task<FinanceTransaction?> GetMiscTransactionByIdAsync(int miscId);
    Task<List<FinanceTransaction>> GetAllMiscTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Summary and Analytics
    Task<FinancialSummary> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate);
    Task<List<FinanceTransaction>> GetAllTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<FinanceTransaction>> GetTransactionsByCategoryAsync(string category, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
}
