using System;
using Android.App;
using Android.Views;
using Android.Content;
using Android.OS;
using Newtonsoft.Json;
using System.Collections.Generic;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Util;
using Android.Support.V4.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Android.Media;
using Stencil.Native.Core;
using Org.Apache.Sanselan;

namespace Stencil.Native.Droid.Core
{
    public static class _ViewExtensions
    {
        #region Color Methods

        private static Dictionary<string, Color> _colorCache = new Dictionary<string,Color>();
        private static Dictionary<string, ColorDrawable> _drawableCache = new Dictionary<string,ColorDrawable>();
        public static ColorDrawable ConvertHexToDrawable(this string hexaColor)
        {
            if(_drawableCache.ContainsKey(hexaColor))
            {
                return _drawableCache[hexaColor];
            }
            ColorDrawable result = new ColorDrawable(hexaColor.ConvertHexToColor());
            _drawableCache[hexaColor] = result;
            return result;
        }
        public static Color ConvertHexToColor(this string hexaColor)
        {
            hexaColor = hexaColor.Replace("#", "");
            hexaColor = hexaColor.ToUpper();
            if(_colorCache.ContainsKey(hexaColor))
            {
                return _colorCache[hexaColor];
            }
            Color result = Color.ParseColor("#" + hexaColor);
            _colorCache[hexaColor] = result;

            return result;
        }

        #endregion

        #region Pixel Methods

        public static float ToDip(this float value, Context context)
        {
            return TypedValue.ApplyDimension(ComplexUnitType.Dip, value, context.Resources.DisplayMetrics);
        }
        public static int ToDip(this int value, Context context)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, value, context.Resources.DisplayMetrics);
        }
        public static int ToSp(this int value, Context context)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, value, context.Resources.DisplayMetrics);
        }
        public static float GetScreenWidth(this Context context)
        {
            DisplayMetrics displayMetrics = context.Resources.DisplayMetrics;
            return displayMetrics.WidthPixels / displayMetrics.Density;
        }
        public static float GetScreenHeight(this Context context)
        {
            DisplayMetrics displayMetrics = context.Resources.DisplayMetrics;
            return displayMetrics.HeightPixels / displayMetrics.Density;
        }
        #endregion

        #region Bitmap Methods

        public static Bitmap LoadBitmap(string sourcePath, int width, int height)
        {
            BitmapFactory.Options options = GenerateLoadOptions(sourcePath, width, height);
            return BitmapFactory.DecodeFile(sourcePath, options);
        }
        public static Bitmap LoadBitmapRotateIfNeeded(string sourcePath, int width, int height)
        {
            int loadWidth = width;
            int loadHeight = height;
            int rotation = Sanselan.GetRotationAngle(sourcePath);
            if(rotation == 90 || rotation == 270)
            {
                loadWidth = height;
                loadHeight = width;
            }

            BitmapFactory.Options options = GenerateLoadOptions(sourcePath, loadWidth, loadHeight);
            Bitmap bitmap = BitmapFactory.DecodeFile(sourcePath, options);

            if(rotation != 0)
            {
                Matrix matrix = new Matrix();
                matrix.PreRotate(rotation);
                Bitmap source = bitmap;
                bitmap = Bitmap.CreateBitmap(source, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
                source.Dispose();
                return bitmap;
            }
            else
            {
                return bitmap;
            }
        }

        public static System.Drawing.Size GenerateAspectFit(int originalWidth, int originalHeight, int targetWidth, int targetHeight)
        {
            if(targetWidth <= 0)
            {
                targetWidth = 1024;
            }
            if(targetHeight <= 0)
            {
                targetHeight = 1024;
            }
            int newWidth = -1;
            int newHeight = -1;
            float multFactor = -1.0F;
            if(originalHeight > originalWidth)
            {
                newHeight = targetHeight;
                multFactor = (float)originalWidth / (float)originalHeight;
                newWidth = (int)(newHeight * multFactor);
            }
            else if(originalWidth > originalHeight)
            {
                newWidth = targetWidth;
                multFactor = (float)originalHeight / (float)originalWidth;
                newHeight = (int)(newWidth * multFactor);
            }
            else // (originalHeight == originalWidth) 
            {
                newHeight = targetHeight;
                newWidth = targetWidth;
            }
            return new System.Drawing.Size(newWidth, newHeight);
        }
        public static Bitmap ResizeCenterCrop(this Bitmap bitmap, int width, int height, bool dispose)
        {
            // fill
            Matrix matrix = new Matrix();
            matrix.SetRectToRect(new RectF(0, 0, bitmap.Width, bitmap.Height), new RectF(0, 0, width, height), Matrix.ScaleToFit.Center);
            Bitmap result = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);

            if(dispose)
            {
                bitmap.Dispose();
            }
            return result;
        }
        public static Bitmap ResizeAspectFit(this Bitmap bitmap, int width, int height, bool dispose)
        {
            System.Drawing.Size size = GenerateAspectFit(bitmap.Width, bitmap.Height, width, height);

            if(bitmap.Width == size.Width && bitmap.Height == size.Height)
            {
                return bitmap;
            }
            Bitmap result = Bitmap.CreateScaledBitmap(bitmap, size.Width, size.Height, false);
            if(result == null)
            {
                throw new Exception("Unable to resize photo, Please try again.");
            }
            if(dispose)
            {
                bitmap.Dispose();
            }

            return result;
        }

        private static BitmapFactory.Options GenerateLoadOptions(string filePath, int requiredWidth, int requiredHeight)
        {

            BitmapFactory.Options options = new BitmapFactory.Options();
            //setting inJustDecodeBounds to true
            //ensures that we are able to measure
            //the dimensions of the image,without
            //actually allocating it memory
            options.InJustDecodeBounds = true;

            //decode the file for measurement
            BitmapFactory.DecodeFile(filePath, options);

            //obtain the inSampleSize for loading a 
            //scaled down version of the image.
            //options.outWidth and options.outHeight 
            //are the measured dimensions of the 
            //original image
            options.InSampleSize = CalculateBitmapScale(options.OutWidth, options.OutHeight, requiredWidth, requiredHeight);

            //set inJustDecodeBounds to false again
            //so that we can now actually allocate the
            //bitmap some memory
            options.InJustDecodeBounds = false;

            return options;
        }
        private static int CalculateBitmapScale(int originalWidth, int originalHeight, int requiredWidth, int requiredHeight)
        {
            int scale = 1;

            if((originalWidth > requiredWidth) || (originalHeight > requiredHeight))
            {
                //calculate scale with respect to the smaller dimension
                if(originalWidth < originalHeight)
                {
                    scale = (int)Math.Round((double)originalWidth / requiredWidth);
                }
                else
                {
                    scale = (int)Math.Round((double)originalHeight / requiredHeight);
                }
            }

            return scale;
        }

        #endregion

        #region Layout Methods

        public static void SetLinearLayoutMargin(this View view, Context context, int? top = null, int? right = null, int? bottom = null, int? left = null)
        {
            LinearLayout.LayoutParams parameters = new LinearLayout.LayoutParams((ViewGroup.MarginLayoutParams)view.LayoutParameters);
            if (top.HasValue)
            {
                parameters.TopMargin = top.Value.ToDip(context);
            }
            if (right.HasValue)
            {
                parameters.RightMargin = right.Value.ToDip(context);
            }
            if (bottom.HasValue)
            {
                parameters.BottomMargin = bottom.Value.ToDip(context);
            }
            if (left.HasValue)
            {
                parameters.LeftMargin = left.Value.ToDip(context);
            }
            view.LayoutParameters = parameters;
            view.RequestLayout();
        }
        public static void SetLinearLayoutSize(this View view, Context context, int? width = null, int? height = null)
        {
            int finalWidth = LinearLayout.LayoutParams.WrapContent;
            int finalHeight = LinearLayout.LayoutParams.WrapContent;
            if (width.HasValue)
            {
                if (width.Value < 0)
                {
                    finalWidth = width.Value;
                }
                else
                {
                    finalWidth = width.Value.ToDip(context);
                }
            }
            if (height.HasValue)
            {
                if (height.Value < 0)
                {
                    finalHeight = height.Value;
                }
                else
                {
                    finalHeight = height.Value.ToDip(context);
                }
            }

            LinearLayout.LayoutParams parameters = null;
            if (view.LayoutParameters != null)
            {
                parameters = new LinearLayout.LayoutParams((ViewGroup.MarginLayoutParams)view.LayoutParameters);
                if (width.HasValue)
                {
                    parameters.Width = finalWidth;
                }
                if (height.HasValue)
                {
                    parameters.Height = finalHeight;
                }
            }
            else
            {
                parameters = new LinearLayout.LayoutParams(finalWidth, finalHeight);
            }

            view.LayoutParameters = parameters;
            view.RequestLayout();
        }
        public static void SetRelativeLayoutSize(this View view, Context context, int? width = null, int? height = null)
        {
            int finalWidth = RelativeLayout.LayoutParams.WrapContent;
            int finalHeight = RelativeLayout.LayoutParams.WrapContent;
            if (width.HasValue)
            {
                if (width.Value < 0)
                {
                    finalWidth = width.Value;
                }
                else
                {
                    finalWidth = width.Value.ToDip(context);
                }
            }
            if (height.HasValue)
            {
                if (height.Value < 0)
                {
                    finalHeight = height.Value;
                }
                else
                {
                    finalHeight = height.Value.ToDip(context);
                }
            }

            RelativeLayout.LayoutParams parameters = null;
            if (view.LayoutParameters != null)
            {
                parameters = new RelativeLayout.LayoutParams((RelativeLayout.LayoutParams)view.LayoutParameters);
                if (width.HasValue)
                {
                    parameters.Width = finalWidth;
                }
                if (height.HasValue)
                {
                    parameters.Height = finalHeight;
                }
            }
            else
            {
                parameters = new RelativeLayout.LayoutParams(finalWidth, finalHeight);
            }

            view.LayoutParameters = parameters;
            view.RequestLayout();
        }
        public static void SetRelativeLayoutRule(this View view, Context context, LayoutRules rule, int anchor)
        {
            RelativeLayout.LayoutParams parameters = new RelativeLayout.LayoutParams((RelativeLayout.LayoutParams)view.LayoutParameters);
            parameters.AddRule(rule, anchor);
            view.LayoutParameters = parameters;
            view.RequestLayout();
        }
        
        #endregion

        #region Navigation Methods

        /// <summary>
        /// Fluent Interface to simplify Fragment creation: new MyFragment().WithRoute(myData);
        /// </summary>
        public static BaseFragment WithRoute<TRoute>(this BaseRoutedFragment<TRoute> fragment, TRoute item) 
            where TRoute : class
        {
            Bundle bundle = new Bundle(1);
            bundle.PutString(AndroidAssumptions.ROUTE_KEY, JsonConvert.SerializeObject(item));
            fragment.Arguments = bundle;
            fragment.RouteData = item;
            return fragment;
        }
        public static void StartActivity<TActivity>(this Activity activity, bool clearHistory = false, bool animate = true)
        {
            activity.StartActivity<TActivity, string>(string.Empty, clearHistory);
        }
        public static void StartActivity<TActivity, TRoute>(this Activity activity, TRoute route, bool clearHistory = false, bool animate = true, int enterAnimation = Resource.Animation.slide_in_from_right, int exitAnimation = Resource.Animation.slide_out_to_left)
        {
            Intent intent = new Intent(activity.ApplicationContext, typeof(TActivity));
            if ((object)route != null)
            {
                intent.PutExtra(AndroidAssumptions.ROUTE_KEY, JsonConvert.SerializeObject(route));
            }
            if (clearHistory)
            {
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.NewTask);
                intent.AddFlags(ActivityFlags.ClearTask);
            }
            activity.StartActivity(intent);

            if (animate)
            {
                activity.OverridePendingTransition(enterAnimation, exitAnimation);
            }
        }
        public static void StartActivity<TActivity>(this Activity activity, bool finishCurrent, Bundle bundle, bool animate = true, int enterAnimation = Resource.Animation.slide_in_from_right, int exitAnimation = Resource.Animation.slide_out_to_left) 
            where TActivity : Activity
        {
            Intent newActivity = new Intent(activity, typeof(TActivity));
            if (bundle != null)
            {
                newActivity.PutExtra(BaseActivity.INIT_BUNDLE_KEY, bundle);
            }
            activity.StartActivity(newActivity);
            if (finishCurrent)
            {
                activity.Finish();
            }
            if (animate)
            {
                activity.OverridePendingTransition(enterAnimation, exitAnimation);
            }
        }
        public static void StartActivityForResult<TActivity, TRoute>(this Activity activity, TRoute route, int requestCode, bool animate = true, int enterAnimation = Resource.Animation.slide_in_from_right, int exitAnimation = Resource.Animation.slide_out_to_left)
        {
            Intent intent = new Intent(activity.ApplicationContext, typeof(TActivity));
            if ((object)route != null)
            {
                intent.PutExtra(AndroidAssumptions.ROUTE_KEY, JsonConvert.SerializeObject(route));
            }

            activity.StartActivityForResult(intent, requestCode);

            if (animate)
            {
                activity.OverridePendingTransition(enterAnimation, exitAnimation);
            }
        }
        public static void StartActivityForResult<TActivity, TRoute>(this BaseFragment fragment, TRoute route, int requestCode, bool animate = true, int enterAnimation = Resource.Animation.slide_in_from_right, int exitAnimation = Resource.Animation.slide_out_to_left)
        {
            Intent intent = new Intent(fragment.Activity.ApplicationContext, typeof(TActivity));
            if ((object)route != null)
            {
                intent.PutExtra(AndroidAssumptions.ROUTE_KEY, JsonConvert.SerializeObject(route));
            }

            fragment.StartActivityForResult(intent, requestCode);

            if (animate)
            {
                fragment.Activity.OverridePendingTransition(enterAnimation, exitAnimation);
            }
        }
        public static void StartActivity<TActivity, TRoute>(this BaseFragment fragment, TRoute route, bool animate = true, int enterAnimation = Resource.Animation.slide_in_from_right, int exitAnimation = Resource.Animation.slide_out_to_left)
        {
            Intent intent = new Intent(fragment.Activity.ApplicationContext, typeof(TActivity));
            if ((object)route != null)
            {
                intent.PutExtra(AndroidAssumptions.ROUTE_KEY, JsonConvert.SerializeObject(route));
            }

            fragment.StartActivity(intent);

            if (animate)
            {
                fragment.Activity.OverridePendingTransition(enterAnimation, exitAnimation);
            }
        }
        public static void OverridePendingTransitionDefault(this Activity activity, int enterAnimation = Resource.Animation.slide_in_from_left, int exitAnimation = Resource.Animation.slide_out_to_right)
        {
            activity.OverridePendingTransition(enterAnimation, exitAnimation);
        }
        public static void FinishWithAnimation(this Activity activity, int enterAnimation = Resource.Animation.slide_in_from_left, int exitAnimation = Resource.Animation.slide_out_to_right)
        {
            activity.Finish();
            activity.OverridePendingTransition(enterAnimation, exitAnimation);
        }
        public static TRoute GetRoute<TRoute>(this Activity activity)
        {
            string json = activity.Intent.GetStringExtra(AndroidAssumptions.ROUTE_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<TRoute>(json);
            }
            return default(TRoute);
        }
        public static Bundle GetInitialBundle(this Activity activity)
        {
            return activity.Intent.GetBundleExtra(BaseActivity.INIT_BUNDLE_KEY);
        }


        public static string GetInitValues(this Context context)
        {
            return ((Activity)context).Intent.GetStringExtra(BaseActivity.INIT_BUNDLE_KEY) ?? string.Empty;
        }

        #endregion

        #region Hidden Helpers

        public static void SetGone(this View view, bool gone)
        {
            if (gone)
            {
                view.Visibility = ViewStates.Gone;
            }
            else
            {
                view.Visibility = ViewStates.Visible;
            }
        }
        public static void SetVisible(this View view, bool visible)
        {
            if (!visible)
            {
                view.Visibility = ViewStates.Invisible;
            }
            else
            {
                view.Visibility = ViewStates.Visible;
            }
        }
        public static void SetHidden(this View view, bool hidden)
        {
            if (hidden)
            {
                view.Visibility = ViewStates.Invisible;
            }
            else
            {
                view.Visibility = ViewStates.Visible;
            }
        }

        #endregion

        #region Activity Helpers

        public static T GetExtraJsonObject<T>(this Intent intent, string key)
        {
            if(intent != null && !string.IsNullOrEmpty(key))
            {
                string json = intent.GetStringExtra(key);
                if(!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            return default(T);
        }

        public static void ExecuteMethodOnMainThread(this Activity activity, string name, Action method)
        {
            activity.RunOnUiThread(delegate ()
            {
                CoreUtility.ExecuteMethod(name, method);
            });
        }

        #endregion

    }
}

