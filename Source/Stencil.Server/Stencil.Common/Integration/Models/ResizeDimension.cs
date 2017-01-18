using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Integration
{
    public class ResizeDimension
    {
        #region Static Methods

        public static ResizeDimension FromConfig(string csv)
        {
            if (!string.IsNullOrEmpty(csv))
            {
                string[] values = csv.Split(',');
                return ResizeDimension.FromConfig(values);
            }
            return new ResizeDimension();
        }
        public static ResizeDimension FromConfig(string[] smallMediumLarge)
        {
            ResizeDimension result = new ResizeDimension();
            if (smallMediumLarge != null)
            {
                if (smallMediumLarge.Length > 0)
                {
                    result.Small = smallMediumLarge[0].Trim();
                }
                if (smallMediumLarge.Length > 1)
                {
                    result.Medium = smallMediumLarge[1].Trim();
                }
                if (smallMediumLarge.Length > 2)
                {
                    result.Large = smallMediumLarge[2].Trim();
                }
            }
            return result;
        }

        #endregion

        #region Public Properties

        public virtual string Large { get; set; }
        public virtual string Medium { get; set; }
        public virtual string Small { get; set; }

        #endregion
    }
}
