<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">

<xsl:for-each select="items/enum">
'''[STARTFILE:<xsl:value-of select="../@projectName"/>.Domain\Domain\<xsl:value-of select="@name"/>.cs]using System;
using System.Text;

namespace <xsl:value-of select="../@projectName"/>.Domain
{
    public enum <xsl:value-of select="@name"/>
    {
        <xsl:for-each select="field"><xsl:if test="position() > 1">,
        </xsl:if><xsl:value-of select="text()"/> = <xsl:value-of select="@value"/></xsl:for-each>
    }
}
'''[ENDFILE]
</xsl:for-each>
  
<xsl:for-each select="items/item">
'''[STARTFILE:<xsl:value-of select="../@projectName"/>.Domain\Domain\<xsl:value-of select="@name"/>.cs]using Codeable.Foundation.Core;
using System;
using System.Collections.Generic;
using System.Text;


namespace <xsl:value-of select="../@projectName"/>.Domain
{
    public partial class <xsl:value-of select="@name"/> : DomainModel
    {	
        public <xsl:value-of select="@name"/>()
        {
				
        }
    
        <xsl:for-each select="field">public <xsl:value-of select="@type"/><xsl:if test="@type!='string' and @isNullable='true'">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> { get; set; }
        </xsl:for-each><xsl:if test="@useIndex='true'">public DateTime created_utc { get; set; }
        public DateTime updated_utc { get; set; }
        public DateTime? deleted_utc { get; set; }
        public DateTime? sync_success_utc { get; set; }
        public DateTime? sync_invalid_utc { get; set; }
        public DateTime? sync_attempt_utc { get; set; }
        public string sync_agent { get; set; }
        public string sync_log { get; set; }</xsl:if><xsl:if test="@trackUpdates='true'">public DateTime created_utc { get; set; }
        public DateTime updated_utc { get; set; }</xsl:if>
        
        <xsl:for-each select="field[string-length(@derivedProperty)>0]">
        public DerivedProperty&lt;<xsl:value-of select="@foreignKey"/>&gt; Related<xsl:value-of select="@friendlyName"/> { get; set; }
        </xsl:for-each>
        <xsl:variable name="currentKey"><xsl:value-of select="@name"/></xsl:variable>
        <xsl:for-each select="../item/field[string-length(@derivedParentProperty)>0 and @foreignKey=$currentKey]">
        public DerivedProperty&lt;List&lt;<xsl:value-of select="../@name"/>&gt;&gt; Related<xsl:value-of select="../@name"/> { get; set; }
        </xsl:for-each>
	}
}

'''[ENDFILE]
'''[STARTFILE:<xsl:value-of select="../@projectName"/>.SDK.Shared\Models\<xsl:value-of select="@name"/>.cs]using System;
using System.Collections.Generic;
using System.Text;

namespace <xsl:value-of select="../@projectName"/>.SDK.Models
{
    public partial class <xsl:value-of select="@name"/> : <xsl:choose><xsl:when test="@sdkBase='true'"><xsl:value-of select="@name"/>Base</xsl:when><xsl:otherwise>SDKModel</xsl:otherwise></xsl:choose>
    {	
        public <xsl:value-of select="@name"/>()
        {
				
        }
    
        <xsl:for-each select="field[not(@sdkHidden='true') and not(@derivedProperty='true') and not(@type='DateTimeOffset')]">public virtual <xsl:value-of select="@type"/><xsl:if test="@type!='string' and @isNullable='true'">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> { get; set; }
        </xsl:for-each>
        
        <xsl:for-each select="field[not(@sdkHidden='true') and not(@derivedProperty='true') and @type='DateTimeOffset']">
        #if WEB
        public virtual DateTime<xsl:if test="@type!='string' and @isNullable='true'">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> { get; set; }
        #else
        public virtual DateTimeOffset<xsl:if test="@type!='string' and @isNullable='true'">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> { get; set; }
        #endif
        </xsl:for-each>
        
        <xsl:if test="count(indexfield)>0">
        //&lt;IndexOnly&gt;
        
        <xsl:for-each select="indexfield">public <xsl:value-of select="@type"/><xsl:if test="@type!='string' and @isNullable='true'">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> { get; set; }
        </xsl:for-each>
        //&lt;/IndexOnly&gt;</xsl:if>
	}
}

'''[ENDFILE]
</xsl:for-each>

</xsl:template>

</xsl:stylesheet>