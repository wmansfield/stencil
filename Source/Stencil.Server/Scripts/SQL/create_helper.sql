
-- <Tables> --------------------------------------------------------------------

CREATE TABLE [dbo].[GlobalSetting] (
	 [global_setting_id] uniqueidentifier NOT NULL
    ,[name] nvarchar(100) NOT NULL
    ,[value] nvarchar(max) NULL
    
  ,CONSTRAINT [PK_GlobalSetting] PRIMARY KEY CLUSTERED 
  (
	  [global_setting_id] ASC
  )
)

GO


CREATE TABLE [dbo].[Account] (
	 [account_id] uniqueidentifier NOT NULL
    ,[email] nvarchar(250) NOT NULL
    ,[password] nvarchar(250) NOT NULL
    ,[password_salt] nvarchar(50) NOT NULL
    ,[disabled] bit NOT NULL
    ,[api_key] nvarchar(50) NOT NULL
    ,[api_secret] nvarchar(50) NOT NULL
    ,[first_name] nvarchar(50) NULL
    ,[last_name] nvarchar(50) NULL
    ,[entitlements] nvarchar(250) NULL
    ,[password_reset_token] nvarchar(50) NULL
    ,[password_reset_utc] datetimeoffset(0) NULL
    ,[push_ios] nvarchar(100) NULL
    ,[push_google] nvarchar(100) NULL
    ,[push_microsoft] nvarchar(100) NULL
    ,[last_login_utc] datetimeoffset(0) NULL
    ,[last_login_platform] nvarchar(250) NULL
    ,[created_utc] DATETIMEOFFSET(0) NOT NULL
    ,[updated_utc] DATETIMEOFFSET(0) NOT NULL
    ,[deleted_utc] DATETIMEOFFSET(0) NULL
	,[sync_hydrate_utc] DATETIMEOFFSET(0) NULL
    ,[sync_success_utc] DATETIMEOFFSET(0) NULL
    ,[sync_invalid_utc] DATETIMEOFFSET(0) NULL
    ,[sync_attempt_utc] DATETIMEOFFSET(0) NULL
    ,[sync_agent] NVARCHAR(50) NULL
    ,[sync_log] NVARCHAR(MAX) NULL
  ,CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED 
  (
	  [account_id] ASC
  )
)

GO


CREATE TABLE [dbo].[Asset] (
	 [asset_id] uniqueidentifier NOT NULL
    ,[type] int NOT NULL
    ,[available] bit NOT NULL
    ,[resize_required] bit NOT NULL
    ,[encode_required] bit NOT NULL
    ,[resize_processing] bit NOT NULL
    ,[encode_processing] bit NOT NULL
    ,[thumb_small_dimensions] nvarchar(10) NULL
    ,[thumb_medium_dimensions] nvarchar(10) NULL
    ,[thumb_large_dimensions] nvarchar(10) NULL
    ,[resize_status] nvarchar(50) NULL
    ,[resize_attempts] int NOT NULL
    ,[resize_attempt_utc] datetimeoffset(0) NULL
    ,[encode_identifier] nvarchar(50) NULL
    ,[encode_status] nvarchar(50) NULL
    ,[raw_url] nvarchar(512) NULL
    ,[public_url] nvarchar(512) NULL
    ,[thumb_small_url] nvarchar(512) NULL
    ,[thumb_medium_url] nvarchar(512) NULL
    ,[thumb_large_url] nvarchar(512) NULL
    ,[encode_log] nvarchar(max) NULL
    ,[resize_log] nvarchar(max) NULL
    ,[dependencies] int NOT NULL
    ,[encode_attempts] int NOT NULL
    ,[encode_attempt_utc] datetimeoffset(0) NULL
    ,[resize_mode] nvarchar(20) NULL
    ,[created_utc] DATETIMEOFFSET(0) NOT NULL
    ,[updated_utc] DATETIMEOFFSET(0) NOT NULL
  ,CONSTRAINT [PK_Asset] PRIMARY KEY CLUSTERED 
  (
	  [asset_id] ASC
  )
)

GO


CREATE TABLE [dbo].[Post] (
	 [post_id] uniqueidentifier NOT NULL
    ,[account_id] uniqueidentifier NOT NULL
    ,[stamp_utc] datetimeoffset(0) NOT NULL
    ,[body] nvarchar(max) NULL
    ,[remark_total] int NOT NULL
    ,[created_utc] DATETIMEOFFSET(0) NOT NULL
    ,[updated_utc] DATETIMEOFFSET(0) NOT NULL
    ,[deleted_utc] DATETIMEOFFSET(0) NULL
	,[sync_hydrate_utc] DATETIMEOFFSET(0) NULL
    ,[sync_success_utc] DATETIMEOFFSET(0) NULL
    ,[sync_invalid_utc] DATETIMEOFFSET(0) NULL
    ,[sync_attempt_utc] DATETIMEOFFSET(0) NULL
    ,[sync_agent] NVARCHAR(50) NULL
    ,[sync_log] NVARCHAR(MAX) NULL
  ,CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED 
  (
	  [post_id] ASC
  )
)

GO


CREATE TABLE [dbo].[Remark] (
	 [remark_id] uniqueidentifier NOT NULL
    ,[post_id] uniqueidentifier NOT NULL
    ,[account_id] uniqueidentifier NOT NULL
    ,[stamp_utc] datetimeoffset(0) NOT NULL
    ,[text] nvarchar(max) NULL
    ,[created_utc] DATETIMEOFFSET(0) NOT NULL
    ,[updated_utc] DATETIMEOFFSET(0) NOT NULL
    ,[deleted_utc] DATETIMEOFFSET(0) NULL
	,[sync_hydrate_utc] DATETIMEOFFSET(0) NULL
    ,[sync_success_utc] DATETIMEOFFSET(0) NULL
    ,[sync_invalid_utc] DATETIMEOFFSET(0) NULL
    ,[sync_attempt_utc] DATETIMEOFFSET(0) NULL
    ,[sync_agent] NVARCHAR(50) NULL
    ,[sync_log] NVARCHAR(MAX) NULL
  ,CONSTRAINT [PK_Remark] PRIMARY KEY CLUSTERED 
  (
	  [remark_id] ASC
  )
)

GO


-- </Tables> --------------------------------------------------------------------


-- <Procedures> --------------------------------------------------------------------

CREATE PROCEDURE [dbo].[spIndex_InvalidateAll]
AS

   UPDATE [dbo].[Account] SET [sync_success_utc] = NULL, [sync_log] = 'invalidateall'

   UPDATE [dbo].[Post] SET [sync_success_utc] = NULL, [sync_log] = 'invalidateall'

   UPDATE [dbo].[Remark] SET [sync_success_utc] = NULL, [sync_log] = 'invalidateall'


GO

CREATE PROCEDURE [dbo].[spIndexHydrate_InvalidateAll]
AS

   UPDATE [dbo].[Account] SET [sync_hydrate_utc] = NULL

   UPDATE [dbo].[Post] SET [sync_hydrate_utc] = NULL

   UPDATE [dbo].[Remark] SET [sync_hydrate_utc] = NULL


GO


CREATE PROCEDURE [dbo].[spIndex_InvalidateAggregates]
AS

	UPDATE [dbo].[Post] SET [sync_success_utc] = NULL

GO


CREATE PROCEDURE [dbo].[spIndexHydrate_InvalidateAggregates]
AS

	UPDATE [dbo].[Post] SET [sync_hydrate_utc] = NULL

GO

CREATE PROCEDURE [dbo].[spIndex_Status]
AS

   SELECT 'Pending Items' as [Pending Items]

      ,(select count(1) from [dbo].[Account] where  [sync_success_utc] IS NULL) as [Account - 10]

      ,(select count(1) from [dbo].[Post] where  [sync_success_utc] IS NULL) as [Post - 20]

      ,(select count(1) from [dbo].[Remark] where  [sync_success_utc] IS NULL) as [Remark - 30]

         

GO

CREATE PROCEDURE [dbo].[spIndexHydrate_Status]
AS

   SELECT 'Pending Items' as [Pending Items]

      ,(select count(1) from [dbo].[Account] where  [sync_hydrate_utc] IS NULL) as [Account - 10]

      ,(select count(1) from [dbo].[Post] where  [sync_hydrate_utc] IS NULL) as [Post - 20]

      ,(select count(1) from [dbo].[Remark] where  [sync_hydrate_utc] IS NULL) as [Remark - 30]

         

GO




CREATE PROCEDURE [dbo].[spAccount_SyncGetInvalid]
	@allowableSecondsToProcessIndex int
    ,@sync_agent nvarchar(50)
AS
  SELECT [account_id]
  FROM [dbo].[Account]
  WHERE [sync_success_utc] IS NULL OR [deleted_utc] > [sync_success_utc]  OR [updated_utc] > [sync_success_utc]
  AND ISNULL([sync_agent],'') = ISNULL(@sync_agent,'')
  ORDER BY  -- oldest attempt, not attempted, failed -> then by change date  
	CASE WHEN NOT [sync_attempt_utc] IS NULL AND DATEDIFF(second,[sync_attempt_utc], GETUTCDATE()) > @allowableSecondsToProcessIndex  
			THEN 0 -- oldest in queue
		WHEN [sync_attempt_utc] IS NULL 
			THEN 1  -- synch is null , freshly invalidated 
		ELSE  2-- recently failed
	END asc
	,[sync_invalid_utc] asc

GO

CREATE PROCEDURE [dbo].[spAccount_SyncUpdate]  
	 @account_id uniqueidentifier,  
	 @sync_success bit,  
	 @sync_success_utc datetimeoffset(0),  
	 @sync_log nvarchar(MAX)  
AS  
BEGIN 
	IF (@sync_success = 1)   
	BEGIN  
		-- ON SUCCESSFUL, SET SYNCH DATE
		UPDATE [dbo].[Account]
		SET [sync_success_utc] = @sync_success_utc
			,[sync_attempt_utc] = NULL
			,[sync_invalid_utc] = NULL
			,[sync_log] = @sync_log
		WHERE [account_id] = @account_id
		AND [sync_success_utc] IS NULL
		AND (([sync_invalid_utc] IS NULL) OR ([sync_invalid_utc] <= @sync_success_utc))
	END
	ELSE
	BEGIN
		-- ON FAILED, SET SYNCH "ATTEMPT" DATE
		UPDATE [dbo].[Account]
		SET [sync_attempt_utc] = GETUTCDATE()
			,[sync_log] = @sync_log
		WHERE [account_id] = @account_id
		AND [sync_success_utc] IS NULL
	END  
END

GO

CREATE PROCEDURE [dbo].[spAccount_HydrateSyncGetInvalid]
	@allowableSecondsToProcessIndex int
    ,@sync_agent nvarchar(50) -- not used yet
AS
  SELECT [account_id]
  FROM [dbo].[Account]
  WHERE [sync_hydrate_utc] IS NULL
  ORDER BY [sync_invalid_utc] asc

GO

CREATE PROCEDURE [dbo].[spAccount_HydrateSyncUpdate]  
	 @account_id uniqueidentifier,  
	 @sync_success bit,  
	 @sync_hydrate_utc datetimeoffset(0),  
	 @sync_log nvarchar(MAX)   -- not used yet
AS  
BEGIN 
	IF (@sync_success = 1)   
	BEGIN  
		-- ON SUCCESSFUL, SET SYNC DATE
		UPDATE [dbo].[Account]
		SET [sync_hydrate_utc] = @sync_hydrate_utc
		WHERE [account_id] = @account_id
		AND [sync_hydrate_utc] IS NULL
	END
	ELSE
	BEGIN
		-- ON FAILED, ADD TO LOG
		UPDATE [dbo].[Account]
		SET [sync_log] = @sync_log
		WHERE [account_id] = @account_id
		AND [sync_hydrate_utc] IS NULL
	END  
END

GO

CREATE PROCEDURE [dbo].[spPost_SyncGetInvalid]
	@allowableSecondsToProcessIndex int
    ,@sync_agent nvarchar(50)
AS
  SELECT [post_id]
  FROM [dbo].[Post]
  WHERE [sync_success_utc] IS NULL OR [deleted_utc] > [sync_success_utc]  OR [updated_utc] > [sync_success_utc]
  AND ISNULL([sync_agent],'') = ISNULL(@sync_agent,'')
  ORDER BY  -- oldest attempt, not attempted, failed -> then by change date  
	CASE WHEN NOT [sync_attempt_utc] IS NULL AND DATEDIFF(second,[sync_attempt_utc], GETUTCDATE()) > @allowableSecondsToProcessIndex  
			THEN 0 -- oldest in queue
		WHEN [sync_attempt_utc] IS NULL 
			THEN 1  -- synch is null , freshly invalidated 
		ELSE  2-- recently failed
	END asc
	,[sync_invalid_utc] asc

GO

CREATE PROCEDURE [dbo].[spPost_SyncUpdate]  
	 @post_id uniqueidentifier,  
	 @sync_success bit,  
	 @sync_success_utc datetimeoffset(0),  
	 @sync_log nvarchar(MAX)  
AS  
BEGIN 
	IF (@sync_success = 1)   
	BEGIN  
		-- ON SUCCESSFUL, SET SYNCH DATE
		UPDATE [dbo].[Post]
		SET [sync_success_utc] = @sync_success_utc
			,[sync_attempt_utc] = NULL
			,[sync_invalid_utc] = NULL
			,[sync_log] = @sync_log
		WHERE [post_id] = @post_id
		AND [sync_success_utc] IS NULL
		AND (([sync_invalid_utc] IS NULL) OR ([sync_invalid_utc] <= @sync_success_utc))
	END
	ELSE
	BEGIN
		-- ON FAILED, SET SYNCH "ATTEMPT" DATE
		UPDATE [dbo].[Post]
		SET [sync_attempt_utc] = GETUTCDATE()
			,[sync_log] = @sync_log
		WHERE [post_id] = @post_id
		AND [sync_success_utc] IS NULL
	END  
END

GO

CREATE PROCEDURE [dbo].[spPost_HydrateSyncGetInvalid]
	@allowableSecondsToProcessIndex int
    ,@sync_agent nvarchar(50) -- not used yet
AS
  SELECT [post_id]
  FROM [dbo].[Post]
  WHERE [sync_hydrate_utc] IS NULL
  ORDER BY [sync_invalid_utc] asc

GO

CREATE PROCEDURE [dbo].[spPost_HydrateSyncUpdate]  
	 @post_id uniqueidentifier,  
	 @sync_success bit,  
	 @sync_hydrate_utc datetimeoffset(0),  
	 @sync_log nvarchar(MAX)   -- not used yet
AS  
BEGIN 
	IF (@sync_success = 1)   
	BEGIN  
		-- ON SUCCESSFUL, SET SYNC DATE
		UPDATE [dbo].[Post]
		SET [sync_hydrate_utc] = @sync_hydrate_utc
		WHERE [post_id] = @post_id
		AND [sync_hydrate_utc] IS NULL
	END
	ELSE
	BEGIN
		-- ON FAILED, ADD TO LOG
		UPDATE [dbo].[Post]
		SET [sync_log] = @sync_log
		WHERE [post_id] = @post_id
		AND [sync_hydrate_utc] IS NULL
	END  
END

GO

CREATE PROCEDURE [dbo].[spRemark_SyncGetInvalid]
	@allowableSecondsToProcessIndex int
    ,@sync_agent nvarchar(50)
AS
  SELECT [remark_id]
  FROM [dbo].[Remark]
  WHERE [sync_success_utc] IS NULL OR [deleted_utc] > [sync_success_utc]  OR [updated_utc] > [sync_success_utc]
  AND ISNULL([sync_agent],'') = ISNULL(@sync_agent,'')
  ORDER BY  -- oldest attempt, not attempted, failed -> then by change date  
	CASE WHEN NOT [sync_attempt_utc] IS NULL AND DATEDIFF(second,[sync_attempt_utc], GETUTCDATE()) > @allowableSecondsToProcessIndex  
			THEN 0 -- oldest in queue
		WHEN [sync_attempt_utc] IS NULL 
			THEN 1  -- synch is null , freshly invalidated 
		ELSE  2-- recently failed
	END asc
	,[sync_invalid_utc] asc

GO

CREATE PROCEDURE [dbo].[spRemark_SyncUpdate]  
	 @remark_id uniqueidentifier,  
	 @sync_success bit,  
	 @sync_success_utc datetimeoffset(0),  
	 @sync_log nvarchar(MAX)  
AS  
BEGIN 
	IF (@sync_success = 1)   
	BEGIN  
		-- ON SUCCESSFUL, SET SYNCH DATE
		UPDATE [dbo].[Remark]
		SET [sync_success_utc] = @sync_success_utc
			,[sync_attempt_utc] = NULL
			,[sync_invalid_utc] = NULL
			,[sync_log] = @sync_log
		WHERE [remark_id] = @remark_id
		AND [sync_success_utc] IS NULL
		AND (([sync_invalid_utc] IS NULL) OR ([sync_invalid_utc] <= @sync_success_utc))
	END
	ELSE
	BEGIN
		-- ON FAILED, SET SYNCH "ATTEMPT" DATE
		UPDATE [dbo].[Remark]
		SET [sync_attempt_utc] = GETUTCDATE()
			,[sync_log] = @sync_log
		WHERE [remark_id] = @remark_id
		AND [sync_success_utc] IS NULL
	END  
END

GO

CREATE PROCEDURE [dbo].[spRemark_HydrateSyncGetInvalid]
	@allowableSecondsToProcessIndex int
    ,@sync_agent nvarchar(50) -- not used yet
AS
  SELECT [remark_id]
  FROM [dbo].[Remark]
  WHERE [sync_hydrate_utc] IS NULL
  ORDER BY [sync_invalid_utc] asc

GO

CREATE PROCEDURE [dbo].[spRemark_HydrateSyncUpdate]  
	 @remark_id uniqueidentifier,  
	 @sync_success bit,  
	 @sync_hydrate_utc datetimeoffset(0),  
	 @sync_log nvarchar(MAX)   -- not used yet
AS  
BEGIN 
	IF (@sync_success = 1)   
	BEGIN  
		-- ON SUCCESSFUL, SET SYNC DATE
		UPDATE [dbo].[Remark]
		SET [sync_hydrate_utc] = @sync_hydrate_utc
		WHERE [remark_id] = @remark_id
		AND [sync_hydrate_utc] IS NULL
	END
	ELSE
	BEGIN
		-- ON FAILED, ADD TO LOG
		UPDATE [dbo].[Remark]
		SET [sync_log] = @sync_log
		WHERE [remark_id] = @remark_id
		AND [sync_hydrate_utc] IS NULL
	END  
END

GO

-- <Procedures> --------------------------------------------------------------------


-- <Foreign Keys> --------------------------------------------------------------------

ALTER TABLE [dbo].[Remark] WITH CHECK ADD  CONSTRAINT [FK_Remark_Post_post_id] FOREIGN KEY([post_id])
REFERENCES [dbo].[Post] ([post_id])
GO

ALTER TABLE [dbo].[Remark] WITH CHECK ADD  CONSTRAINT [FK_Remark_Account_account_id] FOREIGN KEY([account_id])
REFERENCES [dbo].[Account] ([account_id])
GO

ALTER TABLE [dbo].[Post] WITH CHECK ADD  CONSTRAINT [FK_Post_Account_account_id] FOREIGN KEY([account_id])
REFERENCES [dbo].[Account] ([account_id])
GO

-- </Foreign Keys> --------------------------------------------------------------------


-- <Unique Keys> --------------------------------------------------------------------


IF OBJECT_ID('dbo.UK_account_key', 'UQ') IS NOT NULL BEGIN -- multiple passes because of script limitations, thats fine. :)
	ALTER TABLE [dbo].[Account] 
		DROP CONSTRAINT UK_account_key
END
ALTER TABLE [dbo].[Account] 
   ADD CONSTRAINT UK_account_key UNIQUE ([api_key]); 
GO

-- </Unique Keys> --------------------------------------------------------------------


