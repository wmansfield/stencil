using Stencil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Direct
{
    public partial interface IGlobalSettingBusiness
    {
        GlobalSetting GetByName(string name);
        List<GlobalSetting> FindWithPrefix(string prefix);
    }
}
