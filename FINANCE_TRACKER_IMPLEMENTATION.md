# Finance Tracker Implementation Guide

## Overview

The Finance Tracker is a comprehensive financial management system for farmers that helps them:
- Track income from crop sales
- Monitor expenses for farming operations
- Manage loans and subsidies
- Calculate profit/loss
- Generate financial reports
- Make informed financial decisions

## Problem Statement

Farmers face significant challenges:
- **30-40% loss** in crop sales due to lack of market price awareness
- **No digital record-keeping** making it hard to calculate actual profit/loss
- **Difficulty accessing loans** without proper financial documentation
- **Exploitation by middlemen** due to information asymmetry

## Solution Architecture

### Database Schema

The Finance Tracker uses SQLite with the following main tables:

#### 1. **FinanceTransactions** (Base Table)
- Stores all financial transactions
- Fields: Id, UserId, TransactionType, Category, Description, Amount, TransactionDate, CreatedDate, UpdatedDate, Notes, IsDeleted

#### 2. **IncomeTransactions** (Income Records)
- Extends FinanceTransactions for income/sales
- Additional Fields: CropName, Quantity, QuantityUnit, PricePerUnit, BuyerName, TotalAmount (calculated)

#### 3. **ExpenseTransactions** (Expense Records)
- Tracks farm expenses
- Categories: Seeds, Fertilizer, Water, Labor, Equipment, Pesticides, Transport, Others
- Additional Fields: ExpenseCategory, ExpenseName

#### 4. **LoanTransactions** (Loan Management)
- Manages agricultural loans
- Types: Bank, Cooperative, Private, Government
- Additional Fields: LoanType, LenderName, InterestRate, DueDate, IsRepaid, RemainingAmount

#### 5. **LoanRepayments** (Loan Repayment Tracking)
- Tracks individual loan repayments
- Fields: Id, LoanTransactionId, AmountRepaid, RepaymentDate, Notes

#### 6. **SubsidyTransactions** (Government Subsidies)
- Records subsidies and incentives received
- Types: Seeds, Fertilizer, Equipment, Direct Payment
- Additional Fields: SchemeName, SubsidyType, ReceivedDate

#### 7. **MiscellaneousTransactions** (Other Transactions)
- For transactions not fitting other categories
- Direction: Incoming/Outgoing
- Examples: Equipment maintenance, equipment sales, transport charges

## Service Architecture

### IFinanceService Interface

```csharp
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
```

### FinanceService Implementation

The `FinanceService` class implements `IFinanceService` and provides:
- SQLite database operations
- CRUD operations for all transaction types
- Date range filtering
- Analytics and summary calculations
- User-specific data isolation

## UI Components

### Finance Page (`FinancePage.xaml`)

**Main Features:**
1. **Finance Summary Cards**
   - Total Expenses (This Month)
   - Total Income (This Month)
   - Net Profit (This Month)

2. **Quick Actions Grid**
   - Add Income (➕)
   - Add Expense (🧾)
   - Add Loan (🏦)
   - Add Subsidy (🎁)
   - Voice Entry (🎤)
   - Miscellaneous (📦)

3. **This Month Overview**
   - Donut chart showing expense/income/profit breakdown
   - Quick links to Reports and History

### Additional UI Pages
- **FinanceHistoryPage**: Displays a scrollable, filterable list of all past transactions.
- **FinanceReportsPage**: Shows visual breakdowns (Donut chart for expenses, Progress bars for income).
- **ProfitSummaryPage**: Displays a 6-month profit trend line chart and current month overview.
- **AiInsightsPage**: Uses AI to generate actionable insights for cost optimization, profit improvement, and loan readiness.
- **AddMiscellaneousPage**: Provides a quick-tap grid for entering non-standard transactions.

### FinanceViewModel

Handles:
- Loading financial summaries
- Loading transaction lists
- Adding/updating/deleting transactions
- Period-based filtering
- Error handling and user notifications
- Navigation to all sub-pages (History, Reports, Profit Summary, AI Insights)

### Additional ViewModels
- **AddMiscellaneousViewModel**: Logic for entering miscellaneous items.
- **FinanceReportsViewModel**: Formats data for Microcharts visualization.
- **ProfitSummaryViewModel**: Calculates 6-month historical data for line charts.
- **AiInsightsViewModel**: Integrates with `IAIChatService` to generate dynamic JSON insights.
- **FinanceHistoryViewModel**: Logic for loading, filtering, and clearing transaction history.

## AI Integration (Azure OpenAI)

The Finance Tracker features a dynamic "AI Insights" module that gives users highly personalized, actionable farming and financial advice based on their actual numbers. 

### 1. The AI Engine (`AIChatService.cs`)
Powered by **Azure OpenAI** using the official `Azure.AI.OpenAI` NuGet package. The service checks if the user is online and if Azure AI configuration is properly set. It securely transmits prompts to Azure's GPT models.

### 2. Contextual Prompting (`AiInsightsViewModel.cs`)
When the insights page loads, the app calculates the user's financial summary for the current month:
- Total Income
- Total Expense
- Net Profit
- Outstanding Loan Balance

These metrics are injected directly into a prompt sent to Azure OpenAI:
> *"Analyze the following farm financial data for the current month... Provide exactly 3 concise, actionable insights (1-2 sentences each). Return the result as a valid JSON object..."*

### 3. Structured JSON Response
To map seamlessly to the UI, the AI is instructed to return its response as a raw JSON object with exactly three keys: `"CostOptimization"`, `"ProfitImprovement"`, and `"LoanReadiness"`. The app parses this JSON and binds the text directly to three separate summary cards.

### 4. Hackathon Safety Fallback
To ensure a flawless live demo even if the internet drops or the Azure API times out, `AiInsightsViewModel.cs` wraps the API call in a `try-catch` block. If an error occurs, it falls back to a generic set of sensible farming insights, ensuring the UI remains robust and populated.

## Usage Examples

### Adding Income

```csharp
var income = new IncomeTransaction
{
    CropName = "Rice",
    Quantity = 10,
    QuantityUnit = "Quintal",
    PricePerUnit = 5000,
    BuyerName = "Local Trader",
    TransactionDate = DateTime.Now,
    Notes = "Good quality rice"
};

await _financeService.AddIncomeAsync(income);
```

### Adding Expense

```csharp
var expense = new ExpenseTransaction
{
    ExpenseCategory = "Seeds",
    ExpenseName = "Rice Seeds",
    Amount = 2000,
    TransactionDate = DateTime.Now,
    Notes = "Purchased from local supplier"
};

await _financeService.AddExpenseAsync(expense);
```

### Adding Loan

```csharp
var loan = new LoanTransaction
{
    LoanType = "Bank",
    LenderName = "State Bank of India",
    Amount = 50000,
    InterestRate = 8.5M,
    DueDate = DateTime.Now.AddYears(1),
    Notes = "Agricultural loan for farming"
};

await _financeService.AddLoanAsync(loan);
```

### Getting Financial Summary

```csharp
var startDate = new DateTime(2024, 1, 1);
var endDate = new DateTime(2024, 1, 31);

var summary = await _financeService.GetFinancialSummaryAsync(startDate, endDate);

Console.WriteLine($"Total Income: ₹{summary.TotalIncome}");
Console.WriteLine($"Total Expense: ₹{summary.TotalExpense}");
Console.WriteLine($"Net Profit: ₹{summary.NetProfit}");
Console.WriteLine($"Outstanding Loan: ₹{summary.OutstandingLoan}");
```

## Data Models

### FinancialSummary

```csharp
public class FinancialSummary
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalSubsidy { get; set; }
    public decimal TotalLoanTaken { get; set; }
    public decimal TotalLoanRepaid { get; set; }
    public decimal OutstandingLoan { get; set; }
    public decimal NetProfit { get; set; }
    public decimal CropsSold { get; set; }
    public decimal AveragePricePerUnit { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
```

## Features Implemented

✅ **Core Features:**
- Income/Crop Sales tracking
- Expense tracking by category
- Loan management with repayment tracking
- Subsidy record keeping
- Miscellaneous transaction tracking
- Monthly financial summary
- Period-based analytics
- User-specific data isolation
- Soft delete support
- Timestamps for audit trail

✅ **Advanced Features:**
- Profit trend analysis (6-month line charts)
- Expense and Income breakdown charts (Microcharts)
- AI-powered financial insights (Cost optimization, Profit improvement, Loan readiness)
- Comprehensive transaction history filtering

## Future Enhancements

### Phase 2 - Advanced Analytics
- [ ] Crop profitability analysis
- [ ] Market price comparison tool

### Phase 3 - Loan & Credit
- [ ] Loan eligibility calculator
- [ ] Bank recommendation engine
- [ ] Loan repayment schedule generator
- [ ] Interest calculation with EMI breakdown
- [ ] Credit score estimation

### Phase 4 - Market Integration
- [ ] Real-time market prices for crops
- [ ] Historical price trends
- [ ] Optimal selling time recommendations
- [ ] Market alerts for price fluctuations

### Phase 5 - Cloud Sync & Backup
- [ ] Cloud backup of financial records
- [ ] Multi-device sync
- [ ] Export to Excel/PDF
- [ ] Integration with government subsidy portals

### Phase 6 - Voice & AI
- [ ] Voice-based transaction entry
- [ ] Natural language processing for expense categories
- [ ] Expense prediction based on historical data

## Installation & Setup

1. **Add Finance Service to MauiProgram.cs:**
```csharp
builder.Services.AddSingleton<IFinanceService, FinanceService>();
builder.Services.AddSingleton<FinanceViewModel>();
builder.Services.AddSingleton<FinancePage>();
```

2. **Update AppShell.xaml:**
```xml
<ShellContent
    Title="Finance"
    Icon="wallet.png"
    ContentTemplate="{DataTemplate views:FinancePage}"
    Route="finance" />
```

3. **Initialize Database:**
- The `FinanceService` automatically creates tables on first use
- SQL script provided in `Scripts/finance_database_setup.sql` for reference

## API Testing

### Get User Income for Date Range

**Endpoint:** `GET /api/finance/income`

**Parameters:**
- `userId`: User identifier
- `startDate`: Start date (YYYY-MM-DD)
- `endDate`: End date (YYYY-MM-DD)

**Response:**
```json
{
    "success": true,
    "data": [
        {
            "id": 1,
            "cropName": "Rice",
            "quantity": 10,
            "pricePerUnit": 5000,
            "totalAmount": 50000,
            "buyerName": "Local Trader",
            "transactionDate": "2024-01-15"
        }
    ]
}
```

### Get Financial Summary

**Endpoint:** `GET /api/finance/summary`

**Parameters:**
- `userId`: User identifier
- `startDate`: Start date (YYYY-MM-DD)
- `endDate`: End date (YYYY-MM-DD)

**Response:**
```json
{
    "success": true,
    "data": {
        "totalIncome": 150000,
        "totalExpense": 45000,
        "totalSubsidy": 6000,
        "netProfit": 111000,
        "outstandingLoan": 35000
    }
}
```

## Best Practices

1. **Data Validation:**
   - Always validate user input before saving
   - Check for negative amounts
   - Verify date ranges are valid

2. **Error Handling:**
   - Wrap database operations in try-catch
   - Show user-friendly error messages
   - Log errors for debugging

3. **Performance:**
   - Use date range filtering to avoid loading excessive data
   - Create database indexes for frequently queried columns
   - Cache summary calculations

4. **Security:**
   - Validate user identity before accessing financial data
   - Use parameterized queries to prevent SQL injection
   - Implement audit logging for all financial transactions

5. **Data Integrity:**
   - Use transactions for related updates (e.g., loan + repayment)
   - Validate loan remaining amount calculations
   - Prevent double-processing of entries

## Troubleshooting

**Issue:** Database not created
- **Solution:** Ensure `FinanceService` is registered in `MauiProgram.cs` and `InitializeAsync()` is called

**Issue:** No transactions appearing
- **Solution:** Check that `UserId` is properly set; verify transactions are not marked as deleted

**Issue:** Incorrect profit calculation
- **Solution:** Verify all transaction amounts and types; check date range filters

## Support & Maintenance

For issues or feature requests, contact the development team with:
- Error message and logs
- Steps to reproduce
- Expected vs actual behavior
- User details (if applicable)

## References

- [MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [Community MVVM Toolkit](https://github.com/CommunityToolkit/dotnet)
