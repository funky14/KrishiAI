-- Finance Tracker Database Schema
-- This script creates the database tables and stored procedures for the Finance Tracker feature
-- Syntax: Microsoft SQL Server (T-SQL)

-- DROP OLD TABLES IF THEY EXIST (For Migration/Hackathon purposes)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanRepayments') DROP TABLE LoanRepayments;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IncomeTransactions') DROP TABLE IncomeTransactions;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ExpenseTransactions') DROP TABLE ExpenseTransactions;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanTransactions') DROP TABLE LoanTransactions;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SubsidyTransactions') DROP TABLE SubsidyTransactions;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MiscellaneousTransactions') DROP TABLE MiscellaneousTransactions;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FinanceTransactions') DROP TABLE FinanceTransactions;

-- ============================================
-- 1. UNIFIED FINANCE TRANSACTIONS TABLE
-- ============================================
CREATE TABLE FinanceTransactions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          NVARCHAR(450) NOT NULL, -- Assuming UserId is string/GUID based on codebase
    TransactionType NVARCHAR(50)  NOT NULL,
    Category        NVARCHAR(100),
    Description     NVARCHAR(MAX),
    Amount          DECIMAL(18, 2) NOT NULL,
    TransactionDate DATETIME2     NOT NULL,
    CreatedDate     DATETIME2     NOT NULL DEFAULT GETDATE(),
    UpdatedDate     DATETIME2,
    Notes           NVARCHAR(MAX),
    IsDeleted       BIT           NOT NULL DEFAULT 0,

    -- Income specific
    CropName        NVARCHAR(200),
    Quantity        DECIMAL(10, 2),
    QuantityUnit    NVARCHAR(50),
    PricePerUnit    DECIMAL(10, 2),
    BuyerName       NVARCHAR(200),

    -- Expense specific
    ExpenseCategory NVARCHAR(100),
    ExpenseName     NVARCHAR(200),

    -- Loan specific
    LoanType        NVARCHAR(100),
    LenderName      NVARCHAR(200),
    InterestRate    DECIMAL(5, 2),
    DueDate         DATETIME2,
    IsRepaid        BIT,
    RemainingAmount DECIMAL(18, 2),

    -- Subsidy specific
    SchemeName      NVARCHAR(200),
    SubsidyType     NVARCHAR(100),
    ReceivedDate    DATETIME2,

    -- Misc specific
    TransactionDirection NVARCHAR(20),
    MiscCategory         NVARCHAR(100)
);

CREATE INDEX idx_FinanceTransactions_UserId          ON FinanceTransactions(UserId);
CREATE INDEX idx_FinanceTransactions_TransactionType ON FinanceTransactions(TransactionType);
CREATE INDEX idx_FinanceTransactions_TransactionDate ON FinanceTransactions(TransactionDate);
CREATE INDEX idx_FinanceTransactions_IsDeleted       ON FinanceTransactions(IsDeleted);
GO

-- ============================================
-- 2. LOAN REPAYMENT TABLE
-- ============================================
CREATE TABLE LoanRepayments (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    LoanTransactionId INT            NOT NULL,
    AmountRepaid      DECIMAL(18, 2) NOT NULL,
    RepaymentDate     DATETIME2      NOT NULL DEFAULT GETDATE(),
    Notes             NVARCHAR(MAX),
    CONSTRAINT FK_LoanRepayments_FinanceTransactions FOREIGN KEY (LoanTransactionId) REFERENCES FinanceTransactions(Id)
);

CREATE INDEX idx_LoanRepayments_LoanTransactionId ON LoanRepayments(LoanTransactionId);
CREATE INDEX idx_LoanRepayments_RepaymentDate     ON LoanRepayments(RepaymentDate);
GO

-- ============================================
-- ANALYTICS VIEWS
-- ============================================

-- View: Monthly Financial Summary
CREATE OR ALTER VIEW vw_MonthlySummary AS
SELECT
    ft.UserId,
    MONTH(ft.TransactionDate) AS [Month],
    YEAR(ft.TransactionDate)  AS [Year],
    ft.TransactionType,
    SUM(ft.Amount)            AS TotalAmount,
    COUNT(*)                  AS TransactionCount
FROM FinanceTransactions ft
WHERE ft.IsDeleted = 0
GROUP BY ft.UserId, MONTH(ft.TransactionDate), YEAR(ft.TransactionDate), ft.TransactionType;
GO

-- View: Expense Breakdown by Category
CREATE OR ALTER VIEW vw_ExpenseBreakdown AS
SELECT
    ft.UserId,
    ft.ExpenseCategory,
    COUNT(*)        AS [Count],
    SUM(ft.Amount)  AS TotalAmount,
    AVG(ft.Amount)  AS AverageAmount
FROM FinanceTransactions ft
WHERE ft.IsDeleted = 0 AND ft.TransactionType = 'Expense'
GROUP BY ft.UserId, ft.ExpenseCategory;
GO

-- View: Income Summary by Crop
CREATE OR ALTER VIEW vw_IncomeByCrop AS
SELECT
    ft.UserId,
    ft.CropName,
    COUNT(*)               AS SalesCount,
    SUM(ft.Quantity)       AS TotalQuantity,
    SUM(ft.Amount)         AS TotalIncome,
    AVG(ft.PricePerUnit)   AS AveragePricePerUnit
FROM FinanceTransactions ft
WHERE ft.IsDeleted = 0 AND ft.TransactionType = 'Income'
GROUP BY ft.UserId, ft.CropName;
GO

-- View: Outstanding Loans
CREATE OR ALTER VIEW vw_OutstandingLoans AS
SELECT
    ft.UserId,
    ft.LenderName,
    ft.LoanType,
    ft.Amount,
    ft.RemainingAmount,
    ft.InterestRate,
    ft.DueDate,
    (ft.Amount - ft.RemainingAmount) AS AmountRepaid
FROM FinanceTransactions ft
WHERE ft.IsDeleted = 0 AND ft.TransactionType = 'Loan' AND ft.IsRepaid = 0;
GO

-- ============================================
-- UTILITY STORED PROCEDURES
-- ============================================

-- SP: Calculate Net Profit for User
CREATE OR ALTER PROCEDURE sp_GetNetProfit
    @UserId    NVARCHAR(450),
    @StartDate DATETIME2,
    @EndDate   DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        @UserId AS UserId,
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Income'  THEN ft.Amount ELSE 0 END), 0) AS TotalIncome,
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Expense' THEN ft.Amount ELSE 0 END), 0) AS TotalExpense,
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Subsidy' THEN ft.Amount ELSE 0 END), 0) AS TotalSubsidy,
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Income'  THEN ft.Amount ELSE 0 END), 0)
        + COALESCE(SUM(CASE WHEN ft.TransactionType = 'Subsidy' THEN ft.Amount ELSE 0 END), 0)
        - COALESCE(SUM(CASE WHEN ft.TransactionType = 'Expense' THEN ft.Amount ELSE 0 END), 0) AS NetProfit
    FROM FinanceTransactions ft
    WHERE ft.UserId = @UserId
      AND ft.TransactionDate BETWEEN @StartDate AND @EndDate
      AND ft.IsDeleted = 0;
END
GO
