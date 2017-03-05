<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">

<xsl:for-each select="items/item">
  <xsl:variable name="name_lowered"><xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="@name"/></xsl:call-template></xsl:variable>
  <xsl:variable name="removePattern2"><xsl:value-of select="@removePattern"/></xsl:variable>
'''[STARTFILE:<xsl:value-of select="../@projectName"/>.Primary\Business\Direct\I<xsl:value-of select="@name"/>Business_Crud.cs]using System;
using System.Collections.Generic;
using System.Text;
using <xsl:value-of select="../@projectName"/>.Domain;

namespace <xsl:value-of select="../@projectName"/>.Primary.Business.Direct
{
    // WARNING: THIS FILE IS GENERATED
    public partial interface I<xsl:value-of select="@name"/>Business
    {
        <xsl:value-of select="@name"/> GetById(<xsl:value-of select="field[1]/@type"/><xsl:text> </xsl:text><xsl:value-of select="field[1]"/>);
        <xsl:if test="count(field[@searchable='true' and string-length(@computedFrom)=0]) > 0">List&lt;<xsl:value-of select="@name"/>&gt; Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false<xsl:for-each select="field[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text> <xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>);
        </xsl:if>
        <xsl:for-each select="field[@foreignKey]">
        List&lt;<xsl:value-of select="../@name"/>&gt; GetBy<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="@foreignKeyField"/><xsl:if test="$removePattern2='true'">, bool includeRemoved</xsl:if>);
        <xsl:if test="@foreignKeyInvalidatesMe='true'">void InvalidateFor<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="@foreignKeyField"/>, string reason);</xsl:if>
        </xsl:for-each>
        <xsl:for-each select="field[@lookup='true']">
        List&lt;<xsl:value-of select="../@name"/>&gt; GetBy<xsl:call-template name="NoSpace"><xsl:with-param name="inputString" select="@friendlyName"/></xsl:call-template>(<xsl:value-of select="@type" /><xsl:text> </xsl:text><xsl:value-of select="text()"/>);
        </xsl:for-each>
        
        
        <xsl:value-of select="@name"/> Insert(<xsl:value-of select="@name"/> insert<xsl:value-of select="@name"/>);
        <xsl:value-of select="@name"/> Update(<xsl:value-of select="@name"/> update<xsl:value-of select="@name"/>);
        <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
        void Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>(Guid <xsl:value-of select="@priorityGroupBy"/>, Guid <xsl:value-of select="../field[1]"/>, int priority);
        </xsl:for-each>
        void Delete(<xsl:value-of select="field[1]/@type"/><xsl:text> </xsl:text><xsl:value-of select="field[1]"/>);
        <xsl:if test="@useIndex='true'">void SynchronizationUpdate(Guid <xsl:value-of select="field[1]"/>, bool success, DateTime sync_date_utc, string sync_log);
        List&lt;Guid?&gt; SynchronizationGetInvalid(int retryPriorityThreshold, string sync_agent);
        void SynchronizationHydrateUpdate(Guid <xsl:value-of select="field[1]"/>, bool success, DateTime sync_date_utc, string sync_log);
        List&lt;Guid?&gt; SynchronizationHydrateGetInvalid(int retryPriorityThreshold, string sync_agent);</xsl:if>
        <xsl:if test="@useIndex='true'">
        void Invalidate(Guid <xsl:value-of select="field[1]"/>, string reason);</xsl:if>
        
    }
}

'''[ENDFILE]


'''[STARTFILE:<xsl:value-of select="../@projectName"/>.Primary\Business\Direct\Implementation\<xsl:value-of select="@name"/>Business_Crud.cs]using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="../@projectName"/>.Domain;
using <xsl:value-of select="../@projectName"/>.Data.Sql;
using <xsl:value-of select="../@projectName"/>.Primary.Synchronization;

namespace <xsl:value-of select="../@projectName"/>.Primary.Business.Direct.Implementation
{
    // WARNING: THIS FILE IS GENERATED
    public partial class <xsl:value-of select="@name"/>Business : BusinessBase, I<xsl:value-of select="@name"/>Business
    {
        public <xsl:value-of select="@name"/>Business(IFoundation foundation)
            : base(foundation, "<xsl:value-of select="@name"/>")
        {
        }
        
        <xsl:if test="@useIndex='true'">protected I<xsl:value-of select="@name"/>Synchronizer Synchronizer
        {
            get
            {
                return this.IFoundation.Resolve&lt;I<xsl:value-of select="@name"/>Synchronizer&gt;();
            }
        }</xsl:if>
        <xsl:if test="string-length(@indexAgent)>0">
        public override string DefaultAgent
        {
            get
            {
                return Daemons.Agents.<xsl:value-of select="@indexAgent"/>;
            }
        }</xsl:if>

        public <xsl:value-of select="@name"/> Insert(<xsl:value-of select="@name"/> insert<xsl:value-of select="@name"/>)
        {
            return base.ExecuteFunction("Insert", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
                    insert<xsl:value-of select="../@name"/>.priority = (from o in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>
                                                     where o.<xsl:value-of select="@priorityGroupBy"/> == insert<xsl:value-of select="../@name"/>.<xsl:value-of select="@priorityGroupBy"/>
                                                     &amp;&amp; o.deleted_utc == null
                                                     select o.<xsl:value-of select="../field[1]"/>).Count() + 1;
                    </xsl:for-each>

                    this.PreProcess(insert<xsl:value-of select="@name"/>, true);
                    var interception = this.Intercept(insert<xsl:value-of select="@name"/>, true);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    if (insert<xsl:value-of select="@name"/>.<xsl:value-of select="field[1]"/> == Guid.Empty)
                    {
                        insert<xsl:value-of select="@name"/>.<xsl:value-of select="field[1]"/> = Guid.NewGuid();
                    }
                    <xsl:if test="@useIndex='true' or @trackUpdates='true'">insert<xsl:value-of select="@name"/>.created_utc = DateTime.UtcNow;
                    insert<xsl:value-of select="@name"/>.updated_utc = insert<xsl:value-of select="@name"/>.created_utc;</xsl:if>

                    db<xsl:value-of select="@name"/> dbModel = insert<xsl:value-of select="@name"/>.ToDbModel();
                    
                    <xsl:if test="@useIndex='true'">dbModel.InvalidateSync(this.DefaultAgent, "insert");</xsl:if>

                    db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.Add(dbModel);

                    db.SaveChanges();
                    
                    this.AfterInsertPersisted(db, dbModel);
                    
                    <xsl:if test="@useIndex='true'">this.Synchronizer.SynchronizeItem(dbModel.<xsl:value-of select="field[1]"/>, Availability.<xsl:choose><xsl:when test="@indexForSearchable='true'">Searchable</xsl:when><xsl:otherwise>Retrievable</xsl:otherwise></xsl:choose>);
                    this.AfterInsertIndexed(db, dbModel);
                    </xsl:if>
                    this.DependencyCoordinator.<xsl:value-of select="@name"/>Invalidated(Dependency.None, dbModel.<xsl:value-of select="field[1]"/>);
                }
                return this.GetById(insert<xsl:value-of select="@name"/>.<xsl:value-of select="field[1]"/>);
            });
        }
        public <xsl:value-of select="@name"/> Update(<xsl:value-of select="@name"/> update<xsl:value-of select="@name"/>)
        {
            return base.ExecuteFunction("Update", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    this.PreProcess(update<xsl:value-of select="@name"/>, false);
                    var interception = this.Intercept(update<xsl:value-of select="@name"/>, false);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    <xsl:if test="@useIndex='true' or @trackUpdates='true'">update<xsl:value-of select="@name"/>.updated_utc = DateTime.UtcNow;</xsl:if>
                    
                    db<xsl:value-of select="@name"/> found = (from n in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
                                    where n.<xsl:value-of select="field[1]"/> == update<xsl:value-of select="@name"/>.<xsl:value-of select="field[1]"/>
                                    select n).FirstOrDefault();

                    if (found != null)
                    {
                        <xsl:value-of select="@name"/> previous = found.ToDomainModel();
                        <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
                        update<xsl:value-of select="../@name"/>.<xsl:value-of select="text()"/> = found.<xsl:value-of select="text()"/>;// prevent priority update
                        </xsl:for-each>
                        found = update<xsl:value-of select="@name"/>.ToDbModel(found);
                        <xsl:if test="@useIndex='true'">found.InvalidateSync(this.DefaultAgent, "updated");</xsl:if>
                        db.SaveChanges();
                        
                        this.AfterUpdatePersisted(db, found, previous);
                        
                        <xsl:if test="@useIndex='true'">this.Synchronizer.SynchronizeItem(found.<xsl:value-of select="field[1]"/>, Availability.<xsl:choose><xsl:when test="@indexForSearchable='true'">Searchable</xsl:when><xsl:otherwise>Retrievable</xsl:otherwise></xsl:choose>);
                        this.AfterUpdateIndexed(db, found);
                        </xsl:if>
                        this.DependencyCoordinator.<xsl:value-of select="@name"/>Invalidated(Dependency.None, found.<xsl:value-of select="field[1]"/>);
                    
                    }
                    
                    return this.GetById(update<xsl:value-of select="@name"/>.<xsl:value-of select="field[1]"/>);
                }
            });
        }
        <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
        public void Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>(Guid <xsl:value-of select="@priorityGroupBy"/>, Guid <xsl:value-of select="../field[1]"/>, int priority)
        {
            base.ExecuteMethod("Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>", delegate ()
            {
                List&lt;Guid&gt; changedList = new List&lt;Guid&gt;();
                using (var db = base.CreateSQLContext())
                {
                    List&lt;db<xsl:value-of select="../@name"/>&gt; ordered = (from f in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>
                                                      where f.<xsl:value-of select="@priorityGroupBy"/> == <xsl:value-of select="@priorityGroupBy"/>
                                                      &amp;&amp; f.deleted_utc == null
                                                      orderby f.<xsl:value-of select="../field[1]"/>
                                                      select f).ToList();


                    db<xsl:value-of select="../@name"/> match = ordered.Where(x =&gt; x.<xsl:value-of select="../field[1]"/> == <xsl:value-of select="../field[1]"/>).FirstOrDefault();
                    
                    if (match != null &amp;&amp; match.<xsl:value-of select="text()"/> != priority)
                    {
                        ordered.Remove(match);
                        bool added = false;

                        for (int i = 0; i &lt; ordered.Count; i++)
                        {
                            if (priority &lt;= ordered[i].<xsl:value-of select="text()"/>)
                            {
                                if (match.<xsl:value-of select="text()"/> &gt; priority)
                                {
                                    // moving up, so insert before
                                    ordered.Insert(i, match);
                                }
                                else
                                {
                                    // moving down, so insert after [assumes we have a contiguous list]
                                    if (ordered.Count &gt; i + 1)
                                    {
                                        ordered.Insert(i + 1, match);
                                    }
                                    else
                                    {
                                        ordered.Add(match);
                                    }
                                }
                                added = true;
                                break;
                            }
                        }
                        if (!added)
                        {
                            ordered.Add(match);
                        }
                    }
                    for (int i = 0; i &lt; ordered.Count; i++)
                    {
                        int newPriority = i + 1;
                        if (ordered[i].<xsl:value-of select="text()"/> != newPriority)
                        {
                            ordered[i].<xsl:value-of select="text()"/> = i + 1;
                            ordered[i].InvalidateSync(DefaultAgent, "Sort");
                            changedList.Add(ordered[i].<xsl:value-of select="../field[1]"/>);
                        }
                    }
                    db.SaveChanges();
                }

                // outside of scope
                foreach (Guid item in changedList)
                {
                    this.Synchronizer.SynchronizeItem(item, Availability.Retrievable);
                }
                if(changedList.Count &gt; 0)
                {
                    // do the last one again only for sync time
                    this.Synchronizer.SynchronizeItem(changedList[0], Availability.Searchable);
                }
                foreach (Guid item in changedList)
                {
                    this.DependencyCoordinator.<xsl:value-of select="../@name"/>Invalidated(Dependency.None, item);
                }
            });
        }
        </xsl:for-each>
        <xsl:choose><xsl:when test="@removePattern='true'">public void Delete(Guid <xsl:value-of select="field[1]"/>)
        {
            base.ExecuteMethod("Delete", delegate ()
            {
                <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
                Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>(<xsl:value-of select="../field[1]"/>, int.MaxValue); // move to end
                </xsl:for-each>
                using (var db = base.CreateSQLContext())
                {
                    db<xsl:value-of select="@name"/> found = (from a in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
                                           where a.<xsl:value-of select="field[1]"/> == <xsl:value-of select="field[1]"/>
                                           select a).FirstOrDefault();

                    if (found != null &amp;&amp; !found.removed)
                    {
                        found.removed = true;
                        found.removed_utc = DateTime.UtcNow;
                        <xsl:if test="@useIndex='true'">found.InvalidateSync(this.DefaultAgent, "deleted");</xsl:if>
                        db.SaveChanges();
                        
                        this.AfterDeletePersisted(db, found);

                        <xsl:if test="@useIndex='true'">this.Synchronizer.SynchronizeItem(found.<xsl:value-of select="field[1]"/>, Availability.<xsl:choose><xsl:when test="@indexForSearchable='true'">Searchable</xsl:when><xsl:otherwise>Retrievable</xsl:otherwise></xsl:choose>);
                        </xsl:if>
                        this.DependencyCoordinator.<xsl:value-of select="@name"/>Invalidated(Dependency.None, found.<xsl:value-of select="field[1]"/>);
                    }
                }
            });
        }</xsl:when>
        <xsl:otherwise>public void Delete(Guid <xsl:value-of select="field[1]"/>)
        {
            base.ExecuteMethod("Delete", delegate()
            {
                
                using (var db = base.CreateSQLContext())
                {
                    db<xsl:value-of select="@name"/> found = (from a in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
                                    where a.<xsl:value-of select="field[1]"/> == <xsl:value-of select="field[1]"/>
                                    select a).FirstOrDefault();

                    if (found != null)
                    {
                        
                        <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
                        Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>(found.<xsl:value-of select="@priorityGroupBy"/>, <xsl:value-of select="../field[1]"/>, int.MaxValue); // move to end
                        </xsl:for-each>
                        <xsl:if test="count(field[string-length(@priorityGroupBy)>0])>0">
                        //re-retrieve
                        found = (from a in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
                                    where a.<xsl:value-of select="field[1]"/> == <xsl:value-of select="field[1]"/>
                                    select a).FirstOrDefault();
                                    
                        </xsl:if>

                        <xsl:choose><xsl:when test="@useIndex='true'">found.deleted_utc = DateTime.UtcNow;
                        found.InvalidateSync(this.DefaultAgent, "deleted");</xsl:when>
                        <xsl:otherwise>db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.Remove(found);
                        </xsl:otherwise>
                        </xsl:choose>
                        db.SaveChanges();
                        
                        this.AfterDeletePersisted(db, found);
                        
                        <xsl:if test="@useIndex='true'">this.Synchronizer.SynchronizeItem(found.<xsl:value-of select="field[1]"/>, Availability.<xsl:choose><xsl:when test="@indexForSearchable='true'">Searchable</xsl:when><xsl:otherwise>Retrievable</xsl:otherwise></xsl:choose>);
                        </xsl:if>
                        this.DependencyCoordinator.<xsl:value-of select="@name"/>Invalidated(Dependency.None, found.<xsl:value-of select="field[1]"/>);
                    }
                }
            });
        }
        </xsl:otherwise>
        </xsl:choose>
        
        <xsl:if test="@useIndex='true'">public void SynchronizationUpdate(Guid <xsl:value-of select="field[1]"/>, bool success, DateTime sync_date_utc, string sync_log)
        {
            base.ExecuteMethod("SynchronizationUpdate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.sp<xsl:value-of select="@name"/>_SyncUpdate(<xsl:value-of select="field[1]"/>, success, sync_date_utc, sync_log);
                }
            });
        }
        public List&lt;Guid?&gt; SynchronizationGetInvalid(int retryPriorityThreshold, string sync_agent)
        {
            return base.ExecuteFunction("SynchronizationGetInvalid", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    return db.sp<xsl:value-of select="@name"/>_SyncGetInvalid(retryPriorityThreshold, sync_agent).ToList();
                }
            });
        }
        public void SynchronizationHydrateUpdate(Guid <xsl:value-of select="field[1]"/>, bool success, DateTime sync_date_utc, string sync_log)
        {
            base.ExecuteMethod("SynchronizationHydrateUpdate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.sp<xsl:value-of select="@name"/>_HydrateSyncUpdate(<xsl:value-of select="field[1]"/>, success, sync_date_utc, sync_log);
                }
            });
        }
        public List&lt;Guid?&gt; SynchronizationHydrateGetInvalid(int retryPriorityThreshold, string sync_agent)
        {
            return base.ExecuteFunction("SynchronizationHydrateGetInvalid", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    return db.sp<xsl:value-of select="@name"/>_HydrateSyncGetInvalid(retryPriorityThreshold, sync_agent).ToList();
                }
            });
        }
        </xsl:if>
        public <xsl:value-of select="@name"/> GetById(Guid <xsl:value-of select="field[1]"/>)
        {
            return base.ExecuteFunction("GetById", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    db<xsl:value-of select="@name"/> result = (from n in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
                                     where (n.<xsl:value-of select="field[1]"/> == <xsl:value-of select="field[1]"/>)
                                     select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        <xsl:variable name="removePattern"><xsl:value-of select="@removePattern"/></xsl:variable>
        <xsl:for-each select="field[@foreignKey]">public List&lt;<xsl:value-of select="../@name"/>&gt; GetBy<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="@foreignKeyField"/><xsl:if test="$removePattern='true'">, bool includeRemoved</xsl:if>)
        {
            return base.ExecuteFunction("GetBy<xsl:value-of select="@friendlyName" />", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    var result = (from n in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>
                                     where (n.<xsl:value-of select="text()" /> == <xsl:value-of select="@foreignKeyField"/>)<xsl:if test="$removePattern='true'">
                                     &amp;&amp; (includeRemoved || n.removed == false)</xsl:if><xsl:if test="string-length(../@uiDefaultSort)>0">
                                     orderby n.<xsl:value-of select="../@uiDefaultSort" /></xsl:if>
                                     select n);
                    return result.ToDomainModel();
                }
            });
        }
        
        <xsl:if test="@foreignKeyInvalidatesMe='true'">
        public void InvalidateFor<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="@foreignKeyField"/>, string reason)
        {
            base.ExecuteMethod("InvalidateFor<xsl:value-of select="@friendlyName" />", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>
                        .Where(x => x.<xsl:value-of select="text()"/> == <xsl:value-of select="@foreignKeyField"/>)
                        .Update(x => new db<xsl:value-of select="../@name"/>() {
                            sync_success_utc = null,
                            sync_hydrate_utc = null,
                            sync_invalid_utc = DateTime.UtcNow,
                            sync_log = reason
                        });
                    <xsl:variable name="selfName"><xsl:value-of select="../@name"/></xsl:variable>
                    <xsl:variable name="selfProperty"><xsl:value-of select="text()"/></xsl:variable>
                    <xsl:variable name="paramName"><xsl:value-of select="@foreignKeyField"/></xsl:variable>
                    <xsl:for-each select="../../item/field[@foreignKey=$selfName and @foreignKeyInvalidatesMe='true']">
                     db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>
                        .Where(x => x.<xsl:value-of select="$selfName"/>.<xsl:value-of select="$selfProperty"/> == <xsl:value-of select="$paramName"/>)
                        .Update(x => new db<xsl:value-of select="../@name"/>()
                        {
                            sync_success_utc = null,
                            sync_hydrate_utc = null,
                            sync_invalid_utc = DateTime.UtcNow,
                            sync_log = reason
                        });
                    </xsl:for-each>
                }
            });
        }
        </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="field[@lookup='true']">public List&lt;<xsl:value-of select="../@name"/>&gt; GetBy<xsl:call-template name="NoSpace"><xsl:with-param name="inputString" select="@friendlyName"/></xsl:call-template>(<xsl:value-of select="@type" /><xsl:text> </xsl:text><xsl:value-of select="text()"/>)
        {
            return base.ExecuteFunction("GetBy<xsl:call-template name="NoSpace"><xsl:with-param name="inputString" select="@friendlyName"/></xsl:call-template>", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    var result = (from n in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="../@name"/></xsl:call-template>
                                     where (n.<xsl:value-of select="text()" /> == <xsl:value-of select="text()" />)<xsl:if test="string-length(../@uiDefaultSort)>0">
                                     orderby n.<xsl:value-of select="../@uiDefaultSort" /></xsl:if>
                                     select n);
                    return result.ToDomainModel();
                }
            });
        }
        </xsl:for-each>
        <xsl:if test="@useIndex='true'">
        public void Invalidate(Guid <xsl:value-of select="field[1]"/>, string reason)
        {
            base.ExecuteMethod("Invalidate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
                        .Where(x => x.<xsl:value-of select="field[1]"/> == <xsl:value-of select="field[1]"/>)
                        .Update(x => new db<xsl:value-of select="@name"/>() {
                            sync_success_utc = null,
                            sync_hydrate_utc = null,
                            sync_invalid_utc = DateTime.UtcNow,
                            sync_log = reason
                        });
                }
            });
        }
        </xsl:if>

        <xsl:if test="count(field[@searchable='true' and string-length(@computedFrom)=0]) > 0">public List&lt;<xsl:value-of select="@name"/>&gt; Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false<xsl:for-each select="field[string-length(@searchToggle)>0]">, <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text> <xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            return base.ExecuteFunction("Find", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    if(string.IsNullOrEmpty(keyword))
                    { 
                        keyword = ""; 
                    }

                    var data = (from p in db.db<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
                                where (keyword == "" <xsl:for-each select="field[@searchable='true' and string-length(@computedFrom)=0]">
                                    || p.<xsl:value-of select="text()"/>.Contains(keyword)
                                </xsl:for-each>)<xsl:for-each select="field[string-length(@searchToggle)>0]">
                                &amp;&amp; (<xsl:value-of select="text()"/> == null || p.<xsl:value-of select="text()"/> == <xsl:if test="@type!='int' and @dbType='int'">(int)</xsl:if><xsl:value-of select="text()"/><xsl:if test="not(@type='string')">.GetValueOrDefault()</xsl:if>)
                                </xsl:for-each>
                                select p);

                    List&lt;db<xsl:value-of select="@name"/>&gt; result = new List&lt;db<xsl:value-of select="@name"/>&gt;();

                    switch (order_by)
                    {<xsl:for-each select="field[@sortable='true']">
                        case "<xsl:value-of select="text()"/>":
                            if (!descending)
                            {
                                result = data.OrderBy(s => s.<xsl:value-of select="text()"/>).Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                result = data.OrderByDescending(s => s.<xsl:value-of select="text()"/>).Skip(skip).Take(take).ToList();
                            }
                            break;
                        </xsl:for-each>
                        default:
                            <xsl:if test="@uiDefaultSort">if (!descending)
                            {
                                result = data.OrderBy(s => s.<xsl:value-of select="@uiDefaultSort"></xsl:value-of>).Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                result = data.OrderByDescending(s => s.<xsl:value-of select="@uiDefaultSort"></xsl:value-of>).Skip(skip).Take(take).ToList();
                            }
                            </xsl:if><xsl:if test="not(@uiDefaultSort)">result = data.OrderBy(s => s.<xsl:value-of select="field[1]"/>).Skip(skip).Take(take).ToList();</xsl:if>
                            break;
                    }
                    return result.ToDomainModel();
                }
            });
        }</xsl:if>
        


        
        
        public InterceptArgs&lt;<xsl:value-of select="@name"/>&gt; Intercept(<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>, bool forInsert)
        {
            InterceptArgs&lt;<xsl:value-of select="@name"/>&gt; args = new InterceptArgs&lt;<xsl:value-of select="@name"/>&gt;()
            {
                ForInsert = forInsert,
                ReturnEntity = <xsl:value-of select="$name_lowered"/>
            };
            this.PerformIntercept(args);
            return args;
        }

        partial void PerformIntercept(InterceptArgs&lt;<xsl:value-of select="@name"/>&gt; args);
        partial void PreProcess(<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>, bool forInsert);
        partial void AfterInsertPersisted(<xsl:value-of select="../@projectName"/>Context db, db<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>);
        partial void AfterUpdatePersisted(<xsl:value-of select="../@projectName"/>Context db, db<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>, <xsl:value-of select="@name"/> previous);
        partial void AfterDeletePersisted(<xsl:value-of select="../@projectName"/>Context db, db<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>);
        <xsl:if test="@useIndex='true'">partial void AfterUpdateIndexed(<xsl:value-of select="../@projectName"/>Context db, db<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>);
        partial void AfterInsertIndexed(<xsl:value-of select="../@projectName"/>Context db, db<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="$name_lowered"/>);</xsl:if>
    }
}

'''[ENDFILE]

</xsl:for-each>
  


<xsl:for-each select="items/item[@useIndex='true']">
'''[STARTFILE:<xsl:value-of select="../@projectName"/>.Primary\Business\Synchronization\I<xsl:value-of select="@name"/>Synchronizer_Core.cs]using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace <xsl:value-of select="../@projectName"/>.Primary.Synchronization
{
    public partial interface I<xsl:value-of select="@name"/>Synchronizer : ISynchronizer
    {
        void SynchronizeItem(Guid primaryKey, Availability availability);
    }
}

'''[ENDFILE]

'''[STARTFILE:<xsl:value-of select="../@projectName"/>.Primary\Business\Synchronization\Implementation\<xsl:value-of select="@name"/>Synchronizer_Core.cs]using Codeable.Foundation.Common;
using <xsl:value-of select="../@projectName"/>.Common;
using <xsl:value-of select="../@projectName"/>.Domain;
using <xsl:value-of select="../@projectName"/>.Primary.Business.Index;
using <xsl:value-of select="../@projectName"/>.Primary.Health;
using sdk = <xsl:value-of select="../@projectName"/>.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Codeable.Foundation.Core;

namespace <xsl:value-of select="../@projectName"/>.Primary.Synchronization.Implementation
{
    public partial class <xsl:value-of select="@name"/>Synchronizer : SynchronizerBase&lt;Guid&gt;, I<xsl:value-of select="@name"/>Synchronizer
    {
        public <xsl:value-of select="@name"/>Synchronizer(IFoundation foundation)
            : base(foundation, "<xsl:value-of select="@name"/>Synchronizer")
        {

        }

        public override int Priority
        {
            get
            {
                return <xsl:value-of select="@indexPriority"/>;
            }
        }

        public override void PerformSynchronizationForItem(Guid primaryKey)
        {
            base.ExecuteMethod("PerformSynchronizationForItem", delegate ()
            {
                <xsl:value-of select="@name"/> domainModel = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.GetById(primaryKey);
                if (domainModel != null)
                {
                    Action&lt;Guid, bool, DateTime, string&gt; synchronizationUpdateMethod = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.SynchronizationUpdate;
                    if(this.API.Integration.SettingsResolver.IsHydrate())
                    {
                        synchronizationUpdateMethod = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.SynchronizationHydrateUpdate;
                    }
                    DateTime syncDate = DateTime.UtcNow;
                    if (domainModel.sync_invalid_utc.HasValue)
                    {
                        syncDate = domainModel.sync_invalid_utc.Value;
                    }
                    try
                    {
                        sdk.<xsl:value-of select="@name"/> sdkModel = domainModel.ToSDKModel();
                        
                        this.HydrateSDKModelComputed(domainModel, sdkModel);
                        this.HydrateSDKModel(domainModel, sdkModel);

                        if (domainModel.deleted_utc.HasValue)
                        {
                            this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.DeleteDocument(sdkModel);
                            synchronizationUpdateMethod(domainModel.<xsl:value-of select="field[1]"/>, true, syncDate, null);
                        }
                        else
                        {
                            IndexResult result = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.UpdateDocument(sdkModel);
                            if (result.success)
                            {
                                synchronizationUpdateMethod(domainModel.<xsl:value-of select="field[1]"/>, true, syncDate, result.ToString());
                            }
                            else
                            {
                                throw new Exception(result.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.IFoundation.LogError(ex, "PerformSynchronizationForItem");
                        HealthReporter.Current.UpdateMetric(HealthTrackType.Each, string.Format(HealthReporter.INDEXER_ERROR_SYNC, this.EntityName), 0, 1);
                        synchronizationUpdateMethod(primaryKey, false, syncDate, CoreUtility.FormatException(ex));
                    }
                }
            });
        }

        public override int PerformSynchronization(string requestedAgentName)
        {
            return base.ExecuteFunction("PerformSynchronization", delegate ()
            {
                string agentName = requestedAgentName;
                if(string.IsNullOrEmpty(agentName))
                {
                    agentName = this.AgentName;
                }
                List&lt;Guid?&gt; invalidItems = new List&lt;Guid?&gt;();
                if(this.API.Integration.SettingsResolver.IsHydrate())
                {
                    invalidItems = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.SynchronizationHydrateGetInvalid(CommonAssumptions.INDEX_RETRY_THRESHOLD_SECONDS, agentName);
                }
                else
                {
                    invalidItems = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>.SynchronizationGetInvalid(CommonAssumptions.INDEX_RETRY_THRESHOLD_SECONDS, agentName);
                }
                foreach (Guid? item in invalidItems)
                {
                    this.PerformSynchronizationForItem(item.GetValueOrDefault());
                }
                return invalidItems.Count;
            });
        }
        
        /// &lt;summary&gt;
        /// Computed and Calculated Aggs, Typically Generated
        /// &lt;/summary&gt;
        protected void HydrateSDKModelComputed(<xsl:value-of select="@name"/> domainModel, sdk.<xsl:value-of select="@name"/> sdkModel)
        {
            <xsl:for-each select="field[string-length(@computedFrom) > 0]">
            <xsl:variable name="computedFromCurrent"><xsl:value-of select="@computedFrom"/></xsl:variable>
            <xsl:variable name="computedFieldCurrent"><xsl:value-of select="@computedField" /></xsl:variable>
            <xsl:variable name="computedReferenceField"><xsl:value-of select="@computedReferenceField" /></xsl:variable>
            <xsl:variable name="computedByCurrent"><xsl:value-of select="@computedBy" /></xsl:variable>
            <xsl:choose><xsl:when test="$computedByCurrent='Count'">
            sdkModel.<xsl:value-of select="text()" /> = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@computedFrom"/></xsl:call-template>.Get<xsl:value-of select="@computedBy" /><xsl:value-of select="../../item[@name=$computedFromCurrent]/field[text()=$computedFieldCurrent]/@friendlyName" />(sdkModel.<xsl:value-of select="../field[1]" />);
            </xsl:when><xsl:when test="$computedByCurrent='Extra'">
            sdk.<xsl:value-of select="@computedFrom"/> reference<xsl:value-of select="@computedFrom"/> = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@computedFrom"/></xsl:call-template>.GetById(sdkModel.<xsl:value-of select="$computedReferenceField" />);
            if(reference<xsl:value-of select="@computedFrom"/> != null)
            {
                sdkModel.<xsl:value-of select="text()" /> = reference<xsl:value-of select="@computedFrom"/>.<xsl:value-of select="text()"/>;
            }
            else
            {
                <xsl:value-of select="@computedFrom"/> referenceDomain<xsl:value-of select="@computedFrom"/> = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@computedFrom"/></xsl:call-template>.GetById(sdkModel.<xsl:value-of select="$computedReferenceField"/>);
                if(referenceDomain<xsl:value-of select="@computedFrom"/> != null)
                {
                    sdkModel.<xsl:value-of select="text()" /> = referenceDomain<xsl:value-of select="@computedFrom"/>.<xsl:value-of select="text()"/>;
                }
            }
            </xsl:when><xsl:when test="$computedByCurrent='NotNull'">
            sdkModel.<xsl:value-of select="text()" /> = sdkModel.<xsl:value-of select="@computedFrom"/> != null;
            </xsl:when><xsl:when test="$computedByCurrent='Null'">
            sdkModel.<xsl:value-of select="text()" /> = sdkModel.<xsl:value-of select="@computedFrom"/> == null;
            </xsl:when></xsl:choose>
            </xsl:for-each>
            <xsl:for-each select="indexfield[string-length(@computedFrom) > 0]">
            <xsl:variable name="computedFromCurrent"><xsl:value-of select="@computedFrom"/></xsl:variable>
            <xsl:variable name="computedFieldCurrent"><xsl:value-of select="@computedField" /></xsl:variable>
            <xsl:variable name="computedReferenceField"><xsl:value-of select="@computedReferenceField" /></xsl:variable>
            <xsl:variable name="computedByCurrent"><xsl:value-of select="@computedBy" /></xsl:variable>
            <xsl:choose><xsl:when test="$computedByCurrent='Count'">
            sdkModel.<xsl:value-of select="text()" /> = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@computedFrom"/></xsl:call-template>.Get<xsl:value-of select="@computedBy" /><xsl:value-of select="../../item[@name=$computedFromCurrent]/field[text()=$computedFieldCurrent]/@friendlyName" />(sdkModel.<xsl:value-of select="../field[1]" />);
            </xsl:when><xsl:when test="$computedByCurrent='Extra'">
            sdk.<xsl:value-of select="@computedFrom"/> reference<xsl:value-of select="@computedFrom"/> = this.API.Index.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@computedFrom"/></xsl:call-template>.GetById(sdkModel.<xsl:value-of select="$computedReferenceField" />);
            if(reference<xsl:value-of select="@computedFrom"/> != null)
            {
                sdkModel.<xsl:value-of select="text()" /> = reference<xsl:value-of select="@computedFrom"/>.<xsl:value-of select="text()"/>;
            }
            else
            {
                <xsl:value-of select="@computedFrom"/> referenceDomain<xsl:value-of select="@computedFrom"/> = this.API.Direct.<xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@computedFrom"/></xsl:call-template>.GetById(sdkModel.<xsl:value-of select="$computedReferenceField"/>);
                if(referenceDomain<xsl:value-of select="@computedFrom"/> != null)
                {
                    sdkModel.<xsl:value-of select="text()" /> = referenceDomain<xsl:value-of select="@computedFrom"/>.<xsl:value-of select="text()"/>;
                }
            }
            </xsl:when><xsl:when test="$computedByCurrent='NotNull'">
            sdkModel.<xsl:value-of select="text()" /> = sdkModel.<xsl:value-of select="@computedFrom"/> != null;
            </xsl:when><xsl:when test="$computedByCurrent='Null'">
            sdkModel.<xsl:value-of select="text()" /> = sdkModel.<xsl:value-of select="@computedFrom"/> == null;
            </xsl:when></xsl:choose>
            </xsl:for-each>
        }
        partial void HydrateSDKModel(<xsl:value-of select="@name"/> domainModel, sdk.<xsl:value-of select="@name"/> sdkModel);
    }
}

'''[ENDFILE]

</xsl:for-each>

'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\Business\Index\Factory\<xsl:value-of select="items/@projectName"/>ElasticClientFactory_Core.cs]using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="items/@projectName"/>.SDK.Models;
using sdk = <xsl:value-of select="items/@projectName"/>.SDK.Models;

namespace <xsl:value-of select="items/@projectName"/>.Primary.Business.Index
{
    public partial class <xsl:value-of select="items/@projectName"/>ElasticClientFactory
    {
        partial void MapIndexModels(CreateIndexDescriptor indexer)
        {
            MappingsDescriptor descriptor = new MappingsDescriptor();
            <xsl:for-each select="items/item[@useIndex='true']">descriptor.AddMapping&lt;sdk.<xsl:value-of select="@name"/>&gt;(DocumentNames.<xsl:value-of select="@name"/>, p => p
                .AutoMap()<xsl:if test="string-length(@indexParent) > 0">
                .Parent(DocumentNames.<xsl:value-of select="@indexParent"/>)</xsl:if>
                .Properties(props => props
                    <xsl:for-each select="field[@type='Guid' or @notAnalyzed='true']">.String(s => s
                        .Name(n => n.<xsl:value-of select="text()"/>)
                        .Index(FieldIndexOption.NotAnalyzed)
                    )</xsl:for-each>
                    <xsl:for-each select="indexfield[@type='Guid' or @notAnalyzed='true']">.String(s => s
                        .Name(n => n.<xsl:value-of select="text()"/>)
                        .Index(FieldIndexOption.NotAnalyzed)
                    )</xsl:for-each>
                    <xsl:for-each select="indexfield[@sortable='true' or @indexExact='true']">.String(m => m
                        .Name(t => t.<xsl:value-of select="text()"/>)
                        .Fields(f => f
                                .String(s => s.Name(n => n.<xsl:value-of select="text()"/>)
                                .Index(FieldIndexOption.Analyzed))<xsl:if test="@sortable='true'">
                                .String(s => s
                                    .Name(n => n.<xsl:value-of select="text()"/>.Suffix("sort"))
                                    .Analyzer("case_insensitive"))</xsl:if><xsl:if test="@indexExact='true'">
                                .String(s => s
                                    .Name(n => n.<xsl:value-of select="text()"/>.Suffix("exact"))
                                    .Index(FieldIndexOption.NotAnalyzed))</xsl:if>
                                
                        )
                    )</xsl:for-each>
                    <xsl:for-each select="field[@sortable='true' or @indexExact='true']">.String(m => m
                        .Name(t => t.<xsl:value-of select="text()"/>)
                        .Fields(f => f
                                .String(s => s.Name(n => n.<xsl:value-of select="text()"/>)
                                .Index(FieldIndexOption.Analyzed))<xsl:if test="@sortable='true'">
                                .String(s => s
                                    .Name(n => n.<xsl:value-of select="text()"/>.Suffix("sort"))
                                    .Analyzer("case_insensitive"))</xsl:if><xsl:if test="@indexExact='true'">
                                .String(s => s
                                    .Name(n => n.<xsl:value-of select="text()"/>.Suffix("exact"))
                                    .Index(FieldIndexOption.NotAnalyzed))</xsl:if>
                                
                        )
                    )</xsl:for-each>
                    <xsl:for-each select="indexfield[string-length(@nestedObject)>0]"><xsl:variable name="nestedObject"><xsl:value-of select="@nestedObject"/></xsl:variable>.Nested&lt;<xsl:value-of select="$nestedObject"/>&gt;(m => m
                        .Name("<xsl:value-of select="text()"/>")
                        .AutoMap()
                        .Properties(nprops => nprops
                            <xsl:for-each select="../../item[@name=$nestedObject]/field[@type='Guid' or @notAnalyzed='true']">.String(s => s
                                .Name(n => n.<xsl:value-of select="text()"/>)
                                .Index(FieldIndexOption.NotAnalyzed)
                            )</xsl:for-each>
                        )

                    )</xsl:for-each>
                )
            );
            </xsl:for-each>
            indexer.Mappings(m => descriptor);
        }
    }
}
'''[ENDFILE]


  
'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\Mapping\_DomainModelExtensions_Core.cs]using am = AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="items/@projectName"/>.Data.Sql;
using <xsl:value-of select="items/@projectName"/>.Domain;

namespace <xsl:value-of select="items/@projectName"/>.Primary
{
    public static partial class _DomainModelExtensions
    {
        <xsl:for-each select="items/item">
        public static db<xsl:value-of select="@name" /> ToDbModel(this <xsl:value-of select="@name" /> entity, db<xsl:value-of select="@name" /> destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new db<xsl:value-of select="@name" />(); }
                return am.Mapper.Map&lt;<xsl:value-of select="@name" />, db<xsl:value-of select="@name" />&gt;(entity, destination);
            }
            return null;
        }
        public static <xsl:value-of select="@name" /> ToDomainModel(this db<xsl:value-of select="@name" /> entity, <xsl:value-of select="@name" /> destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new <xsl:value-of select="@name" />(); }
                return am.Mapper.Map&lt;db<xsl:value-of select="@name" />, <xsl:value-of select="@name" />&gt;(entity, destination);
            }
            return null;
        }
        public static List&lt;<xsl:value-of select="@name" />&gt; ToDomainModel(this IEnumerable&lt;db<xsl:value-of select="@name" />&gt; entities)
        {
            List&lt;<xsl:value-of select="@name" />&gt; result = new List&lt;<xsl:value-of select="@name" />&gt;();
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    result.Add(item.ToDomainModel());
                }
            }
            return result;
        }
        
        
        </xsl:for-each>
    }
}

'''[ENDFILE]

'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\Mapping\_SDKModelExtensions_Core.cs]using am = AutoMapper;
using Codeable.Foundation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="items/@projectName"/>.Data.Sql;
using <xsl:value-of select="items/@projectName"/>.Domain;

namespace <xsl:value-of select="items/@projectName"/>.Primary
{
    public static partial class _DomainModelExtensions
    {
        <xsl:for-each select="items/item">
        public static <xsl:value-of select="@name" /> ToDomainModel(this SDK.Models.<xsl:value-of select="@name" /> entity, <xsl:value-of select="@name" /> destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new Domain.<xsl:value-of select="@name" />(); }
                <xsl:value-of select="@name" /> result = am.Mapper.Map&lt;SDK.Models.<xsl:value-of select="@name" />, <xsl:value-of select="@name" />&gt;(entity, destination);<xsl:if test="count(field[@facadeMapping='true'])>0">
                result = entity.MapFacade(result);</xsl:if>
                return result;
            }
            return null;
        }
        public static SDK.Models.<xsl:value-of select="@name" /> ToSDKModel(this <xsl:value-of select="@name" /> entity, SDK.Models.<xsl:value-of select="@name" /> destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new SDK.Models.<xsl:value-of select="@name" />(); }
                SDK.Models.<xsl:value-of select="@name" /> result = am.Mapper.Map&lt;<xsl:value-of select="@name" />, SDK.Models.<xsl:value-of select="@name" />&gt;(entity, destination);<xsl:if test="count(field[@facadeMapping='true'])>0">
                result = entity.MapFacade(result);</xsl:if><xsl:for-each select="field[string-length(@derivedProperty)>0]">
                result.<xsl:value-of select="@derivedProperty" /> = entity.Related<xsl:value-of select="@friendlyName" />.GetValueOrDefault().ToRelatedModel();
                </xsl:for-each>
                <xsl:variable name="currentKey"><xsl:value-of select="@name"/></xsl:variable><xsl:for-each select="../item/field[string-length(@derivedParentProperty)>0 and @foreignKey=$currentKey]">
                result.<xsl:value-of select="@derivedParentProperty"/> = entity.Related<xsl:value-of select="../@name"/>.GetValueOrDefault().ToRelatedModel();
                </xsl:for-each>
                return result;
            }
            return null;
        }
        public static List&lt;SDK.Models.<xsl:value-of select="@name" />&gt; ToSDKModel(this IEnumerable&lt;<xsl:value-of select="@name" />&gt; entities)
        {
            List&lt;SDK.Models.<xsl:value-of select="@name" />&gt; result = new List&lt;SDK.Models.<xsl:value-of select="@name" />&gt;();
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    result.Add(item.ToSDKModel());
                }
            }
            return result;
        }
        
        
        </xsl:for-each>
    }
}

'''[ENDFILE]

'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\Mapping\PrimaryMappingProfile_Core.cs]using am = AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="items/@projectName"/>.Data.Sql;
using <xsl:value-of select="items/@projectName"/>.Domain;

namespace <xsl:value-of select="items/@projectName"/>.Primary.Mapping
{
    public partial class PrimaryMappingProfile : AutoMapper.Profile
    {
        public PrimaryMappingProfile()
            : base("PrimaryMappingProfile")
        {
        }

        protected override void Configure()
        {
            this.DbAndDomainMappings();
            this.DomainAndSDKMappings();
            
            this.DbAndDomainMappings_Manual();
            this.DomainAndSDKMappings_Manual();
        }
        
        partial void DbAndDomainMappings_Manual();
        partial void DomainAndSDKMappings_Manual();
        
        protected void DbAndDomainMappings()
        {
            am.Mapper.CreateMap&lt;DateTimeOffset?, DateTime?&gt;()
                .ConvertUsing(x => x.HasValue ? x.Value.UtcDateTime : (DateTime?)null);

            am.Mapper.CreateMap&lt;DateTimeOffset, DateTime?&gt;()
                .ConvertUsing(x => x.UtcDateTime);

            am.Mapper.CreateMap&lt;DateTimeOffset, DateTime&gt;()
                .ConvertUsing(x => x.UtcDateTime);

            am.Mapper.CreateMap&lt;DateTime?, DateTimeOffset?&gt;()
                .ConvertUsing(x => x.HasValue ? new DateTimeOffset(x.Value) : (DateTimeOffset?)null);
                
            <xsl:for-each select="items/item">am.Mapper.CreateMap&lt;db<xsl:value-of select="@name" />, <xsl:value-of select="@name" />&gt;();
            am.Mapper.CreateMap&lt;<xsl:value-of select="@name" />, db<xsl:value-of select="@name" />&gt;();
            </xsl:for-each>
        }
        protected void DomainAndSDKMappings()
        {
            <xsl:for-each select="items/enum">am.Mapper.CreateMap&lt;Domain.<xsl:value-of select="@name" />, SDK.Models.<xsl:value-of select="@name" />&gt;().ConvertUsing(x => (SDK.Models.<xsl:value-of select="@name" />)(int)x);
            am.Mapper.CreateMap&lt;SDK.Models.<xsl:value-of select="@name" />, Domain.<xsl:value-of select="@name" />&gt;().ConvertUsing(x => (Domain.<xsl:value-of select="@name" />)(int)x);
            </xsl:for-each>
            <xsl:for-each select="items/item">
            am.Mapper.CreateMap&lt;Domain.<xsl:value-of select="@name" />, SDK.Models.<xsl:value-of select="@name" />&gt;();
            am.Mapper.CreateMap&lt;SDK.Models.<xsl:value-of select="@name" />, Domain.<xsl:value-of select="@name" />&gt;();
            </xsl:for-each>
        }
    }
}

'''[ENDFILE]

'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\Foundation\<xsl:value-of select="items/@projectName"/>BootStrap_Business.cs]using Codeable.Foundation.Common;
using Codeable.Foundation.UI.Web.Core.Unity;
using <xsl:value-of select="items/@projectName"/>.Primary.Business.Direct;
using <xsl:value-of select="items/@projectName"/>.Primary.Business.Direct.Implementation;
using <xsl:value-of select="items/@projectName"/>.Primary.Business.Index;
using <xsl:value-of select="items/@projectName"/>.Primary.Business.Index.Implementation;
using <xsl:value-of select="items/@projectName"/>.Primary.Synchronization;
using <xsl:value-of select="items/@projectName"/>.Primary.Synchronization.Implementation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace <xsl:value-of select="items/@projectName"/>.Primary.Foundation
{
    public partial class <xsl:value-of select="items/@projectName"/>BootStrap
    {
        protected virtual void RegisterDataElements(IFoundation foundation)
        {
            <xsl:for-each select="items/item">foundation.Container.RegisterType&lt;I<xsl:value-of select="@name" />Business, <xsl:value-of select="@name" />Business&gt;(new HttpRequestLifetimeManager());
            </xsl:for-each>
            
            //Indexes
            <xsl:for-each select="items/item[@useIndex='true']">foundation.Container.RegisterType&lt;I<xsl:value-of select="@name" />Index, <xsl:value-of select="@name" />Index&gt;(new HttpRequestLifetimeManager());
            </xsl:for-each>
            
            //Synchronizers
            <xsl:for-each select="items/item[@useIndex='true']">foundation.Container.RegisterType&lt;I<xsl:value-of select="@name" />Synchronizer, <xsl:value-of select="@name" />Synchronizer&gt;(new HttpRequestLifetimeManager());
            </xsl:for-each>
        }
    }
}

'''[ENDFILE]

'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\<xsl:value-of select="items/@projectName"/>APIIndex.cs]using Codeable.Foundation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="items/@projectName"/>.Primary.Business.Index;

namespace <xsl:value-of select="items/@projectName"/>.Primary
{
    public class <xsl:value-of select="items/@projectName"/>APIIndex : BaseClass
    {
        public <xsl:value-of select="items/@projectName"/>APIIndex(IFoundation ifoundation)
            : base(ifoundation)
        {
        }

        <xsl:for-each select="items/item[@useIndex='true']">public I<xsl:value-of select="@name" />Index <xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
        {
            get { return this.IFoundation.Resolve&lt;I<xsl:value-of select="@name" />Index&gt;(); }
        }
        </xsl:for-each>
    }
}


'''[ENDFILE]



'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\Business\Integration\IDependencyCoordinator.cs]using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="items/@projectName"/>.Domain;

namespace <xsl:value-of select="items/@projectName"/>.Primary.Business.Integration
{
    public interface IDependencyCoordinator
    {
        <xsl:for-each select="items/item">void <xsl:value-of select="@name"/>Invalidated(Dependency affectedDependencies, Guid <xsl:value-of select="field[1]" />);
        </xsl:for-each>
    }
}


'''[ENDFILE]


'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\Business\Integration\DependencyCoordinator_Core.cs]using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using <xsl:value-of select="items/@projectName"/>.Domain;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common;

namespace <xsl:value-of select="items/@projectName"/>.Primary.Business.Integration
{
    public partial class DependencyCoordinator_Core : ChokeableClass, IDependencyCoordinator
    {
        public DependencyCoordinator_Core(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.API = new <xsl:value-of select="items/@projectName"/>API(iFoundation);
        }
        public virtual <xsl:value-of select="items/@projectName"/>API API { get; set; }
        
        <xsl:for-each select="items/item">public virtual void <xsl:value-of select="@name"/>Invalidated(Dependency affectedDependencies, Guid <xsl:value-of select="field[1]" />)
        {
            base.ExecuteMethod("<xsl:value-of select="@name"/>Invalidated", delegate ()
            {
                DependencyWorker&lt;<xsl:value-of select="@name"/>&gt;.EnqueueRequest(this.IFoundation, affectedDependencies, <xsl:value-of select="field[1]" />, this.Process<xsl:value-of select="@name"/>Invalidation);
            });
        }
        protected virtual void Process<xsl:value-of select="@name"/>Invalidation(Dependency dependencies, Guid <xsl:value-of select="field[1]" />)
        {
            base.ExecuteMethod("Process<xsl:value-of select="@name"/>Invalidation", delegate ()
            {
                <xsl:variable name="currentName"><xsl:value-of select="@name"/></xsl:variable>
                <xsl:variable name="currentField"><xsl:value-of select="field[1]" /></xsl:variable>
                <xsl:for-each select="../item/field[@foreignKey=$currentName and @foreignKeyInvalidatesMe='true']">
                this.API.Direct.<xsl:value-of select="../@name"/>s.InvalidateFor<xsl:value-of select="@friendlyName"/>(<xsl:value-of select="$currentField"/>, " changed");
                </xsl:for-each>
                <xsl:for-each select="field[@iInvalidateforeignKey='true']">
                <xsl:value-of select="../@name"/> item = this.API.Direct.<xsl:value-of select="../@name"/>s.GetById(<xsl:value-of select="$currentField"/>);
                if (item != null<xsl:if test="@isNullable='true'"> &amp;&amp; item.<xsl:value-of select="text()"/>.HasValue</xsl:if>)
                {
                    this.API.Direct.<xsl:value-of select="@foreignKey"/>s.Invalidate(item.<xsl:value-of select="text()"/><xsl:if test="@isNullable='true'">.Value</xsl:if>, "<xsl:value-of select="../@name"/> changed");
                }
                </xsl:for-each>
                <xsl:if test="count(../item/field[@foreignKey=$currentName and @foreignKeyInvalidatesMe='true'])>0 or count(field[@iInvalidateforeignKey='true'])>0">
                this.API.Integration.Synchronization.AgitateSyncDaemon();</xsl:if>
            });
        }
        </xsl:for-each>
    }
}


'''[ENDFILE]


'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Data.Sql\Extensions\DatabaseExtensions.cs]using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace <xsl:value-of select="items/@projectName"/>.Data.Sql
{
    public static class DatabaseExtensions
    {
        <xsl:for-each select="items/item[@useIndex='true']">
        public static void InvalidateSync(this db<xsl:value-of select="@name"/> model, string agent, string reason)
        {
            if (model != null)
            {
                model.sync_attempt_utc = null;
                model.sync_success_utc = null;
                model.sync_hydrate_utc = null;
                model.sync_log = reason;
                model.sync_invalid_utc = DateTime.UtcNow;
                model.sync_agent = agent;
            }
        }
        </xsl:for-each>
    }
}
'''[ENDFILE]

<xsl:for-each select="items/item[@useIndex='true']">

'''[STARTFILE:<xsl:value-of select="../@projectName"/>.Primary\Business\Index\I<xsl:value-of select="@name"/>Index_Core.cs]using Codeable.Foundation.Common;
using <xsl:value-of select="../@projectName"/>.SDK.Models;
using <xsl:value-of select="../@projectName"/>.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace <xsl:value-of select="../@projectName"/>.Primary.Business.Index
{
    public partial interface I<xsl:value-of select="@name" />Index : IIndexer&lt;<xsl:value-of select="@name" />&gt;
    {
        <xsl:value-of select="@name" /> GetById(<xsl:if test="string-length(@indexParent) > 0">Guid parentId, </xsl:if>Guid id);
        TCustomModel GetById&lt;TCustomModel&gt;(<xsl:if test="string-length(@indexParent) > 0">Guid parentId, </xsl:if>Guid id)
            where TCustomModel : class;
        <xsl:if test="string-length(@userSpecificData)>0">
        <xsl:value-of select="@name"/> GetById(<xsl:if test="string-length(@indexParent) > 0">Guid parentId, </xsl:if>Guid id, Guid? for_<xsl:value-of select="@userSpecificData"/>);
        </xsl:if>
        <xsl:for-each select="field[@foreignKey]">ListResult&lt;<xsl:value-of select="../@name"/>&gt; GetBy<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="@foreignKeyField"/>, int skip, int take, string order_by = "", bool descending = false<xsl:if test="string-length(../@pagingWindow)>0">, DateTime? before_<xsl:value-of select="../@pagingWindow" /> = null</xsl:if><xsl:if test="string-length(../@userSpecificData)>0">, Guid? for_<xsl:value-of select="../@userSpecificData"/> = null</xsl:if><xsl:for-each select="../field[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text>with_<xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text>with_<xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>);
        </xsl:for-each>
        <xsl:for-each select="indexfield[@lookup='true']">ListResult&lt;<xsl:value-of select="../@name"/>&gt; GetBy<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="text()"/>, int skip, int take, string order_by = "", bool descending = false<xsl:if test="string-length(../@userSpecificData)>0">, Guid? for_<xsl:value-of select="../@userSpecificData"/> = null</xsl:if><xsl:for-each select="../field[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text>with_<xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text>with_<xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>);
        </xsl:for-each>
       <xsl:if test="count(field[@searchable='true']) > 0 or count(indexfield[@searchable='true']) > 0">
        ListResult&lt;<xsl:value-of select="@name"/>&gt; Find(<xsl:if test="string-length(@userSpecificData)>0">Guid? for_<xsl:value-of select="@userSpecificData"/>, </xsl:if>int skip, int take, string keyword = "", string order_by = "", bool descending = false<xsl:for-each select="field[@foreignKey]">, Guid? <xsl:value-of select="text()"/> = null</xsl:for-each><xsl:for-each select="field[string-length(@searchToggle)>0]">, <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text>with_<xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="indexfield[string-length(@searchToggle)>0]">, <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text>with_<xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>);
       </xsl:if>
        <xsl:variable name="nameCache"><xsl:value-of select="@name"/></xsl:variable>
        <xsl:for-each select="../item/field[@computedFrom = $nameCache]">
         <xsl:variable name="computedFrom"><xsl:value-of select="@computedFrom"/></xsl:variable>
         <xsl:variable name="computedType"><xsl:value-of select="@type"/></xsl:variable>
         <xsl:variable name="computedField"><xsl:value-of select="@computedField" /></xsl:variable>
         <xsl:variable name="computedBy"><xsl:value-of select="@computedBy" /></xsl:variable>
         <xsl:variable name="primaryKeyCache"><xsl:value-of select="../field[1]" /></xsl:variable>

         <xsl:for-each select="../../item[@name=$computedFrom]">
        <xsl:choose><xsl:when test="$computedBy='Count'">
        <xsl:value-of select="$computedType"/> Get<xsl:value-of select="$computedBy"/><xsl:value-of select="field[text()=$computedField]/@friendlyName" />(Guid <xsl:value-of select="$primaryKeyCache"/>);
        </xsl:when></xsl:choose>
         </xsl:for-each>
        </xsl:for-each>
        <xsl:for-each select="../item/indexfield[@computedFrom = $nameCache]">
         <xsl:variable name="computedFrom"><xsl:value-of select="@computedFrom"/></xsl:variable>
         <xsl:variable name="computedType"><xsl:value-of select="@type"/></xsl:variable>
         <xsl:variable name="computedField"><xsl:value-of select="@computedField" /></xsl:variable>
         <xsl:variable name="computedBy"><xsl:value-of select="@computedBy" /></xsl:variable>
         <xsl:variable name="primaryKeyCache"><xsl:value-of select="../field[1]" /></xsl:variable>

         <xsl:for-each select="../../item[@name=$computedFrom]">
        <xsl:choose><xsl:when test="$computedBy='Count'">
        <xsl:value-of select="$computedType"/> Get<xsl:value-of select="$computedBy"/><xsl:value-of select="field[text()=$computedField]/@friendlyName" />(Guid <xsl:value-of select="$primaryKeyCache"/>);
        </xsl:when></xsl:choose>
         </xsl:for-each>
        </xsl:for-each>
    }
}
'''[ENDFILE]

'''[STARTFILE:<xsl:value-of select="../@projectName"/>.Primary\Business\Index\Implementation\<xsl:value-of select="@name"/>Index_Core.cs]using Codeable.Foundation.Common;
using <xsl:value-of select="../@projectName"/>.SDK;
using sdk = <xsl:value-of select="../@projectName"/>.SDK.Models;
using <xsl:value-of select="../@projectName"/>.SDK.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace <xsl:value-of select="../@projectName"/>.Primary.Business.Index.Implementation
{
    public partial class <xsl:value-of select="@name"/>Index : Indexer<xsl:if test="string-length(@indexParent) > 0">Child</xsl:if>Base&lt;sdk.<xsl:value-of select="@name" />&gt;, I<xsl:value-of select="@name" />Index
    {
        public <xsl:value-of select="@name"/>Index(IFoundation foundation)
            : base(foundation, "<xsl:value-of select="@name"/>Index", DocumentNames.<xsl:value-of select="@name"/>)
        {

        }
        protected override string GetModelId(sdk.<xsl:value-of select="@name"/> model)
        {
            return model.<xsl:value-of select="field[1]" />.ToString();
        }
        <xsl:if test="string-length(@indexParent) > 0">
        protected override string GetParentId(sdk.<xsl:value-of select="@name"/> model)
        {
            <xsl:for-each select="field[@indexParent='true']">return model.<xsl:value-of select="text()" />.ToString();</xsl:for-each>
        }
        </xsl:if>
        
        <xsl:if test="string-length(@userSpecificData)>0">
        public virtual sdk.<xsl:value-of select="@name"/> GetById(Guid id, Guid? for_<xsl:value-of select="@userSpecificData"/>)
        {
            return base.ExecuteFunction("GetById", delegate ()
            {
                ElasticClient client = ClientFactory.CreateClient();
                IGetResponse&lt;sdk.<xsl:value-of select="@name"/>&gt; response = client.Get&lt;sdk.<xsl:value-of select="@name"/>&gt;(id.ToString(), ClientFactory.IndexName, this.DocumentType);

                sdk.<xsl:value-of select="@name"/> result = response.Source;

                this.PostProcessForUser(new List&lt;sdk.<xsl:value-of select="@name"/>&gt;() { result }, for_<xsl:value-of select="@userSpecificData"/>);

                return result;
            });
        }
        </xsl:if>
        
        <xsl:for-each select="field[@foreignKey]">public ListResult&lt;sdk.<xsl:value-of select="../@name"/>&gt; GetBy<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="@foreignKeyField"/>, int skip, int take, string order_by = "", bool descending = false<xsl:if test="string-length(../@pagingWindow)>0">, DateTime? before_<xsl:value-of select="../@pagingWindow" /> = null</xsl:if><xsl:if test="string-length(../@userSpecificData)>0">, Guid? for_<xsl:value-of select="../@userSpecificData"/> = null</xsl:if><xsl:for-each select="../field[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text> <xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">, <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            return base.ExecuteFunction("GetBy<xsl:value-of select="@friendlyName" />", delegate ()
            {
                QueryContainer query = Query&lt;sdk.<xsl:value-of select="../@name"/>&gt;.Term(w => w.<xsl:value-of select="text()"/>, <xsl:value-of select="@foreignKeyField"/>);

                <xsl:if test="string-length(../@pagingWindow)>0">if(before_<xsl:value-of select="../@pagingWindow" />.HasValue)
                {
                    query &amp;= Query&lt;sdk.<xsl:value-of select="../@name"/>&gt;.DateRange(r => r.Field(f => f.<xsl:value-of select="../@pagingWindow" />).LessThanOrEquals(before_<xsl:value-of select="../@pagingWindow" />.Value));
                }
                </xsl:if>
                <xsl:variable name="searchParentName"><xsl:value-of select="../@name"/></xsl:variable>
                <xsl:for-each select="../field[string-length(@searchToggle)>0]">if(<xsl:value-of select="text()"/>.HasValue)
                {
                    query &amp;= Query&lt;<xsl:value-of select="$searchParentName"/>&gt;.Term(f => f.<xsl:value-of select="text()"/>, <xsl:value-of select="text()"/>.Value);
                }
                </xsl:for-each>
                <xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">if(<xsl:value-of select="text()"/>.HasValue)
                {
                    query &amp;= Query&lt;<xsl:value-of select="$searchParentName"/>&gt;.Term(f => f.<xsl:value-of select="text()"/>, <xsl:value-of select="text()"/>.Value);
                }
                </xsl:for-each>

                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                List&lt;SortFieldDescriptor&lt;sdk.<xsl:value-of select="../@name"/>&gt;&gt; sortFields = new List&lt;SortFieldDescriptor&lt;sdk.<xsl:value-of select="../@name"/>&gt;&gt;();
                if(!string.IsNullOrEmpty(order_by))
                {
                    SortFieldDescriptor&lt;sdk.<xsl:value-of select="../@name"/>&gt; item = new SortFieldDescriptor&lt;sdk.<xsl:value-of select="../@name"/>&gt;()
                        .Field(order_by)
                        .Order(descending ? SortOrder.Descending : SortOrder.Ascending);
                        
                    sortFields.Add(item);
                }
                SortFieldDescriptor&lt;sdk.<xsl:value-of select="../@name"/>&gt; defaultSort = new SortFieldDescriptor&lt;sdk.<xsl:value-of select="../@name"/>&gt;()
                    .Field(r => r.<xsl:choose><xsl:when test="string-length(../@uiDefaultSort)>0"><xsl:value-of select="../@uiDefaultSort" /></xsl:when><xsl:otherwise><xsl:value-of select="../field[1]" /></xsl:otherwise></xsl:choose>)
                    .<xsl:choose><xsl:when test="../@uiDefaultSortDescending='true'">Descending()</xsl:when><xsl:otherwise>Ascending()</xsl:otherwise></xsl:choose>;
                
                sortFields.Add(defaultSort);
                
                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse&lt;sdk.<xsl:value-of select="../@name"/>&gt; searchResponse = client.Search&lt;sdk.<xsl:value-of select="../@name"/>&gt;(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(sr => sr.Multi(sortFields))
                    .Type(this.DocumentType));

                ListResult&lt;sdk.<xsl:value-of select="../@name"/>&gt; result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                <xsl:if test="string-length(../@userSpecificData)>0">
                this.PostProcessForUser(result.items, for_<xsl:value-of select="../@userSpecificData"/>);
                </xsl:if>
                return result;
            });
        }
        </xsl:for-each>

        <xsl:for-each select="indexfield[@lookup='true']">public ListResult&lt;sdk.<xsl:value-of select="../@name"/>&gt; GetBy<xsl:value-of select="@friendlyName" />(Guid <xsl:value-of select="text()"/>, int skip, int take, string order_by = "", bool descending = false<xsl:if test="string-length(../@userSpecificData)>0">, Guid? for_<xsl:value-of select="../@userSpecificData"/> = null</xsl:if><xsl:for-each select="../field[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text> <xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">, <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text><xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            return base.ExecuteFunction("GetBy<xsl:value-of select="@friendlyName" />", delegate ()
            {
                QueryContainer query = Query&lt;sdk.<xsl:value-of select="../@name"/>&gt;.Term(w => w.<xsl:value-of select="text()"/>, <xsl:value-of select="text()"/>);

                <xsl:variable name="searchParentName"><xsl:value-of select="../@name"/></xsl:variable>
                <xsl:for-each select="../field[string-length(@searchToggle)>0]">if(<xsl:value-of select="text()"/>.HasValue)
                {
                    query &amp;= Query&lt;<xsl:value-of select="$searchParentName"/>&gt;.Term(f => f.<xsl:value-of select="text()"/>, <xsl:value-of select="text()"/>.Value);
                }
                </xsl:for-each>
                <xsl:for-each select="../indexfield[string-length(@searchToggle)>0]">if(<xsl:value-of select="text()"/>.HasValue)
                {
                    query &amp;= Query&lt;<xsl:value-of select="$searchParentName"/>&gt;.Term(f => f.<xsl:value-of select="text()"/>, <xsl:value-of select="text()"/>.Value);
                }
                </xsl:for-each>

                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                List&lt;SortFieldDescriptor&lt;<xsl:value-of select="../@name"/>&gt;&gt; sortFields = new List&lt;SortFieldDescriptor&lt;<xsl:value-of select="../@name"/>&gt;&gt;();
                if(!string.IsNullOrEmpty(order_by))
                {
                    SortFieldDescriptor&lt;<xsl:value-of select="../@name"/>&gt; item = new SortFieldDescriptor&lt;<xsl:value-of select="../@name"/>&gt;()
                        .Field(order_by)
                        .Order(descending ? SortOrder.Descending : SortOrder.Ascending);
                        
                    sortFields.Add(item);
                }
                SortFieldDescriptor&lt;<xsl:value-of select="../@name"/>&gt; defaultSort = new SortFieldDescriptor&lt;<xsl:value-of select="../@name"/>&gt;()
                    .Field(r => r.<xsl:choose><xsl:when test="string-length(../@uiDefaultSort)>0"><xsl:value-of select="../@uiDefaultSort" /></xsl:when><xsl:otherwise><xsl:value-of select="../field[1]" /></xsl:otherwise></xsl:choose>)
                    .<xsl:choose><xsl:when test="../@uiDefaultSortDescending='true'">Descending()</xsl:when><xsl:otherwise>Ascending()</xsl:otherwise></xsl:choose>;
                
                sortFields.Add(defaultSort);
                
                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse&lt;<xsl:value-of select="../@name"/>&gt; searchResponse = client.Search&lt;<xsl:value-of select="../@name"/>&gt;(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(sr => sr.Multi(sortFields))
                    .Type(this.DocumentType));

                ListResult&lt;<xsl:value-of select="../@name"/>&gt; result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                <xsl:if test="string-length(../@userSpecificData)>0">
                this.PostProcessForUser(result.items, for_<xsl:value-of select="../@userSpecificData"/>);
                </xsl:if>
                return result;
            });
        }
        </xsl:for-each>
        
        
        
        <xsl:variable name="name"><xsl:value-of select="@name"/></xsl:variable>
        
        <xsl:for-each select="../item/field[@computedFrom = $name]">
        <xsl:variable name="computedFrom"><xsl:value-of select="@computedFrom"/></xsl:variable>
        <xsl:variable name="computedField"><xsl:value-of select="@computedField" /></xsl:variable>
        <xsl:variable name="computedBy"><xsl:value-of select="@computedBy" /></xsl:variable>
        <xsl:variable name="computedType"><xsl:value-of select="@type"/></xsl:variable>
        <xsl:variable name="currentField"><xsl:value-of select="text()" /></xsl:variable>
        <xsl:variable name="primaryKey"><xsl:value-of select="../field[1]" /></xsl:variable>
        
        <xsl:for-each select="../../item[@name=$computedFrom]">
        <xsl:variable name="friendlyName"><xsl:value-of select="field[text()=$computedField]/@friendlyName" /></xsl:variable>
        <xsl:choose><xsl:when test="$computedBy='Count'">
        public <xsl:value-of select="$computedType"/> Get<xsl:value-of select="$computedBy"/><xsl:value-of select="$friendlyName"/>(Guid <xsl:value-of select="$primaryKey"/>)
        {
            return base.ExecuteFunction("Get<xsl:value-of select="$computedBy"/><xsl:value-of select="$friendlyName"/>", delegate ()
            {
                QueryContainer query = Query&lt;<xsl:value-of select="@name" />&gt;.Term(w => w.<xsl:value-of select="$primaryKey"/>, <xsl:value-of select="$primaryKey"/>);
                <xsl:if test="$computedBy='Count'">
                query &amp;= Query&lt;<xsl:value-of select="@name" />&gt;.Exists(f => f.Field(x => x.<xsl:value-of select="$computedField"/>));
               
                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse&lt;sdk.<xsl:value-of select="@name"/>&gt; response = client.Search&lt;sdk.<xsl:value-of select="@name"/>&gt;(s => s
                    .Query(q => query)
                    .Skip(0)
                    .Take(0)
                    .Type(this.DocumentType));

                 </xsl:if>
                return (int)response.GetTotalHit();
            });
        }
        </xsl:when></xsl:choose>

        </xsl:for-each>
        
        </xsl:for-each>
        
        <xsl:for-each select="../item/indexfield[@computedFrom = $name]">
        <xsl:variable name="computedFrom"><xsl:value-of select="@computedFrom"/></xsl:variable>
        <xsl:variable name="computedField"><xsl:value-of select="@computedField" /></xsl:variable>
        <xsl:variable name="computedBy"><xsl:value-of select="@computedBy" /></xsl:variable>
        <xsl:variable name="computedType"><xsl:value-of select="@type"/></xsl:variable>
        <xsl:variable name="currentField"><xsl:value-of select="text()" /></xsl:variable>
        <xsl:variable name="primaryKey"><xsl:value-of select="../field[1]" /></xsl:variable>
        
        <xsl:for-each select="../../item[@name=$computedFrom]">
        <xsl:variable name="friendlyName"><xsl:value-of select="field[text()=$computedField]/@friendlyName" /></xsl:variable>
        <xsl:choose><xsl:when test="$computedBy='Count'">
        public <xsl:value-of select="$computedType"/> Get<xsl:value-of select="$computedBy"/><xsl:value-of select="$friendlyName"/>(Guid <xsl:value-of select="$primaryKey"/>)
        {
            return base.ExecuteFunction("Get<xsl:value-of select="$computedBy"/><xsl:value-of select="$friendlyName"/>", delegate ()
            {
                QueryContainer query = Query&lt;<xsl:value-of select="@name" />&gt;.Term(w => w.<xsl:value-of select="$primaryKey"/>, <xsl:value-of select="$primaryKey"/>);
                <xsl:if test="$computedBy='Count'">
                query &amp;= Query&lt;<xsl:value-of select="@name" />&gt;.Exists(f => f.Field(x => x.<xsl:value-of select="$computedField"/>));
               
                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse&lt;sdk.<xsl:value-of select="@name"/>&gt; response = client.Search&lt;sdk.<xsl:value-of select="@name"/>&gt;(s => s
                    .Query(q => query)
                    .Skip(0)
                    .Take(0)
                    .Type(this.DocumentType));

                 </xsl:if>
                return (int)response.GetTotalHit();
            });
        }
        </xsl:when></xsl:choose>

        </xsl:for-each>
        
        </xsl:for-each>
        
        
        
        <xsl:if test="count(field[@searchable='true']) > 0 or count(indexfield[@searchable='true']) > 0">
        public ListResult&lt;sdk.<xsl:value-of select="@name"/>&gt; Find(<xsl:if test="string-length(@userSpecificData)>0">Guid? for_<xsl:value-of select="@userSpecificData"/>, </xsl:if>int skip, int take, string keyword = "", string order_by = "", bool descending = false<xsl:for-each select="field[@foreignKey]">, Guid? <xsl:value-of select="text()"/> = null</xsl:for-each><xsl:for-each select="field[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text> <xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each><xsl:for-each select="indexfield[string-length(@searchToggle)>0]">,  <xsl:value-of select="@type"/><xsl:if test="not(@type='string')">?</xsl:if><xsl:text> </xsl:text> <xsl:value-of select="text()"/> = <xsl:value-of select="@searchToggle"/></xsl:for-each>)
        {
            return base.ExecuteFunction("Find", delegate ()
            {
                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                QueryContainer query = Query&lt;sdk.<xsl:value-of select="@name"/>&gt;
                    .MultiMatch(m => m
                        .Query(keyword)
                        .Type(TextQueryType.PhrasePrefix)
                        .Fields(mf => mf<xsl:for-each select="field[@searchable='true']">
                                .Field(f => f.<xsl:value-of select="text()"/>)</xsl:for-each><xsl:for-each select="indexfield[@searchable='true']">
                                .Field(f => f.<xsl:value-of select="text()"/>)</xsl:for-each>
                ));
                                
                <xsl:variable name="searchParentName"><xsl:value-of select="@name"/></xsl:variable>
                <xsl:for-each select="field[@foreignKey]">if(<xsl:value-of select="text()"/>.HasValue)
                {
                    query &amp;= Query&lt;sdk.<xsl:value-of select="$searchParentName"/>&gt;.Term(f => f.<xsl:value-of select="text()"/>, <xsl:value-of select="text()"/>.Value);
                }
                </xsl:for-each>
                <xsl:for-each select="field[string-length(@searchToggle)>0]">if(<xsl:value-of select="text()"/>.HasValue)
                {
                    query &amp;= Query&lt;sdk.<xsl:value-of select="$searchParentName"/>&gt;.Term(f => f.<xsl:value-of select="text()"/>, <xsl:value-of select="text()"/>.Value);
                }
                </xsl:for-each>
                <xsl:for-each select="indexfield[string-length(@searchToggle)>0]">if(<xsl:value-of select="text()"/>.HasValue)
                {
                    query &amp;= Query&lt;sdk.<xsl:value-of select="$searchParentName"/>&gt;.Term(f => f.<xsl:value-of select="text()"/>, <xsl:value-of select="text()"/>.Value);
                }
                </xsl:for-each>
                
                SortOrder sortOrder = SortOrder.Ascending;
                if (descending)
                {
                    sortOrder = SortOrder.Descending;
                }
                if (string.IsNullOrEmpty(order_by))
                {
                    order_by = "<xsl:choose><xsl:when test="string-length(../@uiDefaultSort)>0"><xsl:value-of select="../@uiDefaultSort" /></xsl:when><xsl:otherwise><xsl:value-of select="../@uiDisplayField" /></xsl:otherwise></xsl:choose>";
                }

                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse&lt;sdk.<xsl:value-of select="@name"/>&gt; searchResponse = client.Search&lt;sdk.<xsl:value-of select="@name"/>&gt;(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(r => r.Field(order_by, sortOrder))
                    .Type(this.DocumentType));
                
                ListResult&lt;sdk.<xsl:value-of select="@name"/>&gt; result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                <xsl:if test="string-length(@userSpecificData)>0">
                this.PostProcessForUser(result.items, for_<xsl:value-of select="@userSpecificData"/>);
                </xsl:if>
                return result;
            });
        }
        </xsl:if>
        
        <xsl:if test="string-length(@userSpecificData)>0">
        partial void PostProcessForUser(List&lt;<xsl:value-of select="@name"/>&gt; items, Guid? <xsl:value-of select="@userSpecificData"/>);
        </xsl:if>

    }
}
'''[ENDFILE]

</xsl:for-each>

'''[STARTFILE:<xsl:value-of select="items/@projectName"/>.Primary\<xsl:value-of select="items/@projectName"/>APIDirect.cs]using Codeable.Foundation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using <xsl:value-of select="items/@projectName"/>.Primary.Business.Direct;

namespace <xsl:value-of select="items/@projectName"/>.Primary
{
    public class <xsl:value-of select="items/@projectName"/>APIDirect : BaseClass
    {
        public <xsl:value-of select="items/@projectName"/>APIDirect(IFoundation ifoundation)
            : base(ifoundation)
        {
        }
        <xsl:for-each select="items/item">public I<xsl:value-of select="@name" />Business <xsl:call-template name="Pluralize"><xsl:with-param name="inputString" select="@name"/></xsl:call-template>
        {
            get { return this.IFoundation.Resolve&lt;I<xsl:value-of select="@name" />Business&gt;(); }
        }
        </xsl:for-each>
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
  <xsl:template name="NoSpace">
          <xsl:param name="inputString"/>
          <xsl:variable name="spaces" select="' '"/>
          <xsl:variable name="underlines" select="''"/>
          <xsl:value-of select="translate($inputString,$spaces,$underlines)"/>
  </xsl:template>
  <xsl:template name="Pluralize">
          <xsl:param name="inputString"/>
          <xsl:choose><xsl:when test="substring($inputString, string-length($inputString)) = 'y'"><xsl:value-of select="concat(substring($inputString, 1, string-length($inputString)-1),'ies')"/></xsl:when><xsl:otherwise><xsl:value-of select="$inputString"/>s</xsl:otherwise></xsl:choose>
  </xsl:template>
</xsl:stylesheet>