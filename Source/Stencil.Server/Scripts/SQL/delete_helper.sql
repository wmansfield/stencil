

-- <Foreign Keys> --------------------------------------------------------------------

ALTER TABLE [dbo].[Remark] DROP  [FK_Remark_Post_post_id]

ALTER TABLE [dbo].[Remark] DROP  [FK_Remark_Account_account_id]

GO

ALTER TABLE [dbo].[Post] DROP  [FK_Post_Account_account_id]

GO

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

DROP TABLE [dbo].[Remark]
GO

DROP TABLE [dbo].[Post]
GO

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




DROP PROCEDURE [dbo].[spPost_SyncUpdate]
GO

DROP PROCEDURE [dbo].[spPost_SyncGetInvalid]
GO

DROP PROCEDURE [dbo].[spPost_HydrateSyncUpdate]
GO

DROP PROCEDURE [dbo].[spPost_HydrateSyncGetInvalid]
GO




DROP PROCEDURE [dbo].[spRemark_SyncUpdate]
GO

DROP PROCEDURE [dbo].[spRemark_SyncGetInvalid]
GO

DROP PROCEDURE [dbo].[spRemark_HydrateSyncUpdate]
GO

DROP PROCEDURE [dbo].[spRemark_HydrateSyncGetInvalid]
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
