-- =============================================================================
-- Script: Drop all PQData tables entirely
-- Run on: PQData server (194.100.1.222) only
-- Purpose: Remove tables so you can recreate them fresh
-- NOTE: After running this, run Script_AINScan_New.sql to recreate tables,
--       then run Script_ImportFromAINData.sql to import data
-- =============================================================================

USE [PQData]
GO

-- Drop child/detail tables first, then parent/master tables
IF OBJECT_ID('dbo.DO_0020',    'U') IS NOT NULL DROP TABLE [dbo].[DO_0020]
IF OBJECT_ID('dbo.DO_0010',    'U') IS NOT NULL DROP TABLE [dbo].[DO_0010]
IF OBJECT_ID('dbo.TA_LOC0700', 'U') IS NOT NULL DROP TABLE [dbo].[TA_LOC0700]
IF OBJECT_ID('dbo.TA_LOC0600', 'U') IS NOT NULL DROP TABLE [dbo].[TA_LOC0600]
IF OBJECT_ID('dbo.TA_LOC0300', 'U') IS NOT NULL DROP TABLE [dbo].[TA_LOC0300]
IF OBJECT_ID('dbo.TA_PLL001',  'U') IS NOT NULL DROP TABLE [dbo].[TA_PLL001]
IF OBJECT_ID('dbo.TA_PLT003',  'U') IS NOT NULL DROP TABLE [dbo].[TA_PLT003]
IF OBJECT_ID('dbo.TA_PLT002',  'U') IS NOT NULL DROP TABLE [dbo].[TA_PLT002]
IF OBJECT_ID('dbo.TA_PLT001',  'U') IS NOT NULL DROP TABLE [dbo].[TA_PLT001]
IF OBJECT_ID('dbo.PD_0800',    'U') IS NOT NULL DROP TABLE [dbo].[PD_0800]
IF OBJECT_ID('dbo.BD_0010',    'U') IS NOT NULL DROP TABLE [dbo].[BD_0010]

PRINT '=== ALL 11 TABLES DROPPED ==='
GO
