using System;
using UIKit;
using CoreGraphics;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core
{
    public static class IOSUtility
    {
        public static UIImage CreateMaskedImage(string maskImageBundleName, string backgroundColor)
        {
            return CreateMaskedImage(maskImageBundleName, backgroundColor.ConvertHexToColor());
        }
        public static UIImage CreateMaskedImage(string maskImageBundleName, UIColor backgroundColor)
        {
            return CoreUtility.ExecuteFunction("CreateMaskedImage", delegate() 
            {
                UIImage image = UIImage.FromBundle(maskImageBundleName);
                UIGraphics.BeginImageContextWithOptions(image.Size, false, image.CurrentScale);
                CGRect target = new CGRect(new CGPoint(0,0), image.Size);
                var context = UIGraphics.GetCurrentContext();

                //flips drawing context 
                context.TranslateCTM(0f, image.Size.Height);
                context.ScaleCTM(1f, -1f);//flip context

                image.Draw(target);
                context.ClipToMask(target, image.CGImage);
                context.SetBlendMode(CGBlendMode.Normal);
                backgroundColor.SetFill();
                context.FillRect(target);

                image = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                return image;
            });
        }
    }
}

