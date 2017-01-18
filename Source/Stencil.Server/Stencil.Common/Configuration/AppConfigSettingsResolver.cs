using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Configuration
{
    public class AppConfigSettingsResolver : ISettingsResolver
    {
        public AppConfigSettingsResolver()
        {

        }

        protected static Dictionary<string, string> SettingsCache = new Dictionary<string, string>();

        public virtual string GetSetting(string name)
        {
            if (SettingsCache.ContainsKey(name))
            {
                return SettingsCache[name];
            }
            string result = this.PerformGetSetting(name);
            if (!string.IsNullOrEmpty(result))
            {
                SettingsCache[name] = result;
            }
            return result;
        }
        protected virtual string PerformGetSetting(string name)
        {
            if (ConfigurationManager.ConnectionStrings[name] != null)
            {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString.ToString();
            }
            return ConfigurationManager.AppSettings[name];
        }
    }
}
