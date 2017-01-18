using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.App.Config
{
    public class AppPreferences
    {
        public AppPreferences()
        {
        }
        public DateTime? LastTranslationCheck { get; set; }
        public string PreferredCulture { get; set; }
    }
}
