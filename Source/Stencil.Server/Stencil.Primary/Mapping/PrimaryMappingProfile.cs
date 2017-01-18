using am = AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Mapping
{
    public partial class PrimaryMappingProfile
    {
        partial void DbAndDomainMappings_Manual()
        {

        }
        partial void DomainAndSDKMappings_Manual()
        {
            am.Mapper.CreateMap<Domain.Account, SDK.Models.Responses.AccountInfo>();
        }
    }
}

