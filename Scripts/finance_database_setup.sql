-- Finance Tracker Database Schema
-- This script creates the database tables and stored procedures for the Finance Tracker feature
-- Syntax: Microsoft SQL Server (T-SQL)

-- ============================================
-- 1. FINANCE TRANSACTIONS TABLE
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FinanceTransactions')
BEGIN
    CREATE TABLE FinanceTransactions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        UserId          INT NOT NULL,
        TransactionType NVARCHAR(50)  NOT NULL,
        Category        NVARCHAR(100) NOT NULL,
        Description     NVARCHAR(MAX),
        Amount          DECIMAL(18, 2) NOT NULL,
        TransactionDate DATETIME2     NOT NULL,
        CreatedDate     DATETIME2     NOT NULL DEFAULT GETDATE(),
        UpdatedDate     DATETIME2,
        Notes           NVARCHAR(MAX),
        IsDeleted       BIT           NOT NULL DEFAULT 0,
        CONSTRAINT FK_FinanceTransactions_Users FOREIGN KEY (Id) REFERENCES [User](Id)
    );

    CREATE INDEX idx_FinanceTransactions_UserId          ON FinanceTransactions(UserId);
    CREATE INDEX idx_FinanceTransactions_TransactionType ON FinanceTransactions(TransactionType);
    CREATE INDEX idx_FinanceTransactions_TransactionDate ON FinanceTransactions(TransactionDate);
    CREATE INDEX idx_FinanceTransactions_IsDeleted       ON FinanceTransactions(IsDeleted);
END
GO

-- ============================================
-- 2. INCOME TRANSACTIONS TABLE
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IncomeTransactions')
BEGIN
    CREATE TABLE IncomeTransactions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        UserId          INT  NOT NULL,
        TransactionType NVARCHAR(50)   NOT NULL DEFAULT 'Income',
        Category        NVARCHAR(100),
        Description     NVARCHAR(MAX),
        Amount          DECIMAL(18, 2) NOT NULL,
        TransactionDate DATETIME2      NOT NULL,
        CreatedDate     DATETIME2      NOT NULL DEFAULT GETDATE(),
        UpdatedDate     DATETIME2,
        Notes           NVARCHAR(MAX),
        IsDeleted       BIT            NOT NULL DEFAULT 0,
        CropName        NVARCHAR(200)  NOT NULL,
        Quantity        DECIMAL(10, 2) NOT NULL,
        QuantityUnit    NVARCHAR(50)   NOT NULL DEFAULT 'Quintal',
        PricePerUnit    DECIMAL(10, 2) NOT NULL,
        BuyerName       NVARCHAR(200),
        CONSTRAINT FK_IncomeTransactions_Users FOREIGN KEY (UserId) REFERENCES [User](Id)
    );

    CREATE INDEX idx_IncomeTransactions_UserId          ON IncomeTransactions(UserId);
    CREATE INDEX idx_IncomeTransactions_TransactionDate ON IncomeTransactions(TransactionDate);
    CREATE INDEX idx_IncomeTransactions_CropName        ON IncomeTransactions(CropName);
END
GO

-- ============================================
-- 3. EXPENSE TRANSACTIONS TABLE
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExpenseTransactions')
BEGIN
    CREATE TABLE ExpenseTransactions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        UserId          INT  NOT NULL,
        TransactionType NVARCHAR(50)   NOT NULL DEFAULT 'Expense',
        Category        NVARCHAR(100),
        Description     NVARCHAR(MAX),
        Amount          DECIMAL(18, 2) NOT NULL,
        TransactionDate DATETIME2      NOT NULL,
        CreatedDate     DATETIME2      NOT NULL DEFAULT GETDATE(),
        UpdatedDate     DATETIME2,
        Notes           NVARCHAR(MAX),
        IsDeleted       BIT            NOT NULL DEFAULT 0,
        ExpenseCategory NVARCHAR(100)  NOT NULL,
        ExpenseName     NVARCHAR(200)  NOT NULL,
        CONSTRAINT FK_ExpenseTransactions_Users FOREIGN KEY (UserId) REFERENCES [User](Id)
    );

    CREATE INDEX idx_ExpenseTransactions_UserId          ON ExpenseTransactions(UserId);
    CREATE INDEX idx_ExpenseTransactions_TransactionDate ON ExpenseTransactions(TransactionDate);
    CREATE INDEX idx_ExpenseTransactions_ExpenseCategory ON ExpenseTransactions(ExpenseCategory);
END
GO

-- ============================================
-- 4. LOAN TRANSACTIONS TABLE
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanTransactions')
BEGIN
    CREATE TABLE LoanTransactions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        UserId          INT  NOT NULL,
        TransactionType NVARCHAR(50)   NOT NULL DEFAULT 'Loan',
        Category        NVARCHAR(100),
        Description     NVARCHAR(MAX),
        Amount          DECIMAL(18, 2) NOT NULL,
        TransactionDate DATETIME2      NOT NULL,
        CreatedDate     DATETIME2      NOT NULL DEFAULT GETDATE(),
        UpdatedDate     DATETIME2,
        Notes           NVARCHAR(MAX),
        IsDeleted       BIT            NOT NULL DEFAULT 0,
        LoanType        NVARCHAR(100)  NOT NULL,
        LenderName      NVARCHAR(200)  NOT NULL,
        InterestRate    DECIMAL(5, 2)  NOT NULL DEFAULT 0,
        DueDate         DATETIME2,
        IsRepaid        BIT            NOT NULL DEFAULT 0,
        RemainingAmount DECIMAL(18, 2),
        CONSTRAINT FK_LoanTransactions_Users FOREIGN KEY (UserId) REFERENCES [User](Id)
    );

    CREATE INDEX idx_LoanTransactions_UserId          ON LoanTransactions(UserId);
    CREATE INDEX idx_LoanTransactions_TransactionDate ON LoanTransactions(TransactionDate);
    CREATE INDEX idx_LoanTransactions_LoanType        ON LoanTransactions(LoanType);
    CREATE INDEX idx_LoanTransactions_IsRepaid        ON LoanTransactions(IsRepaid);
END
GO

-- ============================================
-- 5. LOAN REPAYMENT TABLE
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanRepayments')
BEGIN
    CREATE TABLE LoanRepayments (
        Id                INT IDENTITY(1,1) PRIMARY KEY,
        LoanTransactionId INT            NOT NULL,
        AmountRepaid      DECIMAL(18, 2) NOT NULL,
        RepaymentDate     DATETIME2      NOT NULL DEFAULT GETDATE(),
        Notes             NVARCHAR(MAX),
        CONSTRAINT FK_LoanRepayments_LoanTransactions FOREIGN KEY (LoanTransactionId) REFERENCES LoanTransactions(Id)
    );

    CREATE INDEX idx_LoanRepayments_LoanTransactionId ON LoanRepayments(LoanTransactionId);
    CREATE INDEX idx_LoanRepayments_RepaymentDate     ON LoanRepayments(RepaymentDate);
END
GO

-- ============================================
-- 6. SUBSIDY TRANSACTIONS TABLE
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubsidyTransactions')
BEGIN
    CREATE TABLE SubsidyTransactions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        UserId          INT  NOT NULL,
        TransactionType NVARCHAR(50)   NOT NULL DEFAULT 'Subsidy',
        Category        NVARCHAR(100),
        Description     NVARCHAR(MAX),
        Amount          DECIMAL(18, 2) NOT NULL,
        TransactionDate DATETIME2      NOT NULL,
        CreatedDate     DATETIME2      NOT NULL DEFAULT GETDATE(),
        UpdatedDate     DATETIME2,
        Notes           NVARCHAR(MAX),
        IsDeleted       BIT            NOT NULL DEFAULT 0,
        SchemeName      NVARCHAR(200)  NOT NULL,
        SubsidyType     NVARCHAR(100)  NOT NULL,
        ReceivedDate    DATETIME2      NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_SubsidyTransactions_Users FOREIGN KEY (UserId) REFERENCES [User](Id)
    );

    CREATE INDEX idx_SubsidyTransactions_UserId          ON SubsidyTransactions(UserId);
    CREATE INDEX idx_SubsidyTransactions_TransactionDate ON SubsidyTransactions(TransactionDate);
    CREATE INDEX idx_SubsidyTransactions_SchemeName      ON SubsidyTransactions(SchemeName);
END
GO

-- ============================================
-- 7. MISCELLANEOUS TRANSACTIONS TABLE
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MiscellaneousTransactions')
BEGIN
    CREATE TABLE MiscellaneousTransactions (
        Id                   INT IDENTITY(1,1) PRIMARY KEY,
        UserId               INT NOT NULL,
        TransactionType      NVARCHAR(50)  NOT NULL DEFAULT 'Miscellaneous',
        Category             NVARCHAR(100),
        Description          NVARCHAR(MAX),
        Amount               DECIMAL(18, 2) NOT NULL,
        TransactionDate      DATETIME2     NOT NULL,
        CreatedDate          DATETIME2     NOT NULL DEFAULT GETDATE(),
        UpdatedDate          DATETIME2,
        Notes                NVARCHAR(MAX),
        IsDeleted            BIT           NOT NULL DEFAULT 0,
        TransactionDirection NVARCHAR(20)  NOT NULL DEFAULT 'Outgoing',
        MiscCategory         NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_MiscellaneousTransactions_Users FOREIGN KEY (UserId) REFERENCES [User](Id)
    );

    CREATE INDEX idx_MiscellaneousTransactions_UserId          ON MiscellaneousTransactions(UserId);
    CREATE INDEX idx_MiscellaneousTransactions_TransactionDate ON MiscellaneousTransactions(TransactionDate);
    CREATE INDEX idx_MiscellaneousTransactions_MiscCategory    ON MiscellaneousTransactions(MiscCategory);
END
GO

-- ============================================
-- STORED PROCEDURES FOR INCOME
-- ============================================

-- SP: Insert Income Transaction
CREATE OR ALTER PROCEDURE sp_InsertIncome
    @UserId          NVARCHAR(450),
    @CropName        NVARCHAR(200),
    @Quantity        DECIMAL(10, 2),
    @PricePerUnit    DECIMAL(10, 2),
    @BuyerName       NVARCHAR(200),
    @TransactionDate DATETIME2,
    @Notes           NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO IncomeTransactions (
        UserId, CropName, Quantity, QuantityUnit, PricePerUnit, BuyerName,
        Amount, TransactionDate, CreatedDate, Notes, IsDeleted
    )
    VALUES (
        @UserId, @CropName, @Quantity, 'Quintal', @PricePerUnit, @BuyerName,
        (@Quantity * @PricePerUnit), @TransactionDate, GETDATE(), @Notes, 0
    );
END
GO

-- SP: Get All Income for User in Date Range
CREATE OR ALTER PROCEDURE sp_GetUserIncome
    @UserId    NVARCHAR(450),
    @StartDate DATETIME2,
    @EndDate   DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM IncomeTransactions
    WHERE UserId = @UserId
      AND TransactionDate BETWEEN @StartDate AND @EndDate
      AND IsDeleted = 0
    ORDER BY TransactionDate DESC;
END
GO

-- ============================================
-- STORED PROCEDURES FOR EXPENSES
-- ============================================

-- SP: Insert Expense Transaction
CREATE OR ALTER PROCEDURE sp_InsertExpense
    @UserId          NVARCHAR(450),
    @ExpenseCategory NVARCHAR(100),
    @ExpenseName     NVARCHAR(200),
    @Amount          DECIMAL(18, 2),
    @TransactionDate DATETIME2,
    @Notes           NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ExpenseTransactions (
        UserId, ExpenseCategory, ExpenseName, Amount, TransactionDate,
        CreatedDate, Notes, IsDeleted
    )
    VALUES (
        @UserId, @ExpenseCategory, @ExpenseName, @Amount, @TransactionDate,
        GETDATE(), @Notes, 0
    );
END
GO

-- SP: Get All Expenses for User in Date Range
CREATE OR ALTER PROCEDURE sp_GetUserExpenses
    @UserId    NVARCHAR(450),
    @StartDate DATETIME2,
    @EndDate   DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM ExpenseTransactions
    WHERE UserId = @UserId
      AND TransactionDate BETWEEN @StartDate AND @EndDate
      AND IsDeleted = 0
    ORDER BY TransactionDate DESC;
END
GO

-- ============================================
-- STORED PROCEDURES FOR LOANS
-- ============================================

-- SP: Insert Loan Transaction
CREATE OR ALTER PROCEDURE sp_InsertLoan
    @UserId       NVARCHAR(450),
    @LoanType     NVARCHAR(100),
    @LenderName   NVARCHAR(200),
    @Amount       DECIMAL(18, 2),
    @InterestRate DECIMAL(5, 2),
    @DueDate      DATETIME2,
    @Notes        NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LoanTransactions (
        UserId, LoanType, LenderName, Amount, InterestRate, DueDate,
        RemainingAmount, TransactionDate, CreatedDate, Notes, IsDeleted
    )
    VALUES (
        @UserId, @LoanType, @LenderName, @Amount, @InterestRate, @DueDate,
        @Amount, GETDATE(), GETDATE(), @Notes, 0
    );
END
GO

-- SP: Add Loan Repayment
CREATE OR ALTER PROCEDURE sp_AddLoanRepayment
    @LoanId      INT,
    @AmountRepaid DECIMAL(18, 2),
    @Notes       NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        INSERT INTO LoanRepayments (LoanTransactionId, AmountRepaid, RepaymentDate, Notes)
        VALUES (@LoanId, @AmountRepaid, GETDATE(), @Notes);

        UPDATE LoanTransactions
        SET RemainingAmount = RemainingAmount - @AmountRepaid
        WHERE Id = @LoanId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Get All Loans for User
CREATE OR ALTER PROCEDURE sp_GetUserLoans
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM LoanTransactions
    WHERE UserId = @UserId
      AND IsDeleted = 0
    ORDER BY TransactionDate DESC;
END
GO

-- ============================================
-- STORED PROCEDURES FOR SUBSIDIES
-- ============================================

-- SP: Insert Subsidy Transaction
CREATE OR ALTER PROCEDURE sp_InsertSubsidy
    @UserId       NVARCHAR(450),
    @SchemeName   NVARCHAR(200),
    @SubsidyType  NVARCHAR(100),
    @Amount       DECIMAL(18, 2),
    @ReceivedDate DATETIME2,
    @Notes        NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SubsidyTransactions (
        UserId, SchemeName, SubsidyType, Amount, ReceivedDate,
        TransactionDate, CreatedDate, Notes, IsDeleted
    )
    VALUES (
        @UserId, @SchemeName, @SubsidyType, @Amount, @ReceivedDate,
        @ReceivedDate, GETDATE(), @Notes, 0
    );
END
GO

-- SP: Get All Subsidies for User in Date Range
CREATE OR ALTER PROCEDURE sp_GetUserSubsidies
    @UserId    NVARCHAR(450),
    @StartDate DATETIME2,
    @EndDate   DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM SubsidyTransactions
    WHERE UserId = @UserId
      AND TransactionDate BETWEEN @StartDate AND @EndDate
      AND IsDeleted = 0
    ORDER BY TransactionDate DESC;
END
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
    et.UserId,
    et.ExpenseCategory,
    COUNT(*)        AS [Count],
    SUM(et.Amount)  AS TotalAmount,
    AVG(et.Amount)  AS AverageAmount
FROM ExpenseTransactions et
WHERE et.IsDeleted = 0
GROUP BY et.UserId, et.ExpenseCategory;
GO

-- View: Income Summary by Crop
CREATE OR ALTER VIEW vw_IncomeByCrop AS
SELECT
    it.UserId,
    it.CropName,
    COUNT(*)               AS SalesCount,
    SUM(it.Quantity)       AS TotalQuantity,
    SUM(it.Amount)         AS TotalIncome,
    AVG(it.PricePerUnit)   AS AveragePricePerUnit
FROM IncomeTransactions it
WHERE it.IsDeleted = 0
GROUP BY it.UserId, it.CropName;
GO

-- View: Outstanding Loans
CREATE OR ALTER VIEW vw_OutstandingLoans AS
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
