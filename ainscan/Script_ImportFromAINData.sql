-- =============================================================================
-- Script: Import records from AINData into PQData
-- Run on: PQData server (194.100.1.222) using SSMS
-- Purpose: Copy all rows from AINData (live) into PQData (demo)
-- NOTE: Run Script_RefreshPQData.sql (delete) FIRST before running this
-- =============================================================================

USE [PQData]
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 1: Setup linked server to AINData (auto-skips if already exists)
-- This tells PQData server how to connect to AINData server
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sys.servers WHERE name = 'AINDATA_LIVE')
BEGIN
    EXEC sp_addlinkedserver
        @server     = 'AINDATA_LIVE',
        @srvproduct = '',
        @provider   = 'SQLNCLI',
        @datasrc    = '194.100.1.249';

    EXEC sp_addlinkedsrvlogin
        @rmtsrvname  = 'AINDATA_LIVE',
        @useself     = 'FALSE',
        @rmtuser     = 'sa',
        @rmtpassword = 'ain06@sql';

    PRINT 'Linked server created.'
END
ELSE
    PRINT 'Linked server already exists. OK.'
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 2: Copy all rows from AINData into PQData
-- Each INSERT reads from AINData (live) and writes into PQData (this server)
-- ─────────────────────────────────────────────────────────────────────────────

PRINT 'Importing BD_0010...'
INSERT INTO [PQData].[dbo].[BD_0010]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[BD_0010]
PRINT '  Done.'

PRINT 'Importing PD_0800...'
INSERT INTO [PQData].[dbo].[PD_0800]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[PD_0800]
PRINT '  Done.'

PRINT 'Importing TA_PLT001...'
INSERT INTO [PQData].[dbo].[TA_PLT001]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[TA_PLT001]
PRINT '  Done.'

PRINT 'Importing TA_PLT002...'
INSERT INTO [PQData].[dbo].[TA_PLT002]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[TA_PLT002]
PRINT '  Done.'

PRINT 'Importing TA_PLT003...'
INSERT INTO [PQData].[dbo].[TA_PLT003]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[TA_PLT003]
PRINT '  Done.'

PRINT 'Importing TA_PLL001...'
INSERT INTO [PQData].[dbo].[TA_PLL001]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[TA_PLL001]
PRINT '  Done.'

PRINT 'Importing TA_LOC0300...'
INSERT INTO [PQData].[dbo].[TA_LOC0300]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[TA_LOC0300]
PRINT '  Done.'

PRINT 'Importing TA_LOC0600...'
INSERT INTO [PQData].[dbo].[TA_LOC0600]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[TA_LOC0600]
PRINT '  Done.'

PRINT 'Importing TA_LOC0700...'
INSERT INTO [PQData].[dbo].[TA_LOC0700]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[TA_LOC0700]
PRINT '  Done.'

PRINT 'Importing DO_0010...'
INSERT INTO [PQData].[dbo].[DO_0010]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[DO_0010]
PRINT '  Done.'

PRINT 'Importing DO_0020...'
INSERT INTO [PQData].[dbo].[DO_0020]
SELECT * FROM [AINDATA_LIVE].[AINData].[dbo].[DO_0020]
PRINT '  Done.'

PRINT '=== IMPORT COMPLETE ==='
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 3: Verify row counts
-- ─────────────────────────────────────────────────────────────────────────────
SELECT 'BD_0010'    AS [Table], COUNT(*) AS [Rows] FROM [PQData].[dbo].[BD_0010]    UNION ALL
SELECT 'PD_0800',                COUNT(*)          FROM [PQData].[dbo].[PD_0800]    UNION ALL
SELECT 'TA_PLT001',              COUNT(*)          FROM [PQData].[dbo].[TA_PLT001]  UNION ALL
SELECT 'TA_PLT002',              COUNT(*)          FROM [PQData].[dbo].[TA_PLT002]  UNION ALL
SELECT 'TA_PLT003',              COUNT(*)          FROM [PQData].[dbo].[TA_PLT003]  UNION ALL
SELECT 'TA_PLL001',              COUNT(*)          FROM [PQData].[dbo].[TA_PLL001]  UNION ALL
SELECT 'TA_LOC0300',             COUNT(*)          FROM [PQData].[dbo].[TA_LOC0300] UNION ALL
SELECT 'TA_LOC0600',             COUNT(*)          FROM [PQData].[dbo].[TA_LOC0600] UNION ALL
SELECT 'TA_LOC0700',             COUNT(*)          FROM [PQData].[dbo].[TA_LOC0700] UNION ALL
SELECT 'DO_0010',                COUNT(*)          FROM [PQData].[dbo].[DO_0010]    UNION ALL
SELECT 'DO_0020',                COUNT(*)          FROM [PQData].[dbo].[DO_0020]
ORDER BY [Table]
GO
