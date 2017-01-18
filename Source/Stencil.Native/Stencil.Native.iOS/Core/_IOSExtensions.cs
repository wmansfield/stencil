using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using Stencil.Native.Core;
using Stencil.Native.iOS.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace Stencil.Native.iOS.Core
{
    public static class _IOSExtensions
    {
        #region Controllers

        public static UIViewController GetRootVisibleController(this UIApplication uiApplication)
        {
            if (uiApplication != null)
            {
                return uiApplication.KeyWindow.RootViewController.GetVisibleController();
            }
            return null;
        }
        public static UIViewController GetVisibleController(this UIViewController rootViewController)
        {
            if (rootViewController == null) { return null; }
            if (rootViewController is UITabBarController)
            {
                return ((UITabBarController)rootViewController).SelectedViewController.GetVisibleController();
            }
            else if (rootViewController is UINavigationController)
            {
                return ((UINavigationController)rootViewController).VisibleViewController.GetVisibleController();
            }
            else if (rootViewController.PresentedViewController != null)
            {
                return rootViewController.PresentedViewController.GetVisibleController();
            }
            else
            {
                return rootViewController;
            }
        }

        [Obsolete("Use the UIRefreshControlDelayed", true)]
        public static void BeginRefreshingWithBugFix(this UIRefreshControl refreshControl, UIScrollView tableViewToAdjust)
        {
            CoreUtility.ExecuteMethod("BeginRefreshingWithBugFix", delegate ()
            {
                if (refreshControl != null)
                {
                    refreshControl.BeginRefreshing();
                    if (tableViewToAdjust.ContentOffset.Y == 0)
                    {
                        UIView.Animate(.25, delegate ()
                        {
                            tableViewToAdjust.ContentOffset = new CGPoint(0, -refreshControl.Frame.Size.Height);
                        });
                    }
                }
            });
        }
        public static UIRefreshControlDelayed InjectRefreshControl(this UITableViewController tableViewController, EventHandler handler)
        {
            return CoreUtility.ExecuteFunction("InjectRefreshControl", delegate ()
            {
                UIRefreshControlDelayed refreshControl = new UIRefreshControlDelayed();
                refreshControl.ScrollView = tableViewController.TableView;
                refreshControl.AddTarget(handler, UIControlEvent.ValueChanged);
                tableViewController.RefreshControl = refreshControl;
                return refreshControl;
            });
        }
        public static UIRefreshControlDelayed InjectRefreshControl(this UIViewController viewController, UITableView tableView, EventHandler handler)
        {
            return CoreUtility.ExecuteFunction("InjectRefreshControl", delegate ()
            {
                UITableViewController tableViewController = new UITableViewController();
                viewController.AddChildViewController(tableViewController);
                tableViewController.TableView = tableView;

                UIRefreshControlDelayed refreshControl = new UIRefreshControlDelayed();
                refreshControl.ScrollView = tableView;
                refreshControl.AddTarget(handler, UIControlEvent.ValueChanged);

                tableViewController.RefreshControl = refreshControl;

                refreshControl.TintColor = UIColor.Black;
                refreshControl.TintColorDidChange();
                return refreshControl;
            });
        }
        public static UIRefreshControl InjectRefreshControl(this UIViewController viewController, UICollectionView collectionView, EventHandler handler)
        {
            return CoreUtility.ExecuteFunction("InjectRefreshControl", delegate ()
            {
                UIRefreshControlDelayed refreshControl = new UIRefreshControlDelayed();
                refreshControl.ScrollView = collectionView;
                refreshControl.AddTarget(handler, UIControlEvent.ValueChanged);
                collectionView.AddSubview(refreshControl);
                collectionView.AlwaysBounceVertical = true;
                return refreshControl;
            });
        }
        public static void RemoveRefreshControl(this UITableViewController tableViewController, UIRefreshControl control, EventHandler handler)
        {
            CoreUtility.ExecuteMethod("RemoveRefreshControl", delegate ()
            {
                control.RemoveTarget(handler, UIControlEvent.ValueChanged);
                if (tableViewController.RefreshControl == control)
                {
                    tableViewController.RefreshControl = null;
                }
            });
        }
        #endregion

        #region Hacky Device Sizing Methods

        private static readonly Lazy<CGRect> _mainScreenBounds = new Lazy<CGRect>(() => UIScreen.MainScreen.Bounds);
        public static CGRect MainScreenBounds { get { return _mainScreenBounds.Value; } }

        public static bool IsIPhone6Plus()
        {
            return (MainScreenBounds.Height == 736);
        }
        public static bool IsIPhone6()
        {
            return (MainScreenBounds.Height == 667);
        }
        public static bool IsIPhone5()
        {
            return (MainScreenBounds.Height == 568);
        }
        public static bool IsIPhone4()
        {
            return (MainScreenBounds.Height == 480);
        }


        /// <summary>
        /// Converts from storyboard basic width of 320 to current device metric.
        /// Assumes the width is constrained by left/right
        /// Essentially only good for text sizing routines
        /// </summary>
        public static float SizingScaleFix(this int width)
        {
            if (IsIPhone6Plus())
            {
                // (414 - 320) = 94
                return width + 94f;
            }
            else if (IsIPhone6())
            {
                // (375 - 320) = 55
                return width + 55f;
            }
            return width;
        }
        /// <summary>
        /// Converts from storyboard basic width of 320 to current device metric.
        /// Assumes the width is constrained by left/right
        /// Essentially only good for text sizing routines
        /// </summary>
        public static float SizingScaleFix(this nfloat width, int countOfItems = 1)
        {
            if (IsIPhone6Plus())
            {
                // (414 - 320) = 94
                return (float)(width + (94f / countOfItems));
            }
            else if (IsIPhone6())
            {
                // (375 - 320) = 55
                return (float)(width + (55f / countOfItems));
            }
            return (float)width;
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Pushes to current root nav controller, presents if one was not found.
        /// </summary>
        public static bool PushToRootNavigationController(this IViewPlatform platform, UIViewController controller, bool animated)
        {
            return CoreUtility.ExecuteFunction("PushToRootNavigationController", delegate ()
            {
                UINavigationController navController = platform.GetRootNavigationController();
                if (navController != null)
                {
                    navController.PushViewController(controller, animated);
                    return true;
                }
                else
                {
                    BaseUIViewController rootBaseController = platform.GetRootViewController() as BaseUIViewController;
                    if (rootBaseController != null)
                    {
                        rootBaseController.PresentViewControllerWithDisposeOnReturn(controller, animated, null);
                        return true;
                    }
                    UIViewController rootController = platform.GetRootViewController();
                    if (rootController != null)
                    {
                        rootController.PresentViewController(controller, animated, null);
                        return true;
                    }
                }
                return false;
            });
        }

        #endregion

        #region Layout


        public static void SetDefaultInsets(this UITableViewCell cell)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                cell.PreservesSuperviewLayoutMargins = false;
                cell.LayoutMargins = UIEdgeInsets.Zero;
            }
            cell.SeparatorInset = new UIEdgeInsets(0, 15, 0, 15);
        }
        public static void RemoveInsets(this UITableViewCell cell)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                cell.PreservesSuperviewLayoutMargins = false;
                cell.LayoutMargins = UIEdgeInsets.Zero;
            }
            else
            {
                cell.SeparatorInset = UIEdgeInsets.Zero;
            }
        }

        public static void SetNavigationBarTranslucent(this UIViewController viewController, bool translucent)
        {
            if (viewController != null && viewController.NavigationController != null)
            {
                SetNavigationBarTranslucent(viewController.NavigationController, translucent);
            }
        }
        public static void SetNavigationBarTranslucent(this UINavigationController navigationController, bool translucent)
        {
            if (navigationController != null && navigationController.NavigationBar != null)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    navigationController.NavigationBar.Translucent = translucent;
                }
            }
        }
        public static void HideNavigationBarHairLine(this UIViewController viewController)
        {
            if (viewController != null && viewController.NavigationController != null)
            {
                HideNavigationBarHairLine(viewController.NavigationController);
            }
        }
        public static void HideNavigationBarHairLine(this UINavigationController navigationController)
        {
            if (navigationController != null && navigationController.NavigationBar != null)
            {
                UIView hairline = FindHairLine(navigationController.NavigationBar);
                if (hairline != null)
                {
                    hairline.Hidden = true;
                }
            }
        }
        private static UIView FindHairLine(UIView view)
        {
            if (view is UIImageView && view.Bounds.Size.Height <= 1f)
            {
                return view;
            }
            foreach (var item in view.Subviews)
            {
                UIView found = FindHairLine(item);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        #endregion

        #region Fonts

        public static void DebugFontNames()
        {
            foreach (var item in UIFont.FamilyNames)
            {
                foreach (var font in UIFont.FontNamesForFamilyName(item))
                {
                    Console.WriteLine(font);
                }
            }

            /*
            NSArray *fontNames;
            NSInteger indFamily, indFont;
            for (indFamily=0; indFamily<[familyNames count]; ++indFamily)
            {
                NSLog(@"Family name: %@", [familyNames objectAtIndex:indFamily]);
                fontNames = [[NSArray alloc] initWithArray:
                    [UIFont fontNamesForFamilyName:
                        [familyNames objectAtIndex:indFamily]]];
                for (indFont=0; indFont<[fontNames count]; ++indFont)
                {
                    NSLog(@"    Font name: %@", [fontNames objectAtIndex:indFont]);
                }
                [fontNames release];
            }
            [familyNames release];
            */
        }

        public static void AddUrlsForText(this NSMutableAttributedString attributedString, UIFont font, UIColor linkColor, string allText, string linkText, string destinationUrl)
        {
            CoreUtility.ExecuteMethod("AddUrlsForText", delegate ()
            {
                int ix = allText.IndexOf(linkText, StringComparison.OrdinalIgnoreCase);
                while (ix >= 0)
                {
                    NSRange foundRange = new NSRange(ix, linkText.Length);

                    if (foundRange.Location >= 0)
                    {
                        attributedString.SetAttributes(new UIStringAttributes()
                        {
                            Font = font,
                            ForegroundColor = linkColor
                        }, foundRange);

                        attributedString.AddAttribute(new NSString("handle"), new NSString(destinationUrl), foundRange);
                    }
                    int nextIX = allText.Substring(ix + linkText.Length).IndexOf(linkText, StringComparison.OrdinalIgnoreCase);
                    if (nextIX >= 0)
                    {
                        ix = ix + linkText.Length + nextIX;
                    }
                    else
                    {
                        ix = -1;
                    }
                }

            });
        }

        #endregion

        #region Collections

        public static NSMutableDictionary ToNSDictionary(this Dictionary<string, string> dictionary)
        {
            NSMutableDictionary result = new NSMutableDictionary();
            if (dictionary != null)
            {
                foreach (var item in dictionary)
                {
                    result.Add(NSObject.FromObject(item.Key), NSObject.FromObject(item.Value));
                }
            }
            return result;
        }

        #endregion

        #region Coloring

        private static Dictionary<string, UIColor> _colorCache = new Dictionary<string, UIColor>(StringComparer.OrdinalIgnoreCase);

        public static UIColor ConvertHexToColor(this string hexColor)
        {
            if (_colorCache.ContainsKey(hexColor))
            {
                return _colorCache[hexColor];
            }
            if (!string.IsNullOrEmpty(hexColor))
            {
                string hexaColor = hexColor.Replace("#", "");
                if (hexaColor.Length == 3)
                {
                    hexaColor = hexaColor + hexaColor;
                }
                if (hexaColor.Length == 6)
                {
                    hexaColor = "FF" + hexaColor;
                }
                hexaColor = hexaColor.ToUpper();
                if (hexaColor.Length == 8)
                {
                    if (_colorCache.ContainsKey(hexaColor))
                    {
                        return _colorCache[hexaColor];
                    }
                    UIColor result = UIColor.FromRGBA(
                        Convert.ToByte(hexaColor.Substring(2, 2), 16),
                        Convert.ToByte(hexaColor.Substring(4, 2), 16),
                        Convert.ToByte(hexaColor.Substring(6, 2), 16),
                        Convert.ToByte(hexaColor.Substring(0, 2), 16)
                    );

                    _colorCache[hexaColor] = result;
                    _colorCache[hexColor] = result;
                    return result;
                }
            }
            return UIColor.White;
        }

        public static UIColor ConvertHexToColor(this string hexColor, double alphaOf1)
        {
            if (_colorCache.ContainsKey(hexColor + alphaOf1.ToString()))
            {
                return _colorCache[hexColor + alphaOf1.ToString()];
            }
            if (!string.IsNullOrEmpty(hexColor))
            {
                string hexaColor = hexColor.Replace("#", "");
                if (hexaColor.Length == 3)
                {
                    hexaColor = hexaColor + hexaColor;
                }
                if (hexaColor.Length == 6)
                {
                    hexaColor = "FF" + hexaColor;
                }
                hexaColor = hexaColor.ToUpper();
                if (hexaColor.Length == 8)
                {
                    UIColor result = UIColor.FromRGBA(
                        Convert.ToByte(hexaColor.Substring(2, 2), 16),
                        Convert.ToByte(hexaColor.Substring(4, 2), 16),
                        Convert.ToByte(hexaColor.Substring(6, 2), 16),
                        Convert.ToByte((int)(255 * alphaOf1))
                    );
                    _colorCache[hexColor + alphaOf1.ToString()] = result;
                    return result;
                }
            }
            return UIColor.White;
        }

        #endregion

        #region Exceptions

        public static Exception ConvertToException(this NSError error)
        {
            if (error != null)
            {
                string message = string.Empty;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Error Code:  {0}\r\n", error.Code.ToString());
                sb.AppendFormat("Description: {0}\r\n", error.LocalizedDescription);
                var userInfo = error.UserInfo;
                for (int i = 0; i < userInfo.Keys.Length; i++)
                {
                    sb.AppendFormat("[{0}]: {1}\r\n", userInfo.Keys[i].ToString(), userInfo.Values[i].ToString());
                }
                message = sb.ToString();
                return new Exception(message);
            }
            return null;
        }

        #endregion

        #region Drawing

        public static float ToRadians(this double val)
        {
            return (float)((Math.PI / 180) * val);
        }
        public static nfloat ToRadians(this nfloat degree)
        {
            return (degree * (nfloat)Math.PI) / 180.0f;
        }
        public static CGPoint GetMidpointTo(this CGPoint self, CGPoint target)
        {
            return new CGPoint
                (
                    (self.X + target.X) / 2f,
                    (self.Y + target.Y) / 2f
                );
        }
        public static UIImage ScaledTo(this UIImage image, CGSize size)
        {
            UIGraphics.BeginImageContextWithOptions(size, true, 0f);

            //draw
            image.Draw(new CGRect(0.0f, 0.0f, size.Width, size.Height));

            //capture resultant image
            UIImage result = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            //return image
            return result;
        }
        public static UIImage ScaledToFit(this UIImage image, CGSize size)
        {
            nfloat aspect = image.Size.Width / image.Size.Height;

            if (size.Width / aspect <= size.Height)
            {
                return image.ScaledTo(new CGSize(size.Width, size.Width / aspect));
            }
            else
            {
                return image.ScaledTo(new CGSize(size.Height * aspect, size.Height));
            }
        }
        public static UIImage ScaledToFill(this UIImage image, CGSize size)
        {
            nfloat aspect = image.Size.Width / image.Size.Height;

            if (size.Width / aspect > size.Height)
            {
                return image.ScaledTo(new CGSize(size.Width, size.Width / aspect));
            }
            else
            {
                return image.ScaledTo(new CGSize(size.Height * aspect, size.Height));
            }
        }
        public static UIImage ScaledToFillAndCenter(this UIImage image, CGSize imageSize)
        {
            UIImage sizedImage = image.ScaledToFill(imageSize);

            CGPoint offset = new CGPoint((imageSize.Width - sizedImage.Size.Width) / 2, (imageSize.Height - sizedImage.Size.Height) / 2);

            UIGraphics.BeginImageContextWithOptions(imageSize, true, 0f);

            sizedImage.Draw(offset);

            UIImage result = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();


            return result;
        }
        public static UIImage CroppedToSize(this UIImage image, CGSize imageSize, CGPoint offset, bool mirror)
        {
            UIGraphics.BeginImageContextWithOptions(imageSize, true, 0f);

            image.Draw(offset);

            UIImage result = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();


            if (mirror)
            {
                UIImageOrientation imageOrientation = UIImageOrientation.Up;
                switch (result.Orientation)
                {
                    case UIImageOrientation.Down:
                        imageOrientation = UIImageOrientation.DownMirrored;
                        break;

                    case UIImageOrientation.DownMirrored:
                        imageOrientation = UIImageOrientation.Down;
                        break;

                    case UIImageOrientation.Left:
                        imageOrientation = UIImageOrientation.LeftMirrored;
                        break;

                    case UIImageOrientation.LeftMirrored:
                        imageOrientation = UIImageOrientation.Left;

                        break;

                    case UIImageOrientation.Right:
                        imageOrientation = UIImageOrientation.RightMirrored;

                        break;

                    case UIImageOrientation.RightMirrored:
                        imageOrientation = UIImageOrientation.Right;

                        break;

                    case UIImageOrientation.Up:
                        imageOrientation = UIImageOrientation.UpMirrored;
                        break;

                    case UIImageOrientation.UpMirrored:
                        imageOrientation = UIImageOrientation.Up;
                        break;
                    default:
                        break;
                }
                result = new UIImage(result.CGImage, result.CurrentScale, imageOrientation);
            }
            return result;
        }

        #endregion

        #region Videos

        public static UIImage GetVideoThumbnail(this AVAsset asset)
        {
            return CoreUtility.ExecuteFunction("GetVideoThumbnail", delegate ()
            {
                AVAssetImageGenerator generator = new AVAssetImageGenerator(asset);
                generator.AppliesPreferredTrackTransform = true;
                CMTime ignoreTime = new CMTime();
                NSError error = null;
                CGImage image = generator.CopyCGImageAtTime(new CMTime(1, 1), out ignoreTime, out error);
                if (error != null)
                {
                    Container.Track.LogError(error.ConvertToException(), "GetVideoThumbnail");
                }
                return new UIImage(image);
            });

        }
        #endregion

        #region Date Methods

        private static DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime ToDateTime(this NSDate date)
        {
            var utcDateTime = reference.AddSeconds(date.SecondsSinceReferenceDate);
            var dateTime = utcDateTime.ToLocalTime();
            return dateTime;
        }
        public static NSDate ToNSDate(this DateTime datetime)
        {
            var utcDateTime = datetime.ToUniversalTime();
            var date = NSDate.FromTimeIntervalSinceReferenceDate((utcDateTime - reference).TotalSeconds);
            return date;
        }

        #endregion
    }

}