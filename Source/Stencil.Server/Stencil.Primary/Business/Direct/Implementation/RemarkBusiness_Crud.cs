using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Domain;
using Stencil.Data.Sql;
using Stencil.Primary.Synchronization;

namespace Stencil.Primary.Business.Direct.Implementation
{
    // WARNING: THIS FILE IS GENERATED
    public partial class RemarkBusiness : BusinessBase, IRemarkBusiness
    {
        public RemarkBusiness(IFoundation foundation)
            : base(foundation, "Remark")
        {
        }
        
        protected IRemarkSynchronizer Synchronizer
        {
            get
            {
                return this.IFoundation.Resolve<IRemarkSynchronizer>();
            }
        }

        public Remark Insert(Remark insertRemark)
        {
            return base.ExecuteFunction("Insert", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    

                    this.PreProcess(insertRemark, true);
                    var interception = this.Intercept(insertRemark, true);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    if (insertRemark.remark_id == Guid.Empty)
                    {
                        insertRemark.remark_id = Guid.NewGuid();
                    }
                    insertRemark.created_utc = DateTime.UtcNow;
                    insertRemark.updated_utc = insertRemark.created_utc;

                    dbRemark dbModel = insertRemark.ToDbModel();
                    
                    dbModel.InvalidateSync(this.DefaultAgent, "insert");

                    db.dbRemarks.Add(dbModel);

                    db.SaveChanges();
                    
                    this.AfterInsertPersisted(db, dbModel);
                    
                    this.Synchronizer.SynchronizeItem(dbModel.remark_id, Availability.Searchable);
                    this.AfterInsertIndexed(db, dbModel);
                    
                    this.DependencyCoordinator.RemarkInvalidated(Dependency.None, dbModel.remark_id);
                }
                return this.GetById(insertRemark.remark_id);
            });
        }
        public Remark Update(Remark updateRemark)
        {
            return base.ExecuteFunction("Update", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    this.PreProcess(updateRemark, false);
                    var interception = this.Intercept(updateRemark, false);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    updateRemark.updated_utc = DateTime.UtcNow;
                    
                    dbRemark found = (from n in db.dbRemarks
                                    where n.remark_id == updateRemark.remark_id
                                    select n).FirstOrDefault();

                    if (found != null)
                    {
                        Remark previous = found.ToDomainModel();
                        
                        found = updateRemark.ToDbModel(found);
                        found.InvalidateSync(this.DefaultAgent, "updated");
                        db.SaveChanges();
                        
                        this.AfterUpdatePersisted(db, found, previous);
                        
                        this.Synchronizer.SynchronizeItem(found.remark_id, Availability.Searchable);
                        this.AfterUpdateIndexed(db, found);
                        
                        this.DependencyCoordinator.RemarkInvalidated(Dependency.None, found.remark_id);
                    
                    }
                    
                    return this.GetById(updateRemark.remark_id);
                }
            });
        }
        public void Delete(Guid remark_id)
        {
            base.ExecuteMethod("Delete", delegate()
            {
                
                using (var db = base.CreateSQLContext())
                {
                    dbRemark found = (from a in db.dbRemarks
                                    where a.remark_id == remark_id
                                    select a).FirstOrDefault();

                    if (found != null)
                    {
                        
                        found.deleted_utc = DateTime.UtcNow;
                        found.InvalidateSync(this.DefaultAgent, "deleted");
                        db.SaveChanges();
                        
                        this.AfterDeletePersisted(db, found);
                        
                        this.Synchronizer.SynchronizeItem(found.remark_id, Availability.Searchable);
                        
                        this.DependencyCoordinator.RemarkInvalidated(Dependency.None, found.remark_id);
                    }
                }
            });
        }
        public void SynchronizationUpdate(Guid remark_id, bool success, DateTime sync_date_utc, string sync_log)
        {
            base.ExecuteMethod("SynchronizationUpdate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.spRemark_SyncUpdate(remark_id, success, sync_date_utc, sync_log);
                }
            });
        }
        public List<Guid?> SynchronizationGetInvalid(int retryPriorityThreshold, string sync_agent)
        {
            return base.ExecuteFunction("SynchronizationGetInvalid", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    return db.spRemark_SyncGetInvalid(retryPriorityThreshold, sync_agent).ToList();
                }
            });
        }
        public void SynchronizationHydrateUpdate(Guid remark_id, bool success, DateTime sync_date_utc, string sync_log)
        {
            base.ExecuteMethod("SynchronizationHydrateUpdate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.spRemark_HydrateSyncUpdate(remark_id, success, sync_date_utc, sync_log);
                }
            });
        }
        public List<Guid?> SynchronizationHydrateGetInvalid(int retryPriorityThreshold, string sync_agent)
        {
            return base.ExecuteFunction("SynchronizationHydrateGetInvalid", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    return db.spRemark_HydrateSyncGetInvalid(retryPriorityThreshold, sync_agent).ToList();
                }
            });
        }
        
        public Remark GetById(Guid remark_id)
        {
            return base.ExecuteFunction("GetById", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    dbRemark result = (from n in db.dbRemarks
                                     where (n.remark_id == remark_id)
                                     select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        public List<Remark> GetByPost(Guid post_id)
        {
            return base.ExecuteFunction("GetByPost", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    var result = (from n in db.dbRemarks
                                     where (n.post_id == post_id)
                                     orderby n.stamp_utc
                                     select n);
                    return result.ToDomainModel();
                }
            });
        }
        
        public List<Remark> GetByAccount(Guid account_id)
        {
            return base.ExecuteFunction("GetByAccount", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    var result = (from n in db.dbRemarks
                                     where (n.account_id == account_id)
                                     orderby n.stamp_utc
                                     select n);
                    return result.ToDomainModel();
                }
            });
        }
        
        
        public void Invalidate(Guid remark_id, string reason)
        {
            base.ExecuteMethod("Invalidate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.dbRemarks
                        .Where(x => x.remark_id == remark_id)
                        .Update(x => new dbRemark() {
                            sync_success_utc = null,
                            sync_hydrate_utc = null,
                            sync_invalid_utc = DateTime.UtcNow,
                            sync_log = reason
                        });
                }
            });
        }
        public List<Remark> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction("Find", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    if(string.IsNullOrEmpty(keyword))
                    { 
                        keyword = ""; 
                    }

                    var data = (from p in db.dbRemarks
                                where (keyword == "" 
                                    || p.text.Contains(keyword)
                                )
                                select p);

                    List<dbRemark> result = new List<dbRemark>();

                    switch (order_by)
                    {
                        default:
                            if (!descending)
                            {
                                result = data.OrderBy(s => s.stamp_utc).Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                result = data.OrderByDescending(s => s.stamp_utc).Skip(skip).Take(take).ToList();
                            }
                            
                            break;
                    }
                    return result.ToDomainModel();
                }
            });
        }
        


        
        
        public InterceptArgs<Remark> Intercept(Remark remark, bool forInsert)
        {
            InterceptArgs<Remark> args = new InterceptArgs<Remark>()
            {
                ForInsert = forInsert,
                ReturnEntity = remark
            };
            this.PerformIntercept(args);
            return args;
        }

        partial void PerformIntercept(InterceptArgs<Remark> args);
        partial void PreProcess(Remark remark, bool forInsert);
        partial void AfterInsertPersisted(StencilContext db, dbRemark remark);
        partial void AfterUpdatePersisted(StencilContext db, dbRemark remark, Remark previous);
        partial void AfterDeletePersisted(StencilContext db, dbRemark remark);
        partial void AfterUpdateIndexed(StencilContext db, dbRemark remark);
        partial void AfterInsertIndexed(StencilContext db, dbRemark remark);
    }
}

