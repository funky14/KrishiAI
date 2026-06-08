-- Finance Tracker Database Schema
-- This script creates the database tables and stored procedures for the Finance Tracker feature

-- ============================================
-- 1. FINANCE TRANSACTIONS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS FinanceTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    TransactionType TEXT NOT NULL,
    Category TEXT NOT NULL,
    Description TEXT,
    Amount DECIMAL(18, 2) NOT NULL,
    TransactionDate DATETIME NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME,
    Notes TEXT,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX idx_FinanceTransactions_UserId ON FinanceTransactions(UserId);
CREATE INDEX idx_FinanceTransactions_TransactionType ON FinanceTransactions(TransactionType);
CREATE INDEX idx_FinanceTransactions_TransactionDate ON FinanceTransactions(TransactionDate);
CREATE INDEX idx_FinanceTransactions_IsDeleted ON FinanceTransactions(IsDeleted);

-- ============================================
-- 2. INCOME TRANSACTIONS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS IncomeTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    TransactionType TEXT DEFAULT 'Income',
    Category TEXT,
    Description TEXT,
    Amount DECIMAL(18, 2) NOT NULL,
    TransactionDate DATETIME NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME,
    Notes TEXT,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CropName TEXT NOT NULL,
    Quantity DECIMAL(10, 2) NOT NULL,
    QuantityUnit TEXT DEFAULT 'Quintal',
    PricePerUnit DECIMAL(10, 2) NOT NULL,
    BuyerName TEXT,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX idx_IncomeTransactions_UserId ON IncomeTransactions(UserId);
CREATE INDEX idx_IncomeTransactions_TransactionDate ON IncomeTransactions(TransactionDate);
CREATE INDEX idx_IncomeTransactions_CropName ON IncomeTransactions(CropName);

-- ============================================
-- 3. EXPENSE TRANSACTIONS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS ExpenseTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    TransactionType TEXT DEFAULT 'Expense',
    Category TEXT,
    Description TEXT,
    Amount DECIMAL(18, 2) NOT NULL,
    TransactionDate DATETIME NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME,
    Notes TEXT,
    IsDeleted BIT NOT NULL DEFAULT 0,
    ExpenseCategory TEXT NOT NULL,
    ExpenseName TEXT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX idx_ExpenseTransactions_UserId ON ExpenseTransactions(UserId);
CREATE INDEX idx_ExpenseTransactions_TransactionDate ON ExpenseTransactions(TransactionDate);
CREATE INDEX idx_ExpenseTransactions_ExpenseCategory ON ExpenseTransactions(ExpenseCategory);

-- ============================================
-- 4. LOAN TRANSACTIONS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS LoanTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    TransactionType TEXT DEFAULT 'Loan',
    Category TEXT,
    Description TEXT,
    Amount DECIMAL(18, 2) NOT NULL,
    TransactionDate DATETIME NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME,
    Notes TEXT,
    IsDeleted BIT NOT NULL DEFAULT 0,
    LoanType TEXT NOT NULL,
    LenderName TEXT NOT NULL,
    InterestRate DECIMAL(5, 2) DEFAULT 0,
    DueDate DATETIME,
    IsRepaid BIT DEFAULT 0,
    RemainingAmount DECIMAL(18, 2),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX idx_LoanTransactions_UserId ON LoanTransactions(UserId);
CREATE INDEX idx_LoanTransactions_TransactionDate ON LoanTransactions(TransactionDate);
CREATE INDEX idx_LoanTransactions_LoanType ON LoanTransactions(LoanType);
CREATE INDEX idx_LoanTransactions_IsRepaid ON LoanTransactions(IsRepaid);

-- ============================================
-- 5. LOAN REPAYMENT TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS LoanRepayments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    LoanTransactionId INTEGER NOT NULL,
    AmountRepaid DECIMAL(18, 2) NOT NULL,
    RepaymentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Notes TEXT,
    FOREIGN KEY (LoanTransactionId) REFERENCES LoanTransactions(Id)
);

CREATE INDEX idx_LoanRepayments_LoanTransactionId ON LoanRepayments(LoanTransactionId);
CREATE INDEX idx_LoanRepayments_RepaymentDate ON LoanRepayments(RepaymentDate);

-- ============================================
-- 6. SUBSIDY TRANSACTIONS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS SubsidyTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    TransactionType TEXT DEFAULT 'Subsidy',
    Category TEXT,
    Description TEXT,
    Amount DECIMAL(18, 2) NOT NULL,
    TransactionDate DATETIME NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME,
    Notes TEXT,
    IsDeleted BIT NOT NULL DEFAULT 0,
    SchemeName TEXT NOT NULL,
    SubsidyType TEXT NOT NULL,
    ReceivedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX idx_SubsidyTransactions_UserId ON SubsidyTransactions(UserId);
CREATE INDEX idx_SubsidyTransactions_TransactionDate ON SubsidyTransactions(TransactionDate);
CREATE INDEX idx_SubsidyTransactions_SchemeName ON SubsidyTransactions(SchemeName);

-- ============================================
-- 7. MISCELLANEOUS TRANSACTIONS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS MiscellaneousTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    TransactionType TEXT DEFAULT 'Miscellaneous',
    Category TEXT,
    Description TEXT,
    Amount DECIMAL(18, 2) NOT NULL,
    TransactionDate DATETIME NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME,
    Notes TEXT,
    IsDeleted BIT NOT NULL DEFAULT 0,
    TransactionDirection TEXT DEFAULT 'Outgoing',
    MiscCategory TEXT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX idx_MiscellaneousTransactions_UserId ON MiscellaneousTransactions(UserId);
CREATE INDEX idx_MiscellaneousTransactions_TransactionDate ON MiscellaneousTransactions(TransactionDate);
CREATE INDEX idx_MiscellaneousTransactions_MiscCategory ON MiscellaneousTransactions(MiscCategory);

-- ============================================
-- STORED PROCEDURES FOR INCOME
-- ============================================

-- SP: Insert Income Transaction
CREATE PROCEDURE IF NOT EXISTS sp_InsertIncome
    @UserId NVARCHAR(MAX),
    @CropName NVARCHAR(MAX),
    @Quantity DECIMAL(10, 2),
    @PricePerUnit DECIMAL(10, 2),
    @BuyerName NVARCHAR(MAX),
    @TransactionDate DATETIME,
    @Notes NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO IncomeTransactions (
        UserId, CropName, Quantity, QuantityUnit, PricePerUnit, BuyerName,
        Amount, TransactionDate, CreatedDate, Notes, IsDeleted
    )
    VALUES (
        @UserId, @CropName, @Quantity, 'Quintal', @PricePerUnit, @BuyerName,
        (@Quantity * @PricePerUnit), @TransactionDate, GETDATE(), @Notes, 0
    );
END;

-- SP: Get All Income for User in Date Range
CREATE PROCEDURE IF NOT EXISTS sp_GetUserIncome
    @UserId NVARCHAR(MAX),
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SELECT * FROM IncomeTransactions
    WHERE UserId = @UserId
    AND TransactionDate BETWEEN @StartDate AND @EndDate
    AND IsDeleted = 0
    ORDER BY TransactionDate DESC;
END;

-- ============================================
-- STORED PROCEDURES FOR EXPENSES
-- ============================================

-- SP: Insert Expense Transaction
CREATE PROCEDURE IF NOT EXISTS sp_InsertExpense
    @UserId NVARCHAR(MAX),
    @ExpenseCategory NVARCHAR(MAX),
    @ExpenseName NVARCHAR(MAX),
    @Amount DECIMAL(18, 2),
    @TransactionDate DATETIME,
    @Notes NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO ExpenseTransactions (
        UserId, ExpenseCategory, ExpenseName, Amount, TransactionDate,
        CreatedDate, Notes, IsDeleted
    )
    VALUES (
        @UserId, @ExpenseCategory, @ExpenseName, @Amount, @TransactionDate,
        GETDATE(), @Notes, 0
    );
END;

-- SP: Get All Expenses for User in Date Range
CREATE PROCEDURE IF NOT EXISTS sp_GetUserExpenses
    @UserId NVARCHAR(MAX),
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SELECT * FROM ExpenseTransactions
    WHERE UserId = @UserId
    AND TransactionDate BETWEEN @StartDate AND @EndDate
    AND IsDeleted = 0
    ORDER BY TransactionDate DESC;
END;

-- ============================================
-- STORED PROCEDURES FOR LOANS
-- ============================================

-- SP: Insert Loan Transaction
CREATE PROCEDURE IF NOT EXISTS sp_InsertLoan
    @UserId NVARCHAR(MAX),
    @LoanType NVARCHAR(MAX),
    @LenderName NVARCHAR(MAX),
    @Amount DECIMAL(18, 2),
    @InterestRate DECIMAL(5, 2),
    @DueDate DATETIME,
    @Notes NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO LoanTransactions (
        UserId, LoanType, LenderName, Amount, InterestRate, DueDate,
        RemainingAmount, TransactionDate, CreatedDate, Notes, IsDeleted
    )
    VALUES (
        @UserId, @LoanType, @LenderName, @Amount, @InterestRate, @DueDate,
        @Amount, GETDATE(), GETDATE(), @Notes, 0
    );
END;

-- SP: Add Loan Repayment
CREATE PROCEDURE IF NOT EXISTS sp_AddLoanRepayment
    @LoanId INT,
    @AmountRepaid DECIMAL(18, 2),
    @Notes NVARCHAR(MAX)
AS
BEGIN
    BEGIN TRANSACTION;
    
    INSERT INTO LoanRepayments (LoanTransactionId, AmountRepaid, RepaymentDate, Notes)
    VALUES (@LoanId, @AmountRepaid, GETDATE(), @Notes);
    
    UPDATE LoanTransactions
    SET RemainingAmount = RemainingAmount - @AmountRepaid
    WHERE Id = @LoanId;
    
    COMMIT TRANSACTION;
END;

-- SP: Get All Loans for User
CREATE PROCEDURE IF NOT EXISTS sp_GetUserLoans
    @UserId NVARCHAR(MAX)
AS
BEGIN
    SELECT * FROM LoanTransactions
    WHERE UserId = @UserId
    AND IsDeleted = 0
    ORDER BY TransactionDate DESC;
END;

-- ============================================
-- STORED PROCEDURES FOR SUBSIDIES
-- ============================================

-- SP: Insert Subsidy Transaction
CREATE PROCEDURE IF NOT EXISTS sp_InsertSubsidy
    @UserId NVARCHAR(MAX),
    @SchemeName NVARCHAR(MAX),
    @SubsidyType NVARCHAR(MAX),
    @Amount DECIMAL(18, 2),
    @ReceivedDate DATETIME,
    @Notes NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO SubsidyTransactions (
        UserId, SchemeName, SubsidyType, Amount, ReceivedDate,
        TransactionDate, CreatedDate, Notes, IsDeleted
    )
    VALUES (
        @UserId, @SchemeName, @SubsidyType, @Amount, @ReceivedDate,
        @ReceivedDate, GETDATE(), @Notes, 0
    );
END;

-- SP: Get All Subsidies for User in Date Range
CREATE PROCEDURE IF NOT EXISTS sp_GetUserSubsidies
    @UserId NVARCHAR(MAX),
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SELECT * FROM SubsidyTransactions
    WHERE UserId = @UserId
    AND TransactionDate BETWEEN @StartDate AND @EndDate
    AND IsDeleted = 0
    ORDER BY TransactionDate DESC;
END;

-- ============================================
-- ANALYTICS VIEWS
-- ============================================

-- View: Monthly Financial Summary
CREATE VIEW IF NOT EXISTS vw_MonthlySummary AS
SELECT 
    ft.UserId,
    MONTH(ft.TransactionDate) AS Month,
    YEAR(ft.TransactionDate) AS Year,
    ft.TransactionType,
    SUM(ft.Amount) AS TotalAmount,
    COUNT(*) AS TransactionCount
FROM FinanceTransactions ft
WHERE ft.IsDeleted = 0
GROUP BY ft.UserId, MONTH(ft.TransactionDate), YEAR(ft.TransactionDate), ft.TransactionType;

-- View: Expense Breakdown by Category
CREATE VIEW IF NOT EXISTS vw_ExpenseBreakdown AS
SELECT 
    et.UserId,
    et.ExpenseCategory,
    COUNT(*) AS Count,
    SUM(et.Amount) AS TotalAmount,
    AVG(et.Amount) AS AverageAmount
FROM ExpenseTransactions et
WHERE et.IsDeleted = 0
GROUP BY et.UserId, et.ExpenseCategory;

-- View: Income Summary by Crop
CREATE VIEW IF NOT EXISTS vw_IncomeByCrop AS
SELECT 
    it.UserId,
    it.CropName,
    COUNT(*) AS SalesCount,
    SUM(it.Quantity) AS TotalQuantity,
    SUM(it.Amount) AS TotalIncome,
    AVG(it.PricePerUnit) AS AveragePricePerUnit
FROM IncomeTransactions it
WHERE it.IsDeleted = 0
GROUP BY it.UserId, it.CropName;

-- View: Outstanding Loans
CREATE VIEW IF NOT EXISTS vw_OutstandingLoans AS
SELECT 
    lt.UserId,
    lt.LenderName,
    lt.LoanType,
    lt.Amount,
    lt.RemainingAmount,
    lt.InterestRate,
    lt.DueDate,
    (lt.Amount - lt.RemainingAmount) AS AmountRepaid
FROM LoanTransactions lt
WHERE lt.IsDeleted = 0 AND lt.IsRepaid = 0;

-- ============================================
-- UTILITY FUNCTIONS
-- ============================================

-- Function: Calculate Net Profit for User
CREATE PROCEDURE IF NOT EXISTS sp_GetNetProfit
    @UserId NVARCHAR(MAX),
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SELECT 
        @UserId AS UserId,
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Income' THEN ft.Amount ELSE 0 END), 0) AS TotalIncome,
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Expense' THEN ft.Amount ELSE 0 END), 0) AS TotalExpense,
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Subsidy' THEN ft.Amount ELSE 0 END), 0) AS TotalSubsidy,
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Income' THEN ft.Amount ELSE 0 END), 0) +
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Subsidy' THEN ft.Amount ELSE 0 END), 0) -
        COALESCE(SUM(CASE WHEN ft.TransactionType = 'Expense' THEN ft.Amount ELSE 0 END), 0) AS NetProfit
    FROM FinanceTransactions ft
    WHERE ft.UserId = @UserId
    AND ft.TransactionDate BETWEEN @StartDate AND @EndDate
    AND ft.IsDeleted = 0;
END;

-- ============================================
-- SEED DATA (Optional - for testing)
-- ============================================
-- Uncomment the following section to add sample data for testing

/*
-- Sample Income Transaction
INSERT INTO IncomeTransactions (UserId, CropName, Quantity, QuantityUnit, PricePerUnit, BuyerName, Amount, TransactionDate, CreatedDate, IsDeleted)
VALUES ('user123', 'Rice', 10, 'Quintal', 5000, 'Local Trader', 50000, GETDATE(), GETDATE(), 0);

-- Sample Expense Transaction
INSERT INTO ExpenseTransactions (UserId, ExpenseCategory, ExpenseName, Amount, TransactionDate, CreatedDate, IsDeleted)
VALUES ('user123', 'Seeds', 'Rice Seeds', 2000, GETDATE(), GETDATE(), 0);

-- Sample Loan Transaction
INSERT INTO LoanTransactions (UserId, LoanType, LenderName, Amount, InterestRate, DueDate, RemainingAmount, TransactionDate, CreatedDate, IsDeleted)
VALUES ('user123', 'Bank', 'State Bank', 50000, 8.5, DATEADD(YEAR, 1, GETDATE()), 50000, GETDATE(), GETDATE(), 0);

-- Sample Subsidy Transaction
INSERT INTO SubsidyTransactions (UserId, SchemeName, SubsidyType, Amount, ReceivedDate, TransactionDate, CreatedDate, IsDeleted)
VALUES ('user123', 'PM-Kisan', 'Direct Payment', 6000, GETDATE(), GETDATE(), GETDATE(), 0);
*/
