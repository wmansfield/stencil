using am = AutoMapper;
using Codeable.Foundation.Core;
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
        
        public static GlobalSetting ToDomainModel(this SDK.Models.GlobalSetting entity, GlobalSetting destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new Domain.GlobalSetting(); }
                GlobalSetting result = am.Mapper.Map<SDK.Models.GlobalSetting, GlobalSetting>(entity, destination);
                return result;
            }
            return null;
        }
        public static SDK.Models.GlobalSetting ToSDKModel(this GlobalSetting entity, SDK.Models.GlobalSetting destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new SDK.Models.GlobalSetting(); }
                SDK.Models.GlobalSetting result = am.Mapper.Map<GlobalSetting, SDK.Models.GlobalSetting>(entity, destination);
                return result;
            }
            return null;
        }
        public static List<SDK.Models.GlobalSetting> ToSDKModel(this IEnumerable<GlobalSetting> entities)
        {
            List<SDK.Models.GlobalSetting> result = new List<SDK.Models.GlobalSetting>();
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    result.Add(item.ToSDKModel());
                }
            }
            return result;
        }
        
        
        
        public static Account ToDomainModel(this SDK.Models.Account entity, Account destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new Domain.Account(); }
                Account result = am.Mapper.Map<SDK.Models.Account, Account>(entity, destination);
                return result;
            }
            return null;
        }
        public static SDK.Models.Account ToSDKModel(this Account entity, SDK.Models.Account destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new SDK.Models.Account(); }
                SDK.Models.Account result = am.Mapper.Map<Account, SDK.Models.Account>(entity, destination);
                return result;
            }
            return null;
        }
        public static List<SDK.Models.Account> ToSDKModel(this IEnumerable<Account> entities)
        {
            List<SDK.Models.Account> result = new List<SDK.Models.Account>();
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    result.Add(item.ToSDKModel());
                }
            }
            return result;
        }
        
        
        
        public static Asset ToDomainModel(this SDK.Models.Asset entity, Asset destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new Domain.Asset(); }
                Asset result = am.Mapper.Map<SDK.Models.Asset, Asset>(entity, destination);
                return result;
            }
            return null;
        }
        public static SDK.Models.Asset ToSDKModel(this Asset entity, SDK.Models.Asset destination = null)
        {
            if (entity != null)
            {
                if (destination == null) { destination = new SDK.Models.Asset(); }
                SDK.Models.Asset result = am.Mapper.Map<Asset, SDK.Models.Asset>(entity, destination);
                return result;
            }
            return null;
        }
        public static List<SDK.Models.Asset> ToSDKModel(this IEnumerable<Asset> entities)
        {
            List<SDK.Models.Asset> result = new List<SDK.Models.Asset>();
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    result.Add(item.ToSDKModel());
                }
            }
            return result;
        }
        
        
        
    }
}

