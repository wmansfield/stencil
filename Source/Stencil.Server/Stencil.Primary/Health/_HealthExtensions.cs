using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Health
{
    public static class _HealthExtensions
    {
        public static string FriendlyName(this Type type)
        {
            return Path.GetExtension(type.ToString()).Trim('.').ToLower();
        }
    }
}
