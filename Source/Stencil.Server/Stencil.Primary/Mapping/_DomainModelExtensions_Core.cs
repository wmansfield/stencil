using am = AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Data.Sql;
using Stencil.Domain;

namespace Stencil.Primary
{
    public static partial class _DomainModelExtensions
    {
        
        public static dbGlobalSetting ToDbModel(this GlobalSetting entity, dbGlobalSetting destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new dbGlobalSetting(); }
                return am.Mapper.Map<GlobalSetting, dbGlobalSetting>(entity, destination);
            }
            return null;
        }
        public static GlobalSetting ToDomainModel(this dbGlobalSetting entity, GlobalSetting destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new GlobalSetting(); }
                return am.Mapper.Map<dbGlobalSetting, GlobalSetting>(entity, destination);
            }
            return null;
        }
        public static List<GlobalSetting> ToDomainModel(this IEnumerable<dbGlobalSetting> entities)
        {
            List<GlobalSetting> result = new List<GlobalSetting>();
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    result.Add(item.ToDomainModel());
                }
            }
            return result;
        }
        
        
        
        public static dbAccount ToDbModel(this Account entity, dbAccount destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new dbAccount(); }
                return am.Mapper.Map<Account, dbAccount>(entity, destination);
            }
            return null;
        }
        public static Account ToDomainModel(this dbAccount entity, Account destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new Account(); }
                return am.Mapper.Map<dbAccount, Account>(entity, destination);
            }
            return null;
        }
        public static List<Account> ToDomainModel(this IEnumerable<dbAccount> entities)
        {
            List<Account> result = new List<Account>();
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    result.Add(item.ToDomainModel());
                }
            }
            return result;
        }
        
        
        
        public static dbAsset ToDbModel(this Asset entity, dbAsset destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new dbAsset(); }
                return am.Mapper.Map<Asset, dbAsset>(entity, destination);
            }
            return null;
        }
        public static Asset ToDomainModel(this dbAsset entity, Asset destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new Asset(); }
                return am.Mapper.Map<dbAsset, Asset>(entity, destination);
            }
            return null;
        }
        public static List<Asset> ToDomainModel(this IEnumerable<dbAsset> entities)
        {
            List<Asset> result = new List<Asset>();
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    result.Add(item.ToDomainModel());
                }
            }
            return result;
        }
        
        
        
    }
}

