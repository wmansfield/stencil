using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Apache.Sanselan.Formats.Jpeg;
using Java.IO;
using Org.Apache.Sanselan.Common;
using Org.Apache.Sanselan.Formats.Tiff;
using Org.Apache.Sanselan.Formats.Tiff.Constants;

namespace Org.Apache.Sanselan
{
    
    public partial class Sanselan
    {
        public static int GetRotationAngle(string sourceFilePath)
        {
            int result = 0;
            try
            {
                IImageMetadata metadata = Sanselan.GetMetadata(new File(sourceFilePath));
                if (metadata is JpegImageMetadata)
                {
                    TiffField orientationValue = ((JpegImageMetadata)metadata).FindEXIFValue(ExifTagConstants.ExifTagOrientation);
                    if (orientationValue != null)
                    {

                        int orientation = orientationValue.IntValue;
                        switch (orientation)
                        {
                            case ExifTagConstants.OrientationValueRotate90Cw:
                                result = 90;
                                break;
                            case ExifTagConstants.OrientationValueRotate180:
                                result = 180;
                                break;
                            case ExifTagConstants.OrientationValueRotate270Cw:
                                result = 270;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch
            {
                // gulp
            }
            return result;
        }


    }
}