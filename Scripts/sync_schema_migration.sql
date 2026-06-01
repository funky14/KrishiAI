/*
  Sync schema migration for dbo.DiseaseHistory
  Safe to run multiple times (idempotent checks).
*/

SET NOCOUNT ON;

IF OBJECT_ID('dbo.DiseaseHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DiseaseHistory (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DiseaseHistory PRIMARY KEY,
        ImagePath NVARCHAR(MAX) NOT NULL,
        DiseaseName NVARCHAR(255) NOT NULL,
        Confidence FLOAT NOT NULL,
        Severity NVARCHAR(50) NOT NULL,
        DetectedDate DATETIME2 NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        AffectedCropPart NVARCHAR(255) NOT NULL,
        TreatmentRecommendations NVARCHAR(MAX) NULL,
        RemoteId UNIQUEIDENTIFIER NULL,
        IsSynced BIT NOT NULL CONSTRAINT DF_DiseaseHistory_IsSynced DEFAULT(0),
        LastSyncTime DATETIME2 NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_DiseaseHistory_IsDeleted DEFAULT(0),
        SyncRetryCount INT NOT NULL CONSTRAINT DF_DiseaseHistory_SyncRetryCount DEFAULT(0),
        SyncError NVARCHAR(MAX) NULL,
        CloudImageUrl NVARCHAR(MAX) NULL,
        ImageUploaded BIT NOT NULL CONSTRAINT DF_DiseaseHistory_ImageUploaded DEFAULT(0),
        CreatedDateUtc DATETIME2 NULL,
        LastModifiedDateUtc DATETIME2 NULL,
        Version INT NOT NULL CONSTRAINT DF_DiseaseHistory_Version DEFAULT(1)
    );
END;

IF COL_LENGTH('dbo.DiseaseHistory', 'RemoteId') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD RemoteId UNIQUEIDENTIFIER NULL;

IF COL_LENGTH('dbo.DiseaseHistory', 'IsSynced') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD IsSynced BIT NOT NULL CONSTRAINT DF_DiseaseHistory_IsSynced DEFAULT(0);

IF COL_LENGTH('dbo.DiseaseHistory', 'LastSyncTime') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD LastSyncTime DATETIME2 NULL;

IF COL_LENGTH('dbo.DiseaseHistory', 'IsDeleted') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD IsDeleted BIT NOT NULL CONSTRAINT DF_DiseaseHistory_IsDeleted DEFAULT(0);

IF COL_LENGTH('dbo.DiseaseHistory', 'SyncRetryCount') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD SyncRetryCount INT NOT NULL CONSTRAINT DF_DiseaseHistory_SyncRetryCount DEFAULT(0);

IF COL_LENGTH('dbo.DiseaseHistory', 'SyncError') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD SyncError NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.DiseaseHistory', 'CloudImageUrl') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD CloudImageUrl NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.DiseaseHistory', 'ImageUploaded') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD ImageUploaded BIT NOT NULL CONSTRAINT DF_DiseaseHistory_ImageUploaded DEFAULT(0);

IF COL_LENGTH('dbo.DiseaseHistory', 'CreatedDateUtc') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD CreatedDateUtc DATETIME2 NULL;

IF COL_LENGTH('dbo.DiseaseHistory', 'LastModifiedDateUtc') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD LastModifiedDateUtc DATETIME2 NULL;

IF COL_LENGTH('dbo.DiseaseHistory', 'Version') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD Version INT NOT NULL CONSTRAINT DF_DiseaseHistory_Version DEFAULT(1);

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.DiseaseHistory')
      AND name = 'IX_IsSynced'
)
    CREATE INDEX IX_IsSynced ON dbo.DiseaseHistory(IsSynced, IsDeleted);

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.DiseaseHistory')
      AND name = 'IX_RemoteId'
)
    CREATE INDEX IX_RemoteId ON dbo.DiseaseHistory(RemoteId);

SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'DiseaseHistory'
ORDER BY ORDINAL_POSITION;
-- Device tracking for multi-device support
IF COL_LENGTH('dbo.DiseaseHistory', 'DeviceId') IS NULL
     ALTER TABLE dbo.DiseaseHistory ADD DeviceId NVARCHAR(255) NULL;

IF COL_LENGTH('dbo.DiseaseHistory', 'DeviceName') IS NULL
     ALTER TABLE dbo.DiseaseHistory ADD DeviceName NVARCHAR(255) NULL;

IF NOT EXISTS (
     SELECT 1
     FROM sys.indexes
     WHERE object_id = OBJECT_ID('dbo.DiseaseHistory')
        AND name = 'IX_DeviceId'
)
     CREATE INDEX IX_DeviceId ON dbo.DiseaseHistory(DeviceId);

-- Final schema verification
SELECT
     COLUMN_NAME,
     DATA_TYPE,
     IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'DiseaseHistory'
ORDER BY ORDINAL_POSITION;
