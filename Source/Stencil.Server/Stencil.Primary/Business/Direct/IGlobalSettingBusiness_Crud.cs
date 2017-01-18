using System;
using System.Collections.Generic;
using System.Text;
using Stencil.Domain;

namespace Stencil.Primary.Business.Direct
{
    // WARNING: THIS FILE IS GENERATED
    public partial interface IGlobalSettingBusiness
    {
        GlobalSetting GetById(Guid global_setting_id);
        List<GlobalSetting> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false);
        GlobalSetting Insert(GlobalSetting insertGlobalSetting);
        GlobalSetting Update(GlobalSetting updateGlobalSetting);
        
        void Delete(Guid global_setting_id);
        
        
    }
}

