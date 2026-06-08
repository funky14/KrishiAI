using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

class Program
{
    static async Task Main(string[] args)
    {
        string cs = "Data Source=azuredemodb.database.windows.net;Initial Catalog=free-sql-db-4227077;Persist Security Info=True;User ID=sqladmin;Password=Amazon@810649;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0";

        using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        // Clear existing hackathon_demo_user data to prevent duplicates
        var cmdDel = conn.CreateCommand();
        cmdDel.CommandText = "DELETE FROM LoanRepayments WHERE LoanTransactionId IN (SELECT Id FROM FinanceTransactions WHERE UserId = 'hackathon_demo_user'); DELETE FROM FinanceTransactions WHERE UserId = 'hackathon_demo_user';";
        await cmdDel.ExecuteNonQueryAsync();

        string userId = "hackathon_demo_user";
        
        // Use a date within the current month so it shows up in "This Month"
        var date = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss");

        var queries = new List<string>
        {
            // EXPENSES (Total: 12500)
            // Fertilizers: 4500
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, ExpenseCategory, ExpenseName, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Expense', 'Fertilizer', 'Fertilizer', 'DAP & Urea', 4500, '{date}', GETDATE(), 0)",
            // Labor: 3000
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, ExpenseCategory, ExpenseName, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Expense', 'Labor', 'Labor', 'Harvesting Labor', 3000, '{date}', GETDATE(), 0)",
            // Seeds: 2000
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, ExpenseCategory, ExpenseName, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Expense', 'Seeds', 'Seeds', 'Wheat Seeds', 2000, '{date}', GETDATE(), 0)",
            // Irrigation: 1500
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, ExpenseCategory, ExpenseName, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Expense', 'Irrigation', 'Irrigation', 'Water Pump Fuel', 1500, '{date}', GETDATE(), 0)",
            // Others: 1500
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, ExpenseCategory, ExpenseName, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Expense', 'Others', 'Others', 'Miscellaneous', 1500, '{date}', GETDATE(), 0)",

            // INCOMES (Total: 28000)
            // Crop Sales: 18000
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, CropName, Quantity, QuantityUnit, PricePerUnit, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Income', 'Crop Sale', 'Wheat', 10, 'Quintals', 1800, 18000, '{date}', GETDATE(), 0)",
            // Subsidy: 6000
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, SchemeName, SubsidyType, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Subsidy', 'Government Subsidy', 'PM-KISAN', 'Direct Transfer', 6000, '{date}', GETDATE(), 0)",
            // Equipment Sale: 3000
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, CropName, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Income', 'Equipment Sale', 'Old Tractor Parts', 3000, '{date}', GETDATE(), 0)",
            // Other Income: 1000
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, CropName, Amount, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Income', 'Other Income', 'Consulting', 1000, '{date}', GETDATE(), 0)",

            // LOANS
            // Total Loan: 65000, Repaid: 15000 -> Outstanding: 50000
            $"INSERT INTO FinanceTransactions (UserId, TransactionType, Category, LoanType, LenderName, Amount, RemainingAmount, IsRepaid, TransactionDate, CreatedDate, IsDeleted) VALUES ('{userId}', 'Loan', 'Bank Loan', 'Kisan Credit Card', 'SBI', 65000, 50000, 0, '{date}', GETDATE(), 0)"
        };

        foreach (var q in queries)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = q;
            await cmd.ExecuteNonQueryAsync();
        }

        // Add Loan Repayment
        var cmdLoan = conn.CreateCommand();
        cmdLoan.CommandText = $"SELECT Id FROM FinanceTransactions WHERE UserId='{userId}' AND TransactionType='Loan' AND LenderName='SBI'";
        var loanId = await cmdLoan.ExecuteScalarAsync();

        if (loanId != null)
        {
            var cmdRepay = conn.CreateCommand();
            cmdRepay.CommandText = $"INSERT INTO LoanRepayments (LoanTransactionId, AmountRepaid, RepaymentDate) VALUES ({loanId}, 15000, '{date}')";
            await cmdRepay.ExecuteNonQueryAsync();
        }

        Console.WriteLine("Mock data inserted successfully!");
    }
}
