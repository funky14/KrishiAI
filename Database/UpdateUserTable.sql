-- SQL Script to update User table with password reset columns
-- Run this in SQL Server Management Studio on your Azure SQL database

-- Check if columns exist and add them if needed
IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'User' AND COLUMN_NAME = 'PasswordResetToken'
)
BEGIN
	ALTER TABLE [dbo].[User] ADD [PasswordResetToken] NVARCHAR(MAX) NULL;
	PRINT 'Added PasswordResetToken column';
END

IF NOT EXISTS (
	SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'User' AND COLUMN_NAME = 'PasswordResetExpiry'
)
BEGIN
	ALTER TABLE [dbo].[User] ADD [PasswordResetExpiry] DATETIME NULL;
	PRINT 'Added PasswordResetExpiry column';
END

-- Verify the table structure
SELECT 
	COLUMN_NAME, 
	DATA_TYPE, 
	IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'User'
ORDER BY ORDINAL_POSITION;
