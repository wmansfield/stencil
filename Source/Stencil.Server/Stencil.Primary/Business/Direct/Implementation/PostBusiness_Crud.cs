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
    public partial class PostBusiness : BusinessBase, IPostBusiness
    {
        public PostBusiness(IFoundation foundation)
            : base(foundation, "Post")
        {
        }
        
        protected IPostSynchronizer Synchronizer
        {
            get
            {
                return this.IFoundation.Resolve<IPostSynchronizer>();
            }
        }

        public Post Insert(Post insertPost)
        {
            return base.ExecuteFunction("Insert", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    

                    this.PreProcess(insertPost, true);
                    var interception = this.Intercept(insertPost, true);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    if (insertPost.post_id == Guid.Empty)
                    {
                        insertPost.post_id = Guid.NewGuid();
                    }
                    insertPost.created_utc = DateTime.UtcNow;
                    insertPost.updated_utc = insertPost.created_utc;

                    dbPost dbModel = insertPost.ToDbModel();
                    
                    dbModel.InvalidateSync(this.DefaultAgent, "insert");

                    db.dbPosts.Add(dbModel);

                    db.SaveChanges();
                    
                    this.AfterInsertPersisted(db, dbModel);
                    
                    this.Synchronizer.SynchronizeItem(dbModel.post_id, Availability.Searchable);
                    this.AfterInsertIndexed(db, dbModel);
                    
                    this.DependencyCoordinator.PostInvalidated(Dependency.None, dbModel.post_id);
                }
                return this.GetById(insertPost.post_id);
            });
        }
        public Post Update(Post updatePost)
        {
            return base.ExecuteFunction("Update", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    this.PreProcess(updatePost, false);
                    var interception = this.Intercept(updatePost, false);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    updatePost.updated_utc = DateTime.UtcNow;
                    
                    dbPost found = (from n in db.dbPosts
                                    where n.post_id == updatePost.post_id
                                    select n).FirstOrDefault();

                    if (found != null)
                    {
                        Post previous = found.ToDomainModel();
                        
                        found = updatePost.ToDbModel(found);
                        found.InvalidateSync(this.DefaultAgent, "updated");
                        db.SaveChanges();
                        
                        this.AfterUpdatePersisted(db, found, previous);
                        
                        this.Synchronizer.SynchronizeItem(found.post_id, Availability.Searchable);
                        this.AfterUpdateIndexed(db, found);
                        
                        this.DependencyCoordinator.PostInvalidated(Dependency.None, found.post_id);
                    
                    }
                    
                    return this.GetById(updatePost.post_id);
                }
            });
        }
        public void Delete(Guid post_id)
        {
            base.ExecuteMethod("Delete", delegate()
            {
                
                using (var db = base.CreateSQLContext())
                {
                    dbPost found = (from a in db.dbPosts
                                    where a.post_id == post_id
                                    select a).FirstOrDefault();

                    if (found != null)
                    {
                        
                        found.deleted_utc = DateTime.UtcNow;
                        found.InvalidateSync(this.DefaultAgent, "deleted");
                        db.SaveChanges();
                        
                        this.AfterDeletePersisted(db, found);
                        
                        this.Synchronizer.SynchronizeItem(found.post_id, Availability.Searchable);
                        
                        this.DependencyCoordinator.PostInvalidated(Dependency.None, found.post_id);
                    }
                }
            });
        }
        public void SynchronizationUpdate(Guid post_id, bool success, DateTime sync_date_utc, string sync_log)
        {
            base.ExecuteMethod("SynchronizationUpdate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.spPost_SyncUpdate(post_id, success, sync_date_utc, sync_log);
                }
            });
        }
        public List<Guid?> SynchronizationGetInvalid(int retryPriorityThreshold, string sync_agent)
        {
            return base.ExecuteFunction("SynchronizationGetInvalid", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    return db.spPost_SyncGetInvalid(retryPriorityThreshold, sync_agent).ToList();
                }
            });
        }
        public void SynchronizationHydrateUpdate(Guid post_id, bool success, DateTime sync_date_utc, string sync_log)
        {
            base.ExecuteMethod("SynchronizationHydrateUpdate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.spPost_HydrateSyncUpdate(post_id, success, sync_date_utc, sync_log);
                }
            });
        }
        public List<Guid?> SynchronizationHydrateGetInvalid(int retryPriorityThreshold, string sync_agent)
        {
            return base.ExecuteFunction("SynchronizationHydrateGetInvalid", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    return db.spPost_HydrateSyncGetInvalid(retryPriorityThreshold, sync_agent).ToList();
                }
            });
        }
        
        public Post GetById(Guid post_id)
        {
            return base.ExecuteFunction("GetById", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    dbPost result = (from n in db.dbPosts
                                     where (n.post_id == post_id)
                                     select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        public List<Post> GetByAccount(Guid account_id)
        {
            return base.ExecuteFunction("GetByAccount", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    var result = (from n in db.dbPosts
                                     where (n.account_id == account_id)
                                     orderby n.stamp_utc
                                     select n);
                    return result.ToDomainModel();
                }
            });
        }
        
        
        public void Invalidate(Guid post_id, string reason)
        {
            base.ExecuteMethod("Invalidate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.dbPosts
                        .Where(x => x.post_id == post_id)
                        .Update(x => new dbPost() {
                            sync_success_utc = null,
                            sync_hydrate_utc = null,
                            sync_invalid_utc = DateTime.UtcNow,
                            sync_log = reason
                        });
                }
            });
        }
        public List<Post> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction("Find", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    if(string.IsNullOrEmpty(keyword))
                    { 
                        keyword = ""; 
                    }

                    var data = (from p in db.dbPosts
                                where (keyword == "" 
                                    || p.body.Contains(keyword)
                                )
                                select p);

                    List<dbPost> result = new List<dbPost>();

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
        


        
        
        public InterceptArgs<Post> Intercept(Post post, bool forInsert)
        {
            InterceptArgs<Post> args = new InterceptArgs<Post>()
            {
                ForInsert = forInsert,
                ReturnEntity = post
            };
            this.PerformIntercept(args);
            return args;
        }

        partial void PerformIntercept(InterceptArgs<Post> args);
        partial void PreProcess(Post post, bool forInsert);
        partial void AfterInsertPersisted(StencilContext db, dbPost post);
        partial void AfterUpdatePersisted(StencilContext db, dbPost post, Post previous);
        partial void AfterDeletePersisted(StencilContext db, dbPost post);
        partial void AfterUpdateIndexed(StencilContext db, dbPost post);
        partial void AfterInsertIndexed(StencilContext db, dbPost post);
    }
}

