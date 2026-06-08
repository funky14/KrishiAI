# Finance Tracker - Setup & Testing Guide

## Quick Start

### 1. Enable Finance Tracker in Your Project

The Finance Tracker is already integrated into the KrishiAI MAUI app. It provides a complete financial management system for farmers.

### 2. Access Finance Tracker

Navigate to the **Finance Tab** in the app shell navigation bar.

## Features Overview

### 📊 Dashboard
- **Real-time Summary**: View total income, expenses, and profit at a glance
- **Quick Actions**: One-tap access to add transactions
- **Visual Analytics**: Chart breakdown of expenses and income

### 💰 Transaction Management

#### Add Income
Track crop sales with:
- Crop name
- Quantity (in quintals)
- Price per unit
- Buyer name
- Sale date
- Additional notes

#### Add Expense
Record farm expenses:
- Category (Seeds, Fertilizer, Labor, Equipment, etc.)
- Amount
- Date
- Notes

#### Add Loans
Manage agricultural loans:
- Loan type (Bank, Cooperative, Private, Government)
- Lender name
- Loan amount
- Interest rate
- Due date
- Repayment tracking

#### Add Subsidies
Record government assistance:
- Scheme name
- Subsidy type
- Amount received
- Receipt date

#### Miscellaneous Transactions
For other financial activities:
- Equipment maintenance
- Equipment sales
- Transport charges
- Other expenses

### 📈 Reports & Analytics
- Monthly financial summary
- Expense breakdown by category
- Income analysis by crop
- Profit/Loss calculation
- Outstanding loan tracking

### 🎤 Voice Entry (Coming Soon)
Add transactions by voice:
- "Add income: Rice, 10 quintals, 5000 per quintal"
- "Add expense: 2000 rupees for seeds"

## Database Schema

### Core Tables
1. **FinanceTransactions** - Base transaction table
2. **IncomeTransactions** - Crop sales records
3. **ExpenseTransactions** - Cost records
4. **LoanTransactions** - Loan records
5. **LoanRepayments** - Loan repayment tracking
6. **SubsidyTransactions** - Government assistance
7. **MiscellaneousTransactions** - Other transactions

### Indexes
- UserId (for user data isolation)
- TransactionDate (for date range queries)
- TransactionType (for filtering)

## API Reference

### Add Income Transaction
```csharp
var income = new IncomeTransaction
{
    CropName = "Wheat",
    Quantity = 15,
    QuantityUnit = "Quintal",
    PricePerUnit = 4500,
    BuyerName = "Agriculture Dept",
    TransactionDate = DateTime.Now,
    Notes = "Grade A wheat"
};
int incomeId = await _financeService.AddIncomeAsync(income);
```

### Add Expense Transaction
```csharp
var expense = new ExpenseTransaction
{
    ExpenseCategory = "Fertilizer",
    ExpenseName = "DAP Fertilizer 50kg",
    Amount = 3500,
    TransactionDate = DateTime.Now,
    Notes = "High quality fertilizer"
};
int expenseId = await _financeService.AddExpenseAsync(expense);
```

### Add Loan Transaction
```csharp
var loan = new LoanTransaction
{
    LoanType = "Bank",
    LenderName = "NABARD",
    Amount = 100000,
    InterestRate = 7.5M,
    DueDate = DateTime.Now.AddYears(2),
    Notes = "Kharif season loan"
};
int loanId = await _financeService.AddLoanAsync(loan);
```

### Record Loan Repayment
```csharp
var repayment = new LoanRepayment
{
    LoanTransactionId = loanId,
    AmountRepaid = 10000,
    RepaymentDate = DateTime.Now,
    Notes = "6-month installment"
};
int repaymentId = await _financeService.AddLoanRepaymentAsync(repayment);
```

### Get Financial Summary
```csharp
var startDate = new DateTime(2024, 1, 1);
var endDate = new DateTime(2024, 1, 31);
var summary = await _financeService.GetFinancialSummaryAsync(startDate, endDate);

Console.WriteLine($"Total Income: ₹{summary.TotalIncome}");
Console.WriteLine($"Total Expense: ₹{summary.TotalExpense}");
Console.WriteLine($"Net Profit: ₹{summary.NetProfit}");
Console.WriteLine($"Outstanding Loan: ₹{summary.OutstandingLoan}");
```

### Get All Transactions
```csharp
var transactions = await _financeService.GetAllTransactionsAsync(startDate, endDate);
foreach (var transaction in transactions)
{
    Console.WriteLine($"{transaction.TransactionType}: ₹{transaction.Amount}");
}
```

### Get Expenses by Category
```csharp
var expensesByCategory = await _financeService.GetExpensesByCategoryAsync(startDate, endDate);
foreach (var category in expensesByCategory)
{
    Console.WriteLine($"{category.Key}: ₹{category.Value}");
}
```

## Testing Guide

### Unit Test Example
```csharp
[TestClass]
public class FinanceServiceTests
{
    private IFinanceService _financeService;

    [TestInitialize]
    public void Setup()
    {
        _financeService = new FinanceService();
    }

    [TestMethod]
    public async Task AddIncome_ValidIncome_ReturnsPositiveId()
    {
        // Arrange
        var income = new IncomeTransaction
        {
            CropName = "Rice",
            Quantity = 10,
            PricePerUnit = 5000
        };

        // Act
        var id = await _financeService.AddIncomeAsync(income);

        // Assert
        Assert.IsTrue(id > 0);
    }

    [TestMethod]
    public async Task GetFinancialSummary_ValidDateRange_ReturnsSummary()
    {
        // Arrange
        var startDate = DateTime.Now.AddMonths(-1);
        var endDate = DateTime.Now;

        // Act
        var summary = await _financeService.GetFinancialSummaryAsync(startDate, endDate);

        // Assert
        Assert.IsNotNull(summary);
        Assert.IsTrue(summary.NetProfit >= 0);
    }
}
```

## Troubleshooting

### Issue: Database Not Initializing
**Solution:** 
- Ensure `IFinanceService` is registered in `MauiProgram.cs`
- Check that app has write permissions to app data directory
- Review debug logs for specific errors

### Issue: No Transactions Showing
**Solution:**
- Verify user ID is set correctly
- Check that transactions are not marked as deleted
- Confirm date range includes the transaction dates
- Review database through SQLite browser

### Issue: Profit Calculation Incorrect
**Solution:**
- Verify all expense amounts are entered correctly
- Check that transaction types are correct
- Ensure subsidies are counted as income, not expense
- Review formula: `NetProfit = TotalIncome + TotalSubsidy - TotalExpense`

### Issue: Loan Repayment Not Updating
**Solution:**
- Ensure loan exists before adding repayment
- Check that repayment amount doesn't exceed outstanding balance
- Verify loan ID in repayment record matches loan transaction ID

## Data Backup & Export

### Backup Local Database
```csharp
var dbPath = Path.Combine(FileSystem.AppDataDirectory, "krishiai.db3");
var backupPath = Path.Combine(FileSystem.AppDataDirectory, $"krishiai_backup_{DateTime.Now:yyyyMMdd}.db3");
File.Copy(dbPath, backupPath);
```

### Export Transactions to CSV
```csharp
var transactions = await _financeService.GetAllTransactionsAsync();
var csv = "Date,Type,Amount,Category\n";
foreach (var tx in transactions)
{
    csv += $"{tx.TransactionDate:yyyy-MM-dd},{tx.TransactionType},{tx.Amount},{tx.Category}\n";
}
File.WriteAllText(exportPath, csv);
```

## Performance Optimization Tips

1. **Use Date Ranges**: Always filter by date range when possible
2. **Index Frequently Queried Columns**: Already included for UserId, TransactionDate
3. **Batch Operations**: Group multiple operations in a transaction
4. **Cache Results**: Cache summary calculations for the current month
5. **Archive Old Data**: Move transactions older than 2 years to archive table

## Security Considerations

1. **User Isolation**: Always filter by UserId to ensure data isolation
2. **Input Validation**: Validate all user input before saving
3. **Soft Deletes**: Use IsDeleted flag instead of hard deletes for audit trail
4. **Audit Logging**: Log all financial transaction changes
5. **Encryption**: Consider encrypting sensitive data at rest

## Integration with Cloud Services

### Azure SQL Database (Future)
```csharp
var connectionString = "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=krishiai;...";
var options = SqlServerDbContextOptionsBuilder
    .UseSqlServer(connectionString);
```

### Firebase Firestore (Alternative)
```csharp
var settings = new FirestoreSettings { DisableFallback = true };
var db = new FirestoreDb("your-project-id", settings);
```

## Future Roadmap

- [ ] Cloud sync for multi-device access
- [ ] Real-time market price integration
- [ ] AI-powered expense categorization
- [ ] Loan eligibility calculator
- [ ] Government subsidy portal integration
- [ ] Mobile-responsive web dashboard
- [ ] Advanced analytics and forecasting
- [ ] PDF report generation

## Support

For issues or questions:
1. Check the FINANCE_TRACKER_IMPLEMENTATION.md for detailed documentation
2. Review test cases for usage examples
3. Check debug logs for error details
4. Contact development team with issue details

## References

- [MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [SQLite](https://www.sqlite.org/)
- [Community MVVM Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Microsoft Learn](https://learn.microsoft.com/)

---

**Last Updated**: 2024-01-10
**Version**: 1.0.0
**Status**: Active Development
