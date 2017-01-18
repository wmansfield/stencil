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
    public partial class AccountBusiness : BusinessBase, IAccountBusiness
    {
        public AccountBusiness(IFoundation foundation)
            : base(foundation, "Account")
        {
        }
        
        protected IAccountSynchronizer Synchronizer
        {
            get
            {
                return this.IFoundation.Resolve<IAccountSynchronizer>();
            }
        }

        public Account Insert(Account insertAccount)
        {
            return base.ExecuteFunction("Insert", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    

                    this.PreProcess(insertAccount, true);
                    var interception = this.Intercept(insertAccount, true);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    if (insertAccount.account_id == Guid.Empty)
                    {
                        insertAccount.account_id = Guid.NewGuid();
                    }
                    insertAccount.created_utc = DateTime.UtcNow;
                    insertAccount.updated_utc = insertAccount.created_utc;

                    dbAccount dbModel = insertAccount.ToDbModel();
                    
                    dbModel.InvalidateSync(this.DefaultAgent, "insert");

                    db.dbAccounts.Add(dbModel);

                    db.SaveChanges();
                    
                    this.AfterInsertPersisted(db, dbModel);
                    
                    this.Synchronizer.SynchronizeItem(dbModel.account_id, Availability.Retrievable);
                    this.AfterInsertIndexed(db, dbModel);
                    
                    this.DependencyCoordinator.AccountInvalidated(Dependency.None, dbModel.account_id);
                }
                return this.GetById(insertAccount.account_id);
            });
        }
        public Account Update(Account updateAccount)
        {
            return base.ExecuteFunction("Update", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    this.PreProcess(updateAccount, false);
                    var interception = this.Intercept(updateAccount, false);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    updateAccount.updated_utc = DateTime.UtcNow;
                    
                    dbAccount found = (from n in db.dbAccounts
                                    where n.account_id == updateAccount.account_id
                                    select n).FirstOrDefault();

                    if (found != null)
                    {
                        Account previous = found.ToDomainModel();
                        
                        found = updateAccount.ToDbModel(found);
                        found.InvalidateSync(this.DefaultAgent, "updated");
                        db.SaveChanges();
                        
                        this.AfterUpdatePersisted(db, found, previous);
                        
                        this.Synchronizer.SynchronizeItem(found.account_id, Availability.Retrievable);
                        this.AfterUpdateIndexed(db, found);
                        
                        this.DependencyCoordinator.AccountInvalidated(Dependency.None, found.account_id);
                    
                    }
                    
                    return this.GetById(updateAccount.account_id);
                }
            });
        }
        public void Delete(Guid account_id)
        {
            base.ExecuteMethod("Delete", delegate()
            {
                
                using (var db = base.CreateSQLContext())
                {
                    dbAccount found = (from a in db.dbAccounts
                                    where a.account_id == account_id
                                    select a).FirstOrDefault();

                    if (found != null)
                    {
                        
                        found.deleted_utc = DateTime.UtcNow;
                        found.InvalidateSync(this.DefaultAgent, "deleted");
                        db.SaveChanges();
                        
                        this.AfterDeletePersisted(db, found);
                        
                        this.Synchronizer.SynchronizeItem(found.account_id, Availability.Retrievable);
                        
                        this.DependencyCoordinator.AccountInvalidated(Dependency.None, found.account_id);
                    }
                }
            });
        }
        public void SynchronizationUpdate(Guid account_id, bool success, DateTime sync_date_utc, string sync_log)
        {
            base.ExecuteMethod("SynchronizationUpdate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.spAccount_SyncUpdate(account_id, success, sync_date_utc, sync_log);
                }
            });
        }
        public List<Guid?> SynchronizationGetInvalid(int retryPriorityThreshold, string sync_agent)
        {
            return base.ExecuteFunction("SynchronizationGetInvalid", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    return db.spAccount_SyncGetInvalid(retryPriorityThreshold, sync_agent).ToList();
                }
            });
        }
        public void SynchronizationHydrateUpdate(Guid account_id, bool success, DateTime sync_date_utc, string sync_log)
        {
            base.ExecuteMethod("SynchronizationHydrateUpdate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.spAccount_HydrateSyncUpdate(account_id, success, sync_date_utc, sync_log);
                }
            });
        }
        public List<Guid?> SynchronizationHydrateGetInvalid(int retryPriorityThreshold, string sync_agent)
        {
            return base.ExecuteFunction("SynchronizationHydrateGetInvalid", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    return db.spAccount_HydrateSyncGetInvalid(retryPriorityThreshold, sync_agent).ToList();
                }
            });
        }
        
        public Account GetById(Guid account_id)
        {
            return base.ExecuteFunction("GetById", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    dbAccount result = (from n in db.dbAccounts
                                     where (n.account_id == account_id)
                                     select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        
        public void Invalidate(Guid account_id, string reason)
        {
            base.ExecuteMethod("Invalidate", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    db.dbAccounts
                        .Where(x => x.account_id == account_id)
                        .Update(x => new dbAccount() {
                            sync_success_utc = null,
                            sync_hydrate_utc = null,
                            sync_invalid_utc = DateTime.UtcNow,
                            sync_log = reason
                        });
                }
            });
        }
        public List<Account> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction("Find", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    if(string.IsNullOrEmpty(keyword))
                    { 
                        keyword = ""; 
                    }

                    var data = (from p in db.dbAccounts
                                where (keyword == "" 
                                    || p.email.Contains(keyword)
                                )
                                select p);

                    List<dbAccount> result = new List<dbAccount>();

                    switch (order_by)
                    {
                        case "email":
                            if (!descending)
                            {
                                result = data.OrderBy(s => s.email).Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                result = data.OrderByDescending(s => s.email).Skip(skip).Take(take).ToList();
                            }
                            break;
                        
                        case "first_name":
                            if (!descending)
                            {
                                result = data.OrderBy(s => s.first_name).Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                result = data.OrderByDescending(s => s.first_name).Skip(skip).Take(take).ToList();
                            }
                            break;
                        
                        case "last_name":
                            if (!descending)
                            {
                                result = data.OrderBy(s => s.last_name).Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                result = data.OrderByDescending(s => s.last_name).Skip(skip).Take(take).ToList();
                            }
                            break;
                        
                        case "last_login_utc":
                            if (!descending)
                            {
                                result = data.OrderBy(s => s.last_login_utc).Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                result = data.OrderByDescending(s => s.last_login_utc).Skip(skip).Take(take).ToList();
                            }
                            break;
                        
                        case "last_login_platform":
                            if (!descending)
                            {
                                result = data.OrderBy(s => s.last_login_platform).Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                result = data.OrderByDescending(s => s.last_login_platform).Skip(skip).Take(take).ToList();
                            }
                            break;
                        
                        default:
                            result = data.OrderBy(s => s.account_id).Skip(skip).Take(take).ToList();
                            break;
                    }
                    return result.ToDomainModel();
                }
            });
        }
        


        
        
        public InterceptArgs<Account> Intercept(Account account, bool forInsert)
        {
            InterceptArgs<Account> args = new InterceptArgs<Account>()
            {
                ForInsert = forInsert,
                ReturnEntity = account
            };
            this.PerformIntercept(args);
            return args;
        }

        partial void PerformIntercept(InterceptArgs<Account> args);
        partial void PreProcess(Account account, bool forInsert);
        partial void AfterInsertPersisted(StencilContext db, dbAccount account);
        partial void AfterUpdatePersisted(StencilContext db, dbAccount account, Account previous);
        partial void AfterDeletePersisted(StencilContext db, dbAccount account);
        partial void AfterUpdateIndexed(StencilContext db, dbAccount account);
        partial void AfterInsertIndexed(StencilContext db, dbAccount account);
    }
}

