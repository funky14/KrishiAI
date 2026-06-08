# Finance Tracker - Quick Reference Guide

## 🚀 Quick Start (5 Minutes)

### 1. Inject the Service
```csharp
public class MyPage : ContentPage
{
    private readonly IFinanceService _financeService;
    
    public MyPage(IFinanceService financeService)
    {
        _financeService = financeService;
    }
}
```

### 2. Add Income
```csharp
var income = new IncomeTransaction
{
    CropName = "Rice",
    Quantity = 10,
    QuantityUnit = "Quintal",
    PricePerUnit = 5000,
    BuyerName = "Buyer Name",
    TransactionDate = DateTime.Now
};
int id = await _financeService.AddIncomeAsync(income);
```

### 3. Add Expense
```csharp
var expense = new ExpenseTransaction
{
    ExpenseCategory = "Seeds",
    ExpenseName = "Rice Seeds",
    Amount = 2000,
    TransactionDate = DateTime.Now
};
int id = await _financeService.AddExpenseAsync(expense);
```

### 4. Add Loan
```csharp
var loan = new LoanTransaction
{
    LoanType = "Bank",
    LenderName = "SBI",
    Amount = 50000,
    InterestRate = 8.5M,
    DueDate = DateTime.Now.AddYears(1)
};
int id = await _financeService.AddLoanAsync(loan);
```

### 5. Get Summary
```csharp
var summary = await _financeService
    .GetFinancialSummaryAsync(startDate, endDate);
```

## 📋 Transaction Types

### IncomeTransaction
```
CropName (string) - e.g., "Rice", "Wheat"
Quantity (decimal) - e.g., 10
QuantityUnit (string) - Default: "Quintal"
PricePerUnit (decimal) - e.g., 5000
BuyerName (string) - e.g., "Local Trader"
Amount (auto-calculated) - Quantity × PricePerUnit
```

### ExpenseTransaction
```
ExpenseCategory (string) - Seeds, Fertilizer, Labor, Equipment, etc.
ExpenseName (string) - Description
Amount (decimal) - Cost
```

### LoanTransaction
```
LoanType (string) - Bank, Cooperative, Private, Government
LenderName (string) - Institution/Person name
Amount (decimal) - Loan amount
InterestRate (decimal) - Annual %
DueDate (DateTime) - Repayment due date
RemainingAmount (decimal) - Auto-calculated
```

### SubsidyTransaction
```
SchemeName (string) - e.g., "PM-Kisan"
SubsidyType (string) - Seeds, Fertilizer, Equipment
Amount (decimal) - Subsidy amount
ReceivedDate (DateTime)
```

### MiscellaneousTransaction
```
MiscCategory (string) - Equipment maintenance, Transport, etc.
TransactionDirection (string) - "Incoming" or "Outgoing"
Amount (decimal)
```

## 🔍 Common Queries

### Get Current Month Summary
```csharp
var today = DateTime.Now;
var startDate = new DateTime(today.Year, today.Month, 1);
var endDate = today;
var summary = await _financeService
    .GetFinancialSummaryAsync(startDate, endDate);
```

### Get All Income This Year
```csharp
var startDate = new DateTime(DateTime.Now.Year, 1, 1);
var endDate = DateTime.Now;
var income = await _financeService
    .GetAllIncomeAsync(startDate, endDate);
```

### Get Expenses by Category
```csharp
var expensesByCategory = await _financeService
    .GetExpensesByCategoryAsync(startDate, endDate);
foreach (var cat in expensesByCategory)
{
    Console.WriteLine($"{cat.Key}: ₹{cat.Value}");
}
```

### Get All Outstanding Loans
```csharp
var loans = await _financeService.GetAllLoansAsync();
var outstanding = loans
    .Where(l => !l.IsRepaid)
    .ToList();
```

### Record Loan Repayment
```csharp
var repayment = new LoanRepayment
{
    LoanTransactionId = loanId,
    AmountRepaid = 10000,
    RepaymentDate = DateTime.Now,
    Notes = "6-month EMI"
};
await _financeService.AddLoanRepaymentAsync(repayment);
```

## 📊 Financial Summary Fields

```csharp
FinancialSummary
├── TotalIncome (decimal)
├── TotalExpense (decimal)
├── TotalSubsidy (decimal)
├── TotalLoanTaken (decimal)
├── TotalLoanRepaid (decimal)
├── OutstandingLoan (calculated)
├── NetProfit (calculated)
├── CropsSold (decimal)
├── AveragePricePerUnit (decimal)
├── PeriodStart (DateTime)
└── PeriodEnd (DateTime)
```

## 🧮 Key Calculations

```
Total Amount (Income) = Quantity × PricePerUnit
Outstanding Loan = TotalLoanTaken - TotalLoanRepaid
Net Profit = TotalIncome + TotalSubsidy - TotalExpense
```

## 🎯 Expense Categories

- Seeds
- Fertilizer
- Water/Irrigation
- Labor/Wages
- Equipment
- Pesticides
- Transportation
- Maintenance
- Others

## 🏦 Loan Types

- Bank
- Cooperative
- Private
- Government

## 💰 Subsidy Types

- Seeds
- Fertilizer
- Equipment
- Direct Payment
- Others

## 🎤 Voice Entry Example (Future)

```csharp
// "Add income: Rice, 10 quintals, 5000 per quintal"
// System will parse and create IncomeTransaction

// "Add expense: 2000 rupees for seeds"
// System will parse and create ExpenseTransaction
```

## ✅ Validation Rules

- Amount must be > 0
- StartDate must be <= EndDate
- Quantity must be > 0
- PricePerUnit must be > 0
- LoanRepayment cannot exceed Outstanding
- UserId must not be empty
- CropName must not be empty
- ExpenseCategory must not be empty

## 🐛 Debugging Tips

### Check if Data Exists
```csharp
var transactions = await _financeService
    .GetAllTransactionsAsync();
Console.WriteLine($"Total transactions: {transactions.Count}");
```

### Verify Calculations
```csharp
var summary = await _financeService
    .GetFinancialSummaryAsync(start, end);
Console.WriteLine($"Income: {summary.TotalIncome}");
Console.WriteLine($"Expense: {summary.TotalExpense}");
Console.WriteLine($"Profit: {summary.NetProfit}");
```

### Check Date Filters
```csharp
var allTransactions = await _financeService
    .GetAllTransactionsAsync();
var filtered = await _financeService
    .GetAllTransactionsAsync(start, end);
Console.WriteLine($"Total: {allTransactions.Count}, Filtered: {filtered.Count}");
```

## 📱 UI Binding Example

```xml
<Label Text="{Binding FinancialSummary.TotalIncome, StringFormat='₹{0:F0}'}" />
<Label Text="{Binding TotalExpense, StringFormat='₹{0:F0}'}" />
<Label Text="{Binding NetProfit, StringFormat='₹{0:F0}'}" />
<ActivityIndicator IsRunning="{Binding IsLoading}" />
```

## 🔗 Related Files

- Models: `Models/FinanceTransaction.cs`
- Service: `Services/FinanceService.cs`
- Interface: `Services/IFinanceService.cs`
- ViewModel: `ViewModels/FinanceViewModel.cs`
- Page: `Views/FinancePage.xaml`
- Database: `Scripts/finance_database_setup.sql`
- Controller: `Controllers/FinanceController.cs`

## 📖 Documentation Links

- Full Implementation: `FINANCE_TRACKER_IMPLEMENTATION.md`
- Setup Guide: `FINANCE_TRACKER_SETUP.md`
- Summary: `FINANCE_TRACKER_SUMMARY.md`

## 🆘 Quick Troubleshooting

| Issue | Solution |
|-------|----------|
| No data showing | Check UserId, verify date range |
| Profit incorrect | Verify all amounts, check formula |
| Loan not updating | Ensure loan exists before repayment |
| Service not found | Check MauiProgram.cs registration |
| DB not creating | Check app data directory permissions |

## 📞 Contact

For issues: Review FINANCE_TRACKER_SETUP.md troubleshooting section

---

**Last Updated**: 2024-01-10
**Quick Reference Version**: 1.0
