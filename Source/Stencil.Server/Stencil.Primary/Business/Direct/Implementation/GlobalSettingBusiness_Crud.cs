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
    public partial class GlobalSettingBusiness : BusinessBase, IGlobalSettingBusiness
    {
        public GlobalSettingBusiness(IFoundation foundation)
            : base(foundation, "GlobalSetting")
        {
        }
        
        

        public GlobalSetting Insert(GlobalSetting insertGlobalSetting)
        {
            return base.ExecuteFunction("Insert", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    

                    this.PreProcess(insertGlobalSetting, true);
                    var interception = this.Intercept(insertGlobalSetting, true);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    if (insertGlobalSetting.global_setting_id == Guid.Empty)
                    {
                        insertGlobalSetting.global_setting_id = Guid.NewGuid();
                    }
                    

                    dbGlobalSetting dbModel = insertGlobalSetting.ToDbModel();
                    
                    

                    db.dbGlobalSettings.Add(dbModel);

                    db.SaveChanges();
                    
                    this.AfterInsertPersisted(db, dbModel);
                    
                    
                    this.DependencyCoordinator.GlobalSettingInvalidated(Dependency.None, dbModel.global_setting_id);
                }
                return this.GetById(insertGlobalSetting.global_setting_id);
            });
        }
        public GlobalSetting Update(GlobalSetting updateGlobalSetting)
        {
            return base.ExecuteFunction("Update", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    this.PreProcess(updateGlobalSetting, false);
                    var interception = this.Intercept(updateGlobalSetting, false);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    
                    
                    dbGlobalSetting found = (from n in db.dbGlobalSettings
                                    where n.global_setting_id == updateGlobalSetting.global_setting_id
                                    select n).FirstOrDefault();

                    if (found != null)
                    {
                        GlobalSetting previous = found.ToDomainModel();
                        
                        found = updateGlobalSetting.ToDbModel(found);
                        
                        db.SaveChanges();
                        
                        this.AfterUpdatePersisted(db, found, previous);
                        
                        
                        this.DependencyCoordinator.GlobalSettingInvalidated(Dependency.None, found.global_setting_id);
                    
                    }
                    
                    return this.GetById(updateGlobalSetting.global_setting_id);
                }
            });
        }
        public void Delete(Guid global_setting_id)
        {
            base.ExecuteMethod("Delete", delegate()
            {
                
                using (var db = base.CreateSQLContext())
                {
                    dbGlobalSetting found = (from a in db.dbGlobalSettings
                                    where a.global_setting_id == global_setting_id
                                    select a).FirstOrDefault();

                    if (found != null)
                    {
                        
                        db.dbGlobalSettings.Remove(found);
                        
                        db.SaveChanges();
                        
                        this.AfterDeletePersisted(db, found);
                        
                        
                        this.DependencyCoordinator.GlobalSettingInvalidated(Dependency.None, found.global_setting_id);
                    }
                }
            });
        }
        
        public GlobalSetting GetById(Guid global_setting_id)
        {
            return base.ExecuteFunction("GetById", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    dbGlobalSetting result = (from n in db.dbGlobalSettings
                                     where (n.global_setting_id == global_setting_id)
                                     select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        public List<GlobalSetting> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction("Find", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    if(string.IsNullOrEmpty(keyword))
                    { 
                        keyword = ""; 
                    }

                    var data = (from p in db.dbGlobalSettings
                                where (keyword == "" 
                                    || p.name.Contains(keyword)
                                )
                                select p);

                    List<dbGlobalSetting> result = new List<dbGlobalSetting>();

                    switch (order_by)
                    {
                        default:
                            result = data.OrderBy(s => s.global_setting_id).Skip(skip).Take(take).ToList();
                            break;
                    }
                    return result.ToDomainModel();
                }
            });
        }
        


        
        
        public InterceptArgs<GlobalSetting> Intercept(GlobalSetting globalsetting, bool forInsert)
        {
            InterceptArgs<GlobalSetting> args = new InterceptArgs<GlobalSetting>()
            {
                ForInsert = forInsert,
                ReturnEntity = globalsetting
            };
            this.PerformIntercept(args);
            return args;
        }

        partial void PerformIntercept(InterceptArgs<GlobalSetting> args);
        partial void PreProcess(GlobalSetting globalsetting, bool forInsert);
        partial void AfterInsertPersisted(StencilContext db, dbGlobalSetting globalsetting);
        partial void AfterUpdatePersisted(StencilContext db, dbGlobalSetting globalsetting, GlobalSetting previous);
        partial void AfterDeletePersisted(StencilContext db, dbGlobalSetting globalsetting);
        
    }
}

