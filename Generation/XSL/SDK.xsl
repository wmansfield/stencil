<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">

<xsl:for-each select="items/enum">
  <xsl:variable name="name_lowered"><xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="@name"/></xsl:call-template></xsl:variable>
  
'''[STARTFILE:<xsl:value-of select="../@projectName"/>.SDK.Shared\Models\<xsl:value-of select="@name"/>.cs]using System;
using System.Text;

namespace <xsl:value-of select="../@projectName"/>.SDK.Models
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
  <xsl:variable name="name_lowered"><xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="@name"/></xsl:call-template></xsl:variable>
  <xsl:variable name="removePattern2"><xsl:value-of select="@removePattern"/></xsl:variable>
'''[STARTFILE:<xsl:value-of select="../@projectName"/>.SDK.Shared\Endpoints\Core\<xsl:value-of select="@name"/>Endpoint.cs]#if WINDOWS_PHONE_APP
using RestSharp.Portable;
#else
using RestSharp;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="../@projectName"/>.SDK.Models;

namespace <xsl:value-of select="../@projectName"/>.SDK.Endpoints
{
    public partial class <xsl:value-of select="@name"/>Endpoint : EndpointBase
    {
        public <xsl:value-of select="@name"/>Endpoint(<xsl:value-of select="../@projectName"/>SDK api)
            : base(api)
        {

        }
        
        public Task&lt;ItemResult&lt;<xsl:value-of select="@name"/>&gt;&gt; Get<xsl:value-of select="@name"/>Async(<xsl:if test="string-length(@indexParent) > 0"><xsl:for-each select="field[@indexParent='true']">Guid <xsl:value-of select="text()" />, </xsl:for-each></xsl:if>Guid<xsl:text> </xsl:text><xsl:value-of select="field[1]"/>)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "<xsl:value-of select="$name_lowered"/>s/<xsl:if test="string-length(@indexParent) > 0"><xsl:for-each select="field[@indexParent='true']">{<xsl:value-of select="text()" />}/</xsl:for-each></xsl:if>{<xsl:value-of select="field[1]"/>}";
            request.AddUrlSegment("<xsl:value-of select="field[1]"/>",<xsl:text> </xsl:text><xsl:value-of select="field[1]"/>.ToString());
            <xsl:if test="string-length(@indexParent) > 0"><xsl:for-each select="field[@indexParent='true']">request.AddUrlSegment("<xsl:value-of select="text()" />",<xsl:text> </xsl:text><xsl:value-of select="text()" />.ToString());</xsl:for-each></xsl:if>
            return this.Sdk.ExecuteAsync&lt;ItemResult&lt;<xsl:value-of select="@name"/>&gt;&gt;(request);
        }
        
        <xsl:if test="count(field[@searchable='true']) > 0 or count(indexfield[@searchable='true']) > 0">public Task&lt;ListResult&lt;<xsl:value-of select="@name"/>&gt;&gt; Find(int skip = 0, int take = 10, string keyword = "", string order_by = "", bool descending = false<xsl:for-each select="field[@foreignKey]">, Guid? <xsl:value-of select="text()"/> = null</xsl:for-each><xsl:for-each select="field[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="indexfield[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "<xsl:value-of select="$name_lowered"/>s";
            request.AddParameter("skip", skip);
            request.AddParameter("take", take);
            request.AddParameter("order_by", order_by);
            request.AddParameter("descending", descending);
            request.AddParameter("keyword", keyword);
            <xsl:for-each select="field[@foreignKey]">request.AddParameter("<xsl:value-of select="text()"/>", <xsl:value-of select="text()"/>);
            </xsl:for-each>
            <xsl:for-each select="field[string-length(@searchToggle)>0]">request.AddParameter("<xsl:value-of select="text()"/>", <xsl:value-of select="text()"/>);
            </xsl:for-each>
            <xsl:for-each select="indexfield[string-length(@searchToggle)>0]">request.AddParameter("<xsl:value-of select="text()"/>", <xsl:value-of select="text()"/>);
            </xsl:for-each>
            
            return this.Sdk.ExecuteAsync&lt;ListResult&lt;<xsl:value-of select="@name"/>&gt;&gt;(request);
        }</xsl:if>
        
        <xsl:for-each select="field[@foreignKey]">
        public Task&lt;ListResult&lt;<xsl:value-of select="../@name"/>&gt;&gt; Get<xsl:value-of select="../@name"/>By<xsl:value-of select="@friendlyName" />Async(Guid<xsl:text> </xsl:text><xsl:value-of select="@foreignKeyField"/><xsl:if test="$removePattern2='true'">, bool include_removed</xsl:if>, int skip = 0, int take = 10, string order_by = "", bool descending = false<xsl:if test="string-length(../@pagingWindow)>0">, DateTime? before_<xsl:value-of select="../@pagingWindow" /> = null</xsl:if><xsl:for-each select="../field[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "<xsl:value-of select="$name_lowered"/>s/by_<xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="@friendlyName"/></xsl:call-template>/{<xsl:value-of select="@foreignKeyField"/>}";
            request.AddUrlSegment("<xsl:value-of select="@foreignKeyField"/>",<xsl:text> </xsl:text><xsl:value-of select="@foreignKeyField"/>.ToString());
            request.AddParameter("skip", skip);
            request.AddParameter("take", take);
            request.AddParameter("order_by", order_by);
            request.AddParameter("descending", descending);
            <xsl:if test="string-length(../@pagingWindow)>0">request.AddParameter("before_<xsl:value-of select="../@pagingWindow" />", before_<xsl:value-of select="../@pagingWindow" />);</xsl:if>
            <xsl:if test="$removePattern2='true'">request.AddParameter("include_removed", include_removed);</xsl:if>
            <xsl:for-each select="../field[string-length(@searchToggle)>0]">request.AddParameter("<xsl:value-of select="text()"/>", <xsl:value-of select="text()"/>);
            </xsl:for-each>
            <xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">request.AddParameter("<xsl:value-of select="text()"/>", <xsl:value-of select="text()"/>);
            </xsl:for-each>
            return this.Sdk.ExecuteAsync&lt;ListResult&lt;<xsl:value-of select="../@name"/>&gt;&gt;(request);
        }
        </xsl:for-each>

        public Task&lt;ItemResult&lt;<xsl:value-of select="@name"/>&gt;&gt; Create<xsl:value-of select="@name"/>Async(<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "<xsl:value-of select="$name_lowered"/>s";
            request.AddJsonBody(<xsl:value-of select="$name_lowered"/>);
            return this.Sdk.ExecuteAsync&lt;ItemResult&lt;<xsl:value-of select="@name"/>&gt;&gt;(request);
        }

        public Task&lt;ItemResult&lt;<xsl:value-of select="@name"/>&gt;&gt; Update<xsl:value-of select="@name"/>Async(Guid<xsl:text> </xsl:text><xsl:value-of select="field[1]"/>, <xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>)
        {
            var request = new RestRequest(Method.PUT);
            request.Resource = "<xsl:value-of select="$name_lowered"/>s/{<xsl:value-of select="field[1]"/>}";
            request.AddUrlSegment("<xsl:value-of select="field[1]"/>",<xsl:text> </xsl:text><xsl:value-of select="field[1]"/>.ToString());
            request.AddJsonBody(<xsl:value-of select="$name_lowered"/>);
            return this.Sdk.ExecuteAsync&lt;ItemResult&lt;<xsl:value-of select="@name"/>&gt;&gt;(request);
        }

        <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
        public Task&lt;ActionResult&gt; Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>Async(Guid <xsl:value-of select="../field[1]"/>, int priority)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "<xsl:value-of select="$name_lowered"/>s/{<xsl:value-of select="../field[1]"/>}/update_<xsl:value-of select="text()"/>/{priority}";
            request.AddUrlSegment("<xsl:value-of select="../field[1]"/>", <xsl:value-of select="../field[1]"/>.ToString());
            request.AddUrlSegment("priority", priority.ToString());
            return this.Sdk.ExecuteAsync&lt;ActionResult&gt;(request);
        }
        </xsl:for-each>

        public Task&lt;ActionResult&gt; Delete<xsl:value-of select="@name"/>Async(Guid<xsl:text> </xsl:text><xsl:value-of select="field[1]"/>)
        {
            var request = new RestRequest(Method.DELETE);
            request.Resource = "<xsl:value-of select="$name_lowered"/>s/{<xsl:value-of select="field[1]"/>}";
            request.AddUrlSegment("<xsl:value-of select="field[1]"/>",<xsl:text> </xsl:text><xsl:value-of select="field[1]"/>.ToString());
            return this.Sdk.ExecuteAsync&lt;ActionResult&gt;(request);
        }
    }
}
'''[ENDFILE]

'''[STARTFILE:Plugins\<xsl:value-of select="../@projectName"/>.Plugins.RestApi\Controllers\<xsl:value-of select="@name"/>Controller_Crud.cs]using Codeable.Foundation.Common;
using Codeable.Foundation.Core;
using System;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using sdk = <xsl:value-of select="../@projectName"/>.SDK.Models;
using dm = <xsl:value-of select="../@projectName"/>.Domain;
using <xsl:value-of select="../@projectName"/>.Primary;
using <xsl:value-of select="../@projectName"/>.SDK;
using <xsl:value-of select="../@projectName"/>.Web.Controllers;
using <xsl:value-of select="../@projectName"/>.Web.Security;

namespace <xsl:value-of select="../@projectName"/>.Plugins.RestAPI.Controllers
{
    [ApiKeyHttpAuthorize]
    [RoutePrefix("api/<xsl:value-of select="$name_lowered"/>s")]
    public partial class <xsl:value-of select="@name" />Controller : HealthRestApiController
    {
        public <xsl:value-of select="@name" />Controller(IFoundation foundation)
            : base(foundation, "<xsl:value-of select="@name" />")
        {
        }

        <xsl:choose><xsl:when test="@useIndex">[HttpGet]
        [Route("<xsl:if test="string-length(@indexParent) > 0"><xsl:for-each select="field[@indexParent='true']">{<xsl:value-of select="text()" />}/</xsl:for-each></xsl:if>{<xsl:value-of select="field[1]"/>}")]
        public object GetById(<xsl:if test="string-length(@indexParent) > 0"><xsl:for-each select="field[@indexParent='true']">Guid <xsl:value-of select="text()" />, </xsl:for-each></xsl:if>Guid <xsl:value-of select="field[1]"/>)
        {
            return base.ExecuteFunction&lt;object&gt;("GetById", delegate()
            {
                <xsl:if test="@userSpecificData='account_id'">dm.Account currentAccount = this.GetCurrentAccount();
                </xsl:if>sdk.<xsl:value-of select="@name" /> result = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.GetById(<xsl:if test="string-length(@indexParent) > 0"><xsl:for-each select="field[@indexParent='true']"><xsl:value-of select="text()" />, </xsl:for-each></xsl:if><xsl:value-of select="field[1]"/><xsl:if test="@userSpecificData='account_id'">, currentAccount.account_id</xsl:if>);
                if (result == null)
                {
                    return Http404("<xsl:value-of select="@name" />");
                }

                <xsl:if test="count(field[text()='faction_id'])>0">this.Analytics.TrackFactionAccess(this.GetCurrentAccount(), result.faction_id);</xsl:if>

                return base.Http200(new ItemResult&lt;sdk.<xsl:value-of select="@name" />&gt;()
                {
                    success = true, 
                    item = result
                });
            });
        }
        
        <xsl:if test="count(field[@searchable='true']) > 0 or count(indexfield[@searchable='true']) > 0">
        [HttpGet]
        [Route("")]
        public object Find(int skip = 0, int take = 10, string order_by = "", bool descending = false, string keyword = ""<xsl:for-each select="field[@foreignKey]">, Guid? <xsl:value-of select="text()"/> = null</xsl:for-each><xsl:for-each select="field[string-length(@searchToggle)>0]">, <xsl:variable name="searchType"><xsl:value-of select="@type" /></xsl:variable><xsl:if test="/items/enum[@name=$searchType]">sdk.</xsl:if><xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:if test="/items/enum[@name=$searchType] and @searchToggle!='null'">sdk.</xsl:if><xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="indexfield[string-length(@searchToggle)>0]">, <xsl:variable name="searchType"><xsl:value-of select="@type" /></xsl:variable><xsl:if test="/items/enum[@name=$searchType]">sdk.</xsl:if><xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:if test="/items/enum[@name=$searchType] and @searchToggle!='null'">sdk.</xsl:if><xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            return base.ExecuteFunction&lt;object&gt;("Find", delegate()
            {
                <xsl:if test="count(field[@foreignKeyField='faction_id'])>0">this.Analytics.TrackFactionAccess(this.GetCurrentAccount(), faction_id);
                </xsl:if>
                <xsl:if test="@userSpecificData='account_id'">dm.Account currentAccount = this.GetCurrentAccount();</xsl:if>
                ListResult&lt;sdk.<xsl:value-of select="@name" />&gt; result = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.Find(<xsl:if test="@userSpecificData='account_id'">currentAccount.account_id, </xsl:if>skip, take, keyword, order_by, descending<xsl:for-each select="field[@foreignKey]">, <xsl:value-of select="text()"/></xsl:for-each><xsl:for-each select="field[string-length(@searchToggle)>0]">, <xsl:value-of select="text()"/></xsl:for-each><xsl:for-each select="indexfield[string-length(@searchToggle)>0]">, <xsl:value-of select="text()"/></xsl:for-each>);
                result.success = true;
                return base.Http200(result);
            });
        }</xsl:if>
        
        <xsl:for-each select="field[@foreignKey]">
        <xsl:variable name="foreignKey"><xsl:value-of select="@foreignKey"/></xsl:variable>
        [HttpGet]
        [Route("by_<xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="@friendlyName"/></xsl:call-template>/{<xsl:value-of select="@foreignKeyField"/>}")]
        public object GetBy<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="@foreignKeyField"/>, int skip = 0, int take = 10, string order_by = "", bool descending = false<xsl:if test="string-length(../@pagingWindow)>0">, DateTime? before_<xsl:value-of select="../@pagingWindow" /> = null</xsl:if><xsl:for-each select="../field[string-length(@searchToggle)>0]">, <xsl:variable name="searchType"><xsl:value-of select="@type" /></xsl:variable><xsl:if test="/items/enum[@name=$searchType]">sdk.</xsl:if><xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:if test="/items/enum[@name=$searchType] and @searchToggle!='null'">sdk.</xsl:if><xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">, <xsl:variable name="searchType"><xsl:value-of select="@type" /></xsl:variable><xsl:if test="/items/enum[@name=$searchType]">sdk.</xsl:if><xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:if test="/items/enum[@name=$searchType] and @searchToggle!='null'">sdk.</xsl:if><xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            return base.ExecuteFunction&lt;object&gt;("GetBy<xsl:value-of select="@friendlyName" />", delegate ()
            {
                
                <xsl:if test="not(@foreignKeyField='faction_id') and ../../item[@name=$foreignKey]/field/text()='faction_id'">Guid? faction_id = null;
                dm.<xsl:value-of select="$foreignKey"/> reference<xsl:value-of select="$foreignKey"/> = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="$foreignKey"/></xsl:call-template>.GetById(<xsl:value-of select="@foreignKeyField"/>);
                if(reference<xsl:value-of select="$foreignKey"/> != null)
                {
                    faction_id = reference<xsl:value-of select="$foreignKey"/>.faction_id;
                }
                this.Analytics.TrackFactionAccess(this.GetCurrentAccount(), faction_id);</xsl:if>
                <xsl:if test="../@userSpecificData='account_id'">dm.Account currentAccount = this.GetCurrentAccount();</xsl:if>
                ListResult&lt;sdk.<xsl:value-of select="../@name"/>&gt; result = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>.GetBy<xsl:value-of select="@friendlyName" />(<xsl:value-of select="@foreignKeyField"/>, skip, take, order_by, descending<xsl:if test="string-length(../@pagingWindow)>0">, before_<xsl:value-of select="../@pagingWindow" /></xsl:if><xsl:if test="../@userSpecificData='account_id'">, currentAccount.account_id</xsl:if><xsl:for-each select="../field[string-length(@searchToggle)>0]">, <xsl:value-of select="text()"/></xsl:for-each><xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">, <xsl:value-of select="text()"/></xsl:for-each>);
                result.success = true;
                return base.Http200(result);
            });
        }
        </xsl:for-each>
        
        </xsl:when>
        <xsl:otherwise>[HttpGet]
        [Route("{<xsl:value-of select="field[1]"/>}")]
        public object GetById(Guid <xsl:value-of select="field[1]"/>)
        {
            return base.ExecuteFunction&lt;object&gt;("GetById", delegate()
            {
                dm.<xsl:value-of select="@name" /> result = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.GetById(<xsl:value-of select="field[1]"/>);
                if (result == null)
                {
                    return Http404("<xsl:value-of select="@name" />");
                }

                <xsl:if test="count(field[text()='faction_id'])>0">this.Analytics.TrackFactionAccess(this.GetCurrentAccount(), result.faction_id);</xsl:if>

                return base.Http200(new ItemResult&lt;sdk.<xsl:value-of select="@name" />&gt;()
                {
                    success = true,
                    item = result.ToSDKModel()
                });
            });
        }
        
        <xsl:if test="count(field[@searchable='true']) > 0 or count(indexfield[@searchable='true']) > 0">[HttpGet]
        [Route("")]
        public object Find(int skip = 0, int take = 10, string order_by = "", bool descending = false, string keyword = ""<xsl:for-each select="field[string-length(@searchToggle)>0]">,  <xsl:variable name="searchType"><xsl:value-of select="@type" /></xsl:variable><xsl:if test="/items/enum[@name=$searchType]">sdk.</xsl:if><xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="indexfield[string-length(@searchToggle)>0]">,  <xsl:variable name="searchType"><xsl:value-of select="@type" /></xsl:variable><xsl:if test="/items/enum[@name=$searchType]">sdk.</xsl:if><xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            return base.ExecuteFunction&lt;object&gt;("Find", delegate()
            {


                int takePlus = take;
                if (take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }

                List&lt;dm.<xsl:value-of select="@name" />&gt; result = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.Find(skip, takePlus, keyword, order_by, descending<xsl:for-each select="field[string-length(@searchToggle)>0]">, <xsl:value-of select="text()"/></xsl:for-each><xsl:for-each select="indexfield[string-length(@searchToggle)>0]">, <xsl:value-of select="text()"/></xsl:for-each>);
                return base.Http200(result.ToSteppedListResult(skip, take));

            });
        }
        
        </xsl:if>
        <xsl:variable name="removePattern"><xsl:value-of select="@removePattern"/></xsl:variable>
        <xsl:for-each select="field[@foreignKey]">[HttpGet]
        [Route("by_<xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="@friendlyName"/></xsl:call-template>/{<xsl:value-of select="@foreignKeyField"/>}")]
        public object GetBy<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="@foreignKeyField"/><xsl:if test="$removePattern='true'">, bool include_removed = false</xsl:if>)
        {
            return base.ExecuteFunction&lt;object&gt;("GetBy<xsl:value-of select="@friendlyName" />", delegate ()
            {
                List&lt;dm.<xsl:value-of select="../@name"/>&gt; result = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>.GetBy<xsl:value-of select="@friendlyName" />(<xsl:value-of select="@foreignKeyField"/><xsl:if test="$removePattern='true'">, include_removed</xsl:if>);

                return base.Http200(new ListResult&lt;sdk.<xsl:value-of select="../@name"/>&gt;()
                {
                    success = true,
                    items = result.ToSDKModel()
                });
            });
        }
        </xsl:for-each>
        
        </xsl:otherwise></xsl:choose>
        
        
       

        [HttpPost]
        [Route("")]
        public object Create(sdk.<xsl:value-of select="@name" /><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>)
        {
            return base.ExecuteFunction&lt;object&gt;("Create", delegate()
            {
                this.ValidateNotNull(<xsl:value-of select="$name_lowered"/>, "<xsl:value-of select="@name" />");

                dm.<xsl:value-of select="@name" /> insert = <xsl:value-of select="$name_lowered"/>.ToDomainModel();

                
                insert = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.Insert(insert);
                

                <xsl:choose><xsl:when test="@useIndex='true'">
                sdk.<xsl:value-of select="@name" /> result = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.GetById(<xsl:if test="string-length(@indexParent) > 0"><xsl:for-each select="field[@indexParent='true']">insert.<xsl:value-of select="text()" />, </xsl:for-each></xsl:if>insert.<xsl:value-of select="field[1]"/>);</xsl:when><xsl:otherwise>
                sdk.<xsl:value-of select="@name" /> result = insert.ToSDKModel();</xsl:otherwise></xsl:choose>

                return base.Http201(new ItemResult&lt;sdk.<xsl:value-of select="@name" />&gt;()
                {
                    success = true,
                    item = result
                }
                , string.Format("api/<xsl:value-of select="$name_lowered"/>/{0}", <xsl:value-of select="$name_lowered"/>.<xsl:value-of select="field[1]"/>));

            });

        }


        [HttpPut]
        [Route("{<xsl:value-of select="field[1]"/>}")]
        public object Update(Guid <xsl:value-of select="field[1]"/>, sdk.<xsl:value-of select="@name" /><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>)
        {
            return base.ExecuteFunction&lt;object&gt;("Update", delegate()
            {
                this.ValidateNotNull(<xsl:value-of select="$name_lowered"/>, "<xsl:value-of select="@name" />");
                this.ValidateRouteMatch(<xsl:value-of select="field[1]"/>, <xsl:value-of select="$name_lowered"/>.<xsl:value-of select="field[1]"/>, "<xsl:value-of select="@name" />");

                <xsl:value-of select="$name_lowered"/>.<xsl:value-of select="field[1]"/> = <xsl:value-of select="field[1]"/>;
                dm.<xsl:value-of select="@name" /> update = <xsl:value-of select="$name_lowered"/>.ToDomainModel();


                update = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.Update(update);
                
                <xsl:choose><xsl:when test="@useIndex='true'">
                sdk.<xsl:value-of select="@name" /> existing = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.GetById(<xsl:if test="string-length(@indexParent) > 0"><xsl:for-each select="field[@indexParent='true']">update.<xsl:value-of select="text()" />, </xsl:for-each></xsl:if>update.<xsl:value-of select="field[1]"/>);
                </xsl:when><xsl:otherwise>
                sdk.<xsl:value-of select="@name" /> existing = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.GetById(update.<xsl:value-of select="field[1]"/>).ToSDKModel();</xsl:otherwise></xsl:choose>
                
                return base.Http200(new ItemResult&lt;sdk.<xsl:value-of select="@name" />&gt;()
                {
                    success = true,
                    item = existing
                });

            });

        }

        <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
        [HttpPost]
        [Route("{<xsl:value-of select="../field[1]"/>}/update_<xsl:value-of select="text()"/>/{priority}")]
        public object Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>(Guid <xsl:value-of select="../field[1]"/>, int priority)
        {
            return base.ExecuteFunction&lt;object&gt;("Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>", delegate ()
            {
                dm.<xsl:value-of select="../@name"/> found = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>.GetById(<xsl:value-of select="../field[1]"/>);
                this.ValidateNotNull(found, "<xsl:value-of select="../@name"/>");

                this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>.Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>(found.<xsl:value-of select="@priorityGroupBy"/>, found.<xsl:value-of select="../field[1]"/>, priority);

                return base.Http200(new ActionResult()
                {
                    success = true
                });
            });
        }
        </xsl:for-each>

        [HttpDelete]
        [Route("{<xsl:value-of select="field[1]"/>}")]
        public object Delete(Guid <xsl:value-of select="field[1]"/>)
        {
            return base.ExecuteFunction("Delete", delegate()
            {
                dm.<xsl:value-of select="@name" /> delete = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.GetById(<xsl:value-of select="field[1]"/>);
                
                
                this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.Delete(<xsl:value-of select="field[1]"/>);

                return Http200(new ActionResult()
                {
                    success = true,
                    message = <xsl:value-of select="field[1]"/>.ToString()
                });
            });
        }

    }
}

'''[ENDFILE]

</xsl:for-each>
  
'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.SDK.Shared\<xsl:value-of select="items/@projectName"/>SDK_Endpoints_Core.cs]using <xsl:value-of select="items/@projectName"/>.SDK.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace <xsl:value-of select="items/@projectName"/>.SDK
{
    public partial class <xsl:value-of select="items/@projectName"/>SDK
    {
        // members for web ease
        <xsl:for-each select="items/item">public <xsl:value-of select="@name" />Endpoint <xsl:value-of select="@name" />;
        </xsl:for-each>

        protected virtual void ConstructCoreEndpoints()
        {
            <xsl:for-each select="items/item">this.<xsl:value-of select="@name" /> = new <xsl:value-of select="@name" />Endpoint(this);
            </xsl:for-each>
        }   
    }
}

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
    <xsl:template name="Pluralize">
          <xsl:param name="inputString"/>
          <xsl:choose><xsl:when test="substring($inputString, string-length($inputString)) = 'y'"><xsl:value-of select="concat(substring($inputString, 1, string-length($inputString)-1),'ies')"/></xsl:when><xsl:otherwise><xsl:value-of select="$inputString"/>s</xsl:otherwise></xsl:choose>
  </xsl:template>
</xsl:stylesheet>