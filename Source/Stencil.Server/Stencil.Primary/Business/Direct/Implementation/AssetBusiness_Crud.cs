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
    public partial class AssetBusiness : BusinessBase, IAssetBusiness
    {
        public AssetBusiness(IFoundation foundation)
            : base(foundation, "Asset")
        {
        }
        
        

        public Asset Insert(Asset insertAsset)
        {
            return base.ExecuteFunction("Insert", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    

                    this.PreProcess(insertAsset, true);
                    var interception = this.Intercept(insertAsset, true);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    if (insertAsset.asset_id == Guid.Empty)
                    {
                        insertAsset.asset_id = Guid.NewGuid();
                    }
                    insertAsset.created_utc = DateTime.UtcNow;
                    insertAsset.updated_utc = insertAsset.created_utc;

                    dbAsset dbModel = insertAsset.ToDbModel();
                    
                    

                    db.dbAssets.Add(dbModel);

                    db.SaveChanges();
                    
                    this.AfterInsertPersisted(db, dbModel);
                    
                    
                    this.DependencyCoordinator.AssetInvalidated(Dependency.None, dbModel.asset_id);
                }
                return this.GetById(insertAsset.asset_id);
            });
        }
        public Asset Update(Asset updateAsset)
        {
            return base.ExecuteFunction("Update", delegate()
            {
                using (var db = base.CreateSQLContext())
                {
                    this.PreProcess(updateAsset, false);
                    var interception = this.Intercept(updateAsset, false);
                    if(interception.Intercepted)
                    {
                        return interception.ReturnEntity;
                    }
                    
                    updateAsset.updated_utc = DateTime.UtcNow;
                    
                    dbAsset found = (from n in db.dbAssets
                                    where n.asset_id == updateAsset.asset_id
                                    select n).FirstOrDefault();

                    if (found != null)
                    {
                        Asset previous = found.ToDomainModel();
                        
                        found = updateAsset.ToDbModel(found);
                        
                        db.SaveChanges();
                        
                        this.AfterUpdatePersisted(db, found, previous);
                        
                        
                        this.DependencyCoordinator.AssetInvalidated(Dependency.None, found.asset_id);
                    
                    }
                    
                    return this.GetById(updateAsset.asset_id);
                }
            });
        }
        public void Delete(Guid asset_id)
        {
            base.ExecuteMethod("Delete", delegate()
            {
                
                using (var db = base.CreateSQLContext())
                {
                    dbAsset found = (from a in db.dbAssets
                                    where a.asset_id == asset_id
                                    select a).FirstOrDefault();

                    if (found != null)
                    {
                        
                        db.dbAssets.Remove(found);
                        
                        db.SaveChanges();
                        
                        this.AfterDeletePersisted(db, found);
                        
                        
                        this.DependencyCoordinator.AssetInvalidated(Dependency.None, found.asset_id);
                    }
                }
            });
        }
        
        public Asset GetById(Guid asset_id)
        {
            return base.ExecuteFunction("GetById", delegate()
            {
                using (var db = this.CreateSQLContext())
                {
                    dbAsset result = (from n in db.dbAssets
                                     where (n.asset_id == asset_id)
                                     select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        
        


        
        
        public InterceptArgs<Asset> Intercept(Asset asset, bool forInsert)
        {
            InterceptArgs<Asset> args = new InterceptArgs<Asset>()
            {
                ForInsert = forInsert,
                ReturnEntity = asset
            };
            this.PerformIntercept(args);
            return args;
        }

        partial void PerformIntercept(InterceptArgs<Asset> args);
        partial void PreProcess(Asset asset, bool forInsert);
        partial void AfterInsertPersisted(StencilContext db, dbAsset asset);
        partial void AfterUpdatePersisted(StencilContext db, dbAsset asset, Asset previous);
        partial void AfterDeletePersisted(StencilContext db, dbAsset asset);
        
    }
}

