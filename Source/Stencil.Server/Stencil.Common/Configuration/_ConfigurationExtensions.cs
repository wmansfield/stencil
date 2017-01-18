using Stencil.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common
{
    public static class _ConfigurationExtensions
    {
        public static bool IsLocalHost(this ISettingsResolver settingsResolver)
        {
            if (settingsResolver != null)
            {
                try
                {
                    return bool.Parse(settingsResolver.GetSetting("IsLocalHost"));
                }
                catch { }
            }
            return false;
        }

        public static bool IsBackPane(this ISettingsResolver settingsResolver)
        {
            return settingsResolver.GetSetting(CommonAssumptions.APP_KEY_IS_BACKING) == "true";
        }

        public static bool IsHydrate(this ISettingsResolver settingsResolver)
        {
            return settingsResolver.GetSetting(CommonAssumptions.APP_KEY_IS_HYDRATE) == "true";
        }
    }
}
