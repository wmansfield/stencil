using System;
using Android.Graphics;
using Android.Util;
using Android.OS;

namespace Stencil.Native.Droid.Core
{
    public static class AndroidUtility
    {
        public static bool IsEmulator()
        {
            bool result = false;

            string fingerprint = Build.Fingerprint;
            if (!string.IsNullOrEmpty(fingerprint)) 
            {
                result = fingerprint.Contains("vbox") || fingerprint.Contains("generic");
            }
            return result;
        }
        public static Bitmap RoundEdgedBitmap(Bitmap bitmap, RectF size, float radius, float borderWidth = 0, Color? borderColor = null)
        {
            Bitmap output = Bitmap.CreateBitmap((int)size.Width(), (int)size.Height(), Bitmap.Config.Argb4444);

            float vRation = size.Width() / size.Height();
            float bRation = (float)bitmap.Width / bitmap.Height;

            int srcWidth = 0;
            int srcHeight = 0;
            int srcX = 0;
            int srcY = 0;

            if (vRation > bRation) 
            {
                srcWidth = bitmap.Width;
                srcHeight = (int) (size.Height() * ((float) bitmap.Width / size.Width()));
                srcX = 0;
                srcY = (bitmap.Height - srcHeight) / 2;
            } 
            else 
            {
                srcWidth = (int) (size.Width() * ((float) bitmap.Height / size.Height()));
                srcHeight = bitmap.Height;
                srcX = (bitmap.Width - srcWidth) / 2;
                srcY = 0;
            }
            Rect srcRect = new Rect(srcX, srcY, srcX + srcWidth, srcY + srcHeight);
            Rect destRect = new Rect(0, 0, srcWidth, srcHeight);

            Canvas canvas = new Canvas(output);

            Paint paint = new Paint()
            {
                AntiAlias = true,
                Color = Color.DarkGray
            };

            canvas.DrawARGB(0, 0, 0, 0);

            //--CROP THE IMAGE
            canvas.DrawRoundRect(size, radius, radius,  paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(bitmap, srcRect, destRect, paint);

            //--ADD BORDER IF NEEDED
            if(borderWidth > 0)
            {
                paint = new Paint()
                {
                    AntiAlias = true,
                    Color = borderColor.GetValueOrDefault(),
                    StrokeWidth = borderWidth,
                };
                paint.SetStyle(Paint.Style.Stroke);
                canvas.DrawCircle(bitmap.Width / 2, bitmap.Height / 2, (float) (bitmap.Width / 2 - Math.Ceiling(borderWidth / 2)), paint);
            }
            return output;
        }
        public static Bitmap EllipsizeBitmap(Bitmap bitmap, float borderWidth = 0, Color? borderColor = null, Rect destinationSize = null)
        {
            Rect sourceRect = new Rect(0, 0, bitmap.Width, bitmap.Height);
            Rect destRect = new Rect(0, 0, bitmap.Width, bitmap.Height);
            if (destinationSize != null && destinationSize.Width() > 0 && destinationSize.Height() > 0)
            {
                int srcWidth = 0;
                int srcHeight = 0;
                int srcX = 0;
                int srcY = 0;

                float vRatio = destinationSize.Width() / destinationSize.Height();
                float bRatio = (float)bitmap.Width / bitmap.Height;
                if (vRatio > bRatio) 
                {
                    srcWidth = bitmap.Width;
                    srcHeight = (int) (destinationSize.Height() * ((float) bitmap.Width / destinationSize.Width()));
                    srcX = 0;
                    srcY = (bitmap.Height - srcHeight) / 2;
                } 
                else 
                {
                    srcWidth = (int) (destinationSize.Width() * ((float) bitmap.Height / destinationSize.Height()));
                    srcHeight = bitmap.Height;
                    srcX = (bitmap.Width - srcWidth) / 2;
                    srcY = 0;
                }

                sourceRect = new Rect(srcX, srcY, srcX + srcWidth, srcY + srcHeight);
                destRect = new Rect(0, 0, srcWidth, srcHeight);
            }

            Bitmap output = Bitmap.CreateBitmap(destRect.Width(), destRect.Height(), Bitmap.Config.Argb4444);

            Canvas canvas = new Canvas(output);

            Paint paint = new Paint()
            {
                AntiAlias = true,
                Color = Color.DarkGray
            };
            canvas.DrawARGB(0, 0, 0, 0);

            //--CROP THE IMAGE
            canvas.DrawCircle(output.Width / 2, output.Height / 2, (output.Width / 2) - 1, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(bitmap, sourceRect, destRect, paint);

            //--ADD BORDER IF NEEDED
            if(borderWidth > 0)
            {
                paint = new Paint()
                {
                    AntiAlias = true,
                    Color = borderColor.GetValueOrDefault(),
                    StrokeWidth = borderWidth,
                };
                paint.SetStyle(Paint.Style.Stroke);
                canvas.DrawCircle(output.Width / 2, output.Height / 2, (float) (output.Width / 2 - Math.Ceiling(borderWidth / 2)), paint);
            }
            return output;
        }
        public static Bitmap EllipsizeColor(int width, int height, Color color, float borderWidth = 0, Color? borderColor = null)
        {
            Bitmap output = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb4444);

            Canvas canvas = new Canvas(output);

            Paint paint = new Paint()
            {
                AntiAlias = true,
                Color = Color.DarkGray
            };

            canvas.DrawARGB(0, 0, 0, 0);

            //--CROP THE IMAGE
            canvas.DrawCircle(width / 2, height / 2, width / 2 - 1, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawColor(color, PorterDuff.Mode.SrcIn);

            //--ADD BORDER IF NEEDED
            if(borderWidth > 0)
            {
                paint = new Paint()
                {
                    AntiAlias = true,
                    Color = borderColor.GetValueOrDefault(),
                    StrokeWidth = borderWidth,
                };
                paint.SetStyle(Paint.Style.Stroke);
                canvas.DrawCircle(width / 2, height / 2, (float) (width / 2 - Math.Ceiling(borderWidth / 2)), paint);
            }
            return output;
        }
    }
}

