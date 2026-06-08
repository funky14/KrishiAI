using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface IFinanceService
{
    // Income operations
    Task<int> AddIncomeAsync(IncomeTransaction income);
    Task<bool> UpdateIncomeAsync(IncomeTransaction income);
    Task<bool> DeleteIncomeAsync(int incomeId);
    Task<IncomeTransaction?> GetIncomeByIdAsync(int incomeId);
    Task<List<IncomeTransaction>> GetAllIncomeAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Expense operations
    Task<int> AddExpenseAsync(ExpenseTransaction expense);
    Task<bool> UpdateExpenseAsync(ExpenseTransaction expense);
    Task<bool> DeleteExpenseAsync(int expenseId);
    Task<ExpenseTransaction?> GetExpenseByIdAsync(int expenseId);
    Task<List<ExpenseTransaction>> GetAllExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Loan operations
    Task<int> AddLoanAsync(LoanTransaction loan);
    Task<bool> UpdateLoanAsync(LoanTransaction loan);
    Task<bool> DeleteLoanAsync(int loanId);
    Task<LoanTransaction?> GetLoanByIdAsync(int loanId);
    Task<List<LoanTransaction>> GetAllLoansAsync();
    Task<int> AddLoanRepaymentAsync(LoanRepayment repayment);
    Task<List<LoanRepayment>> GetLoanRepaymentsAsync(int loanId);

    // Subsidy operations
    Task<int> AddSubsidyAsync(SubsidyTransaction subsidy);
    Task<bool> UpdateSubsidyAsync(SubsidyTransaction subsidy);
    Task<bool> DeleteSubsidyAsync(int subsidyId);
    Task<SubsidyTransaction?> GetSubsidyByIdAsync(int subsidyId);
    Task<List<SubsidyTransaction>> GetAllSubsidiesAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Miscellaneous operations
    Task<int> AddMiscTransactionAsync(MiscellaneousTransaction misc);
    Task<bool> UpdateMiscTransactionAsync(MiscellaneousTransaction misc);
    Task<bool> DeleteMiscTransactionAsync(int miscId);
    Task<MiscellaneousTransaction?> GetMiscTransactionByIdAsync(int miscId);
    Task<List<MiscellaneousTransaction>> GetAllMiscTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Summary and Analytics
    Task<FinancialSummary> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate);
    Task<List<FinanceTransaction>> GetAllTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<FinanceTransaction>> GetTransactionsByCategoryAsync(string category, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
}
