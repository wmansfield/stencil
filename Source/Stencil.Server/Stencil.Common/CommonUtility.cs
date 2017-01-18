using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stencil.Common
{
    public static class CommonUtility
    {
        #region File Helpers

        private static Regex _fileNameOnlyCharacters = new Regex("[^a-zA-Z0-9\\._-]");
        public static string CleanFileName(string item)
        {
            if (!string.IsNullOrEmpty(item))
            {
                return _fileNameOnlyCharacters.Replace(item, "");
            }
            return string.Empty;
        }

        #endregion
    }
}
