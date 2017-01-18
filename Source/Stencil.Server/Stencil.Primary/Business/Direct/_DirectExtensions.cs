using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Domain;
using Stencil.Primary.Business.Direct;

namespace Stencil.Primary
{
    public static class _DirectExtensions
    {
        public static string GetValueOrDefault(this IGlobalSettingBusiness globalSettings, string name, string defaultValue)
        {
            string result = defaultValue;
            GlobalSetting setting = globalSettings.GetByName(name);
            if (setting != null && !string.IsNullOrEmpty(setting.value))
            {
                result = setting.value;
            }
            return result;
        }
    }
}
