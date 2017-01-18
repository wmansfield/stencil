using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Domain;
using sdk = Stencil.SDK.Models;
using AutoMapper;

namespace Stencil.Primary.Mapping
{
    public static class _SDKModelExtensions
    {
        public static sdk.Responses.AccountInfo ToInfoModel(this Account entity)
        {
            if (entity != null)
            {
                sdk.Responses.AccountInfo response = Mapper.Map<Account, sdk.Responses.AccountInfo>(entity);
                response.admin = entity.IsAdmin();
                return response;
            }
            return null;
        }
    }
}
