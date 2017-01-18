using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Configuration
{
    public interface ISettingsResolver
    {
        string GetSetting(string name);
    }
}
