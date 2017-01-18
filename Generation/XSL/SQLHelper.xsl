<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">

'''[STARTFILE:<xsl:value-of select="../@folderName"/>\Scripts\SQL\create_helper.sql]
-- &lt;Tables&gt; --------------------------------------------------------------------
<xsl:for-each select="items/item">
CREATE TABLE [<xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="../@schema"/></xsl:call-template>].[<xsl:value-of select="@name" />] (
	 <xsl:for-each select="field[not(@derivedProperty='true')]"><xsl:if test="position() > 1">,</xsl:if>[<xsl:value-of select="text()" />] <xsl:value-of select="@dbType" /><xsl:if test="not(@isNullable='true')"> NOT</xsl:if> NULL
    </xsl:for-each><xsl:if test="@useIndex='true'">,[created_utc] DATETIMEOFFSET(0) NOT NULL
    ,[updated_utc] DATETIMEOFFSET(0) NOT NULL
    ,[deleted_utc] DATETIMEOFFSET(0) NULL
	,[sync_hydrate_utc] DATETIMEOFFSET(0) NULL
    ,[sync_success_utc] DATETIMEOFFSET(0) NULL
    ,[sync_invalid_utc] DATETIMEOFFSET(0) NULL
    ,[sync_attempt_utc] DATETIMEOFFSET(0) NULL
    ,[sync_agent] NVARCHAR(50) NULL
    ,[sync_log] NVARCHAR(MAX) NULL</xsl:if><xsl:if test="@trackUpdates='true'">,[created_utc] DATETIMEOFFSET(0) NOT NULL
    ,[updated_utc] DATETIMEOFFSET(0) NOT NULL</xsl:if>
  ,CONSTRAINT [PK_<xsl:value-of select="@name" />] PRIMARY KEY CLUSTERED 
  (
	  [<xsl:value-of select="field[1]"/>] ASC
  )
)

GO

</xsl:for-each>
-- &lt;/Tables&gt; --------------------------------------------------------------------


-- &lt;Procedures&gt; --------------------------------------------------------------------

CREATE PROCEDURE [dbo].[spIndex_InvalidateAll]
AS
<xsl:for-each select="items/item[@useIndex='true']">
   UPDATE [dbo].[<xsl:value-of select="@name" />] SET [sync_success_utc] = NULL, [sync_log] = 'invalidateall'
</xsl:for-each>

GO

CREATE PROCEDURE [dbo].[spIndexHydrate_InvalidateAll]
AS
<xsl:for-each select="items/item[@useIndex='true']">
   UPDATE [dbo].[<xsl:value-of select="@name" />] SET [sync_hydrate_utc] = NULL
</xsl:for-each>

GO


CREATE PROCEDURE [dbo].[spIndex_InvalidateAggregates]
AS
<xsl:for-each select="items/item[@useIndex='true']">
	<xsl:if test="@manualAggregate='true' or count(field[@computedBy='Sum' or @computedBy='Count']) > 0 or count(indexfield[@computedBy='Sum' or @computedBy='Count']) > 0">
	UPDATE [dbo].[<xsl:value-of select="@name" />] SET [sync_success_utc] = NULL</xsl:if>
</xsl:for-each>

GO


CREATE PROCEDURE [dbo].[spIndexHydrate_InvalidateAggregates]
AS
<xsl:for-each select="items/item[@useIndex='true']">
	<xsl:if test="@manualAggregate='true' or count(field[@computedBy='Sum' or @computedBy='Count']) > 0 or count(indexfield[@computedBy='Sum' or @computedBy='Count']) > 0">
	UPDATE [dbo].[<xsl:value-of select="@name" />] SET [sync_hydrate_utc] = NULL</xsl:if>
</xsl:for-each>

GO

CREATE PROCEDURE [dbo].[spIndex_Status]
AS

   SELECT 'Pending Items' as [Pending Items]
<xsl:for-each select="items/item[@useIndex='true']">
<xsl:sort select="@indexPriority" data-type="number" order="ascending"/>
      ,(select count(1) from [dbo].[<xsl:value-of select="@name" />] where  [sync_success_utc] IS NULL) as [<xsl:value-of select="@name" /> - <xsl:value-of select="@indexPriority" />]
</xsl:for-each>
         

GO

CREATE PROCEDURE [dbo].[spIndexHydrate_Status]
AS

   SELECT 'Pending Items' as [Pending Items]
<xsl:for-each select="items/item[@useIndex='true']">
<xsl:sort select="@indexPriority" data-type="number" order="ascending"/>
      ,(select count(1) from [dbo].[<xsl:value-of select="@name" />] where  [sync_hydrate_utc] IS NULL) as [<xsl:value-of select="@name" /> - <xsl:value-of select="@indexPriority" />]
</xsl:for-each>
         

GO



<xsl:for-each select="items/item[@useIndex='true']">
CREATE PROCEDURE [dbo].[sp<xsl:value-of select="@name" />_SyncGetInvalid]
	@allowableSecondsToProcessIndex int
    ,@sync_agent nvarchar(50)
AS
  SELECT [<xsl:value-of select="field[1]"/>]
  FROM [dbo].[<xsl:value-of select="@name" />]
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

CREATE PROCEDURE [dbo].[sp<xsl:value-of select="@name" />_SyncUpdate]  
	 @<xsl:value-of select="field[1]"/> uniqueidentifier,  
	 @sync_success bit,  
	 @sync_success_utc datetimeoffset(0),  
	 @sync_log nvarchar(MAX)  
AS  
BEGIN 
	IF (@sync_success = 1)   
	BEGIN  
		-- ON SUCCESSFUL, SET SYNCH DATE
		UPDATE [dbo].[<xsl:value-of select="@name" />]
		SET [sync_success_utc] = @sync_success_utc
			,[sync_attempt_utc] = NULL
			,[sync_invalid_utc] = NULL
			,[sync_log] = @sync_log
		WHERE [<xsl:value-of select="field[1]"/>] = @<xsl:value-of select="field[1]"/>
		AND [sync_success_utc] IS NULL
		AND (([sync_invalid_utc] IS NULL) OR ([sync_invalid_utc] &lt;= @sync_success_utc))
	END
	ELSE
	BEGIN
		-- ON FAILED, SET SYNCH "ATTEMPT" DATE
		UPDATE [dbo].[<xsl:value-of select="@name" />]
		SET [sync_attempt_utc] = GETUTCDATE()
			,[sync_log] = @sync_log
		WHERE [<xsl:value-of select="field[1]"/>] = @<xsl:value-of select="field[1]"/>
		AND [sync_success_utc] IS NULL
	END  
END

GO

CREATE PROCEDURE [dbo].[sp<xsl:value-of select="@name" />_HydrateSyncGetInvalid]
	@allowableSecondsToProcessIndex int
    ,@sync_agent nvarchar(50) -- not used yet
AS
  SELECT [<xsl:value-of select="field[1]"/>]
  FROM [dbo].[<xsl:value-of select="@name" />]
  WHERE [sync_hydrate_utc] IS NULL
  ORDER BY [sync_invalid_utc] asc

GO

CREATE PROCEDURE [dbo].[sp<xsl:value-of select="@name" />_HydrateSyncUpdate]  
	 @<xsl:value-of select="field[1]"/> uniqueidentifier,  
	 @sync_success bit,  
	 @sync_hydrate_utc datetimeoffset(0),  
	 @sync_log nvarchar(MAX)   -- not used yet
AS  
BEGIN 
	IF (@sync_success = 1)   
	BEGIN  
		-- ON SUCCESSFUL, SET SYNC DATE
		UPDATE [dbo].[<xsl:value-of select="@name" />]
		SET [sync_hydrate_utc] = @sync_hydrate_utc
		WHERE [<xsl:value-of select="field[1]"/>] = @<xsl:value-of select="field[1]"/>
		AND [sync_hydrate_utc] IS NULL
	END
	ELSE
	BEGIN
		-- ON FAILED, ADD TO LOG
		UPDATE [dbo].[<xsl:value-of select="@name" />]
		SET [sync_log] = @sync_log
		WHERE [<xsl:value-of select="field[1]"/>] = @<xsl:value-of select="field[1]"/>
		AND [sync_hydrate_utc] IS NULL
	END  
END

GO
</xsl:for-each>
-- &lt;Procedures&gt; --------------------------------------------------------------------


-- &lt;Foreign Keys&gt; --------------------------------------------------------------------
<xsl:for-each select="items/item">
<xsl:sort select="position()" data-type="number" order="descending"/>
<xsl:for-each select="field[@foreignKey]">
ALTER TABLE [dbo].[<xsl:value-of select="../@name" />] WITH CHECK ADD  CONSTRAINT [FK_<xsl:value-of select="../@name" />_<xsl:value-of select="@foreignKey" />_<xsl:value-of select="text()" />] FOREIGN KEY([<xsl:value-of select="text()" />])
REFERENCES [dbo].[<xsl:value-of select="@foreignKey" />] ([<xsl:value-of select="@foreignKeyField" />])
GO
</xsl:for-each>
</xsl:for-each>
-- &lt;/Foreign Keys&gt; --------------------------------------------------------------------


-- &lt;Unique Keys&gt; --------------------------------------------------------------------
<xsl:for-each select="items/item">
<xsl:sort select="position()" data-type="number" order="descending"/>
<xsl:for-each select="field[string-length(@ukGroup)>0]">
<xsl:variable name="groupName" select="@ukGroup"/>

IF OBJECT_ID('dbo.UK_<xsl:value-of select="$groupName" />', 'UQ') IS NOT NULL BEGIN -- multiple passes because of script limitations, thats fine. :)
	ALTER TABLE [dbo].[<xsl:value-of select="../@name" />] 
		DROP CONSTRAINT UK_<xsl:value-of select="$groupName" />
END
ALTER TABLE [dbo].[<xsl:value-of select="../@name" />] 
   ADD CONSTRAINT UK_<xsl:value-of select="$groupName" /> UNIQUE (<xsl:for-each select="../field[@ukGroup=$groupName]"><xsl:if test="position()>1">,</xsl:if>[<xsl:value-of select="text()" />]</xsl:for-each>); 
GO
</xsl:for-each>
</xsl:for-each>
-- &lt;/Unique Keys&gt; --------------------------------------------------------------------


'''[ENDFILE]

'''[STARTFILE:<xsl:value-of select="../@folderName"/>\Scripts\SQL\delete_helper.sql]

-- &lt;Foreign Keys&gt; --------------------------------------------------------------------
<xsl:for-each select="items/item">
<xsl:sort select="position()" data-type="number" order="descending"/>
<xsl:for-each select="field[@foreignKey]">
ALTER TABLE [dbo].[<xsl:value-of select="../@name" />] DROP  [FK_<xsl:value-of select="../@name" />_<xsl:value-of select="@foreignKey" />_<xsl:value-of select="text()" />]
</xsl:for-each>
GO
</xsl:for-each>
-- &lt;/Foreign Keys&gt; --------------------------------------------------------------------



-- &lt;Unique Keys&gt; --------------------------------------------------------------------
<xsl:for-each select="items/item">
<xsl:sort select="position()" data-type="number" order="descending"/>
<xsl:for-each select="field[string-length(@ukGroup)>0]">
<xsl:variable name="groupName" select="@ukGroup"/>
IF OBJECT_ID('dbo.UK_<xsl:value-of select="$groupName" />', 'UQ') IS NOT NULL BEGIN -- multiple passes because of script limitations, thats fine. :)
	ALTER TABLE [dbo].[<xsl:value-of select="../@name" />] 
		DROP CONSTRAINT UK_<xsl:value-of select="$groupName" />
END
GO

</xsl:for-each>
</xsl:for-each>
-- &lt;/Unique Keys&gt; --------------------------------------------------------------------


-- &lt;Tables&gt; --------------------------------------------------------------------
<xsl:for-each select="items/item">
<xsl:sort select="position()" data-type="number" order="descending"/>
DROP TABLE [<xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="../@schema"/></xsl:call-template>].[<xsl:value-of select="@name" />]
GO
</xsl:for-each>
-- &lt;/Tables&gt; --------------------------------------------------------------------

-- &lt;Procedures&gt; --------------------------------------------------------------------
<xsl:for-each select="items/item[@useIndex='true']">

DROP PROCEDURE [dbo].[sp<xsl:value-of select="@name" />_SyncUpdate]
GO

DROP PROCEDURE [dbo].[sp<xsl:value-of select="@name" />_SyncGetInvalid]
GO

DROP PROCEDURE [dbo].[sp<xsl:value-of select="@name" />_HydrateSyncUpdate]
GO

DROP PROCEDURE [dbo].[sp<xsl:value-of select="@name" />_HydrateSyncGetInvalid]
GO


</xsl:for-each>

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


-- &lt;Procedures&gt; --------------------------------------------------------------------
'''[ENDFILE]

</xsl:template>
	<xsl:template match="@space"> </xsl:template>
  <xsl:template name="ToLower">
          <xsl:param name="inputString"/>
          <xsl:variable name="smallCase" select="'abcdefghijklmnopqrstuvwxyz'"/>
          <xsl:variable name="upperCase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
          <xsl:value-of select="translate($inputString,$upperCase,$smallCase)"/>
  </xsl:template>
 <xsl:template name="ToUpper">
          <xsl:param name="inputString"/>
          <xsl:variable name="smallCase" select="'abcdefghijklmnopqrstuvwxyz'"/>
          <xsl:variable name="upperCase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
          <xsl:value-of select="translate($inputString,$smallCase,$upperCase)"/>
  </xsl:template>
</xsl:stylesheet>