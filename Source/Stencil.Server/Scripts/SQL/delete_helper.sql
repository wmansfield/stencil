

-- <Foreign Keys> --------------------------------------------------------------------

GO

GO

GO

-- </Foreign Keys> --------------------------------------------------------------------



-- <Unique Keys> --------------------------------------------------------------------

IF OBJECT_ID('dbo.UK_account_key', 'UQ') IS NOT NULL BEGIN -- multiple passes because of script limitations, thats fine. :)
	ALTER TABLE [dbo].[Account] 
		DROP CONSTRAINT UK_account_key
END
GO


-- </Unique Keys> --------------------------------------------------------------------


-- <Tables> --------------------------------------------------------------------

DROP TABLE [dbo].[Asset]
GO

DROP TABLE [dbo].[Account]
GO

DROP TABLE [dbo].[GlobalSetting]
GO

-- </Tables> --------------------------------------------------------------------

-- <Procedures> --------------------------------------------------------------------


DROP PROCEDURE [dbo].[spAccount_SyncUpdate]
GO

DROP PROCEDURE [dbo].[spAccount_SyncGetInvalid]
GO

DROP PROCEDURE [dbo].[spAccount_HydrateSyncUpdate]
GO

DROP PROCEDURE [dbo].[spAccount_HydrateSyncGetInvalid]
GO




DROP PROCEDURE [dbo].[spIndex_InvalidateAll]
GO
DROP PROCEDURE [dbo].[spIndex_InvalidateAggregates]
GO

DROP PROCEDURE [dbo].[spIndex_Status]
GO

DROP PROCEDURE [dbo].[spIndexHydrate_InvalidateAll]
GO

DROP PROCEDURE [dbo].[spIndexHydrate_InvalidateAggregates]
GO


DROP PROCEDURE [dbo].[spIndexHydrate_Status]
GO


-- <Procedures> --------------------------------------------------------------------
