using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace Stencil.Plugins.Amazon.Integration
{
    /// <summary>
    /// Extracted from Codeable Library
    /// </summary>
    public static class ImageFormatter
    {
        public static Bitmap Resize(Image image, int width, int height)
        {
            return Resize(image, ResizeMode.Fixed, AnchorStyles.Center | AnchorStyles.Middle, width, height, PixelFormat.Format24bppRgb, InterpolationMode.HighQualityBicubic, Color.Black);
        }

        public static Bitmap Resize(Image image, ResizeMode resize, int value)
        {
            return Resize(image, resize, AnchorStyles.Center | AnchorStyles.Middle, value);
        }
        public static Bitmap Resize(Image image, ResizeMode resize, int width, int height)
        {
            return Resize(image, resize, AnchorStyles.Center | AnchorStyles.Middle, width, height);
        }
        public static Bitmap Resize(Image image, ResizeMode resize, int width, int height, Color backColor)
        {
            return Resize(image, resize, AnchorStyles.Center | AnchorStyles.Middle, width, height, backColor);
        }
        public static Bitmap Resize(Image image, ResizeMode resize, AnchorStyles anchorStyle, int value)
        {
            int h = 0;
            int w = 0;
            if (resize == ResizeMode.HeightConstraint)
            {
                h = value;
            }
            if (resize == ResizeMode.WidthContraint)
            {
                w = value;
            }
            if (resize == ResizeMode.Percentage)
            {
                w = value;
            }
            return Resize(image, resize, anchorStyle, w, h);
        }
        public static Bitmap Resize(Image image, ResizeMode resize, AnchorStyles anchorStyle, int width, int height)
        {
            return Resize(image, resize, anchorStyle, width, height, PixelFormat.Format24bppRgb, InterpolationMode.HighQualityBicubic, Color.Black);
        }
        public static Bitmap Resize(Image image, ResizeMode resize, AnchorStyles anchorStyle, int width, int height, Color backColor)
        {
            return Resize(image, resize, anchorStyle, width, height, PixelFormat.Format24bppRgb, InterpolationMode.HighQualityBicubic, backColor);
        }
        public static Bitmap Resize(Image image, ResizeMode resize, AnchorStyles anchorStyle, int width, int height, PixelFormat bitFormat, InterpolationMode algorithm, Color backColor)
        {
            if (((image == null) || (image.Size.Height == 0)) || (image.Size.Width == 0))
            {
                throw new ArgumentOutOfRangeException("image", "Image cannot be null");
            }
            switch (resize)
            {
                case ResizeMode.Percentage:
                case ResizeMode.WidthContraint:
                case ResizeMode.Fixed:
                case ResizeMode.Fit:
                case ResizeMode.FitNoPadding:
                case ResizeMode.Crop:
                    if (width <= 0)
                    {
                        throw new ArgumentOutOfRangeException("Width must be greater than 0");
                    }
                    break;
                case ResizeMode.HeightConstraint:
                default:
                    // width not used
                    break;
            }
            switch (resize)
            {
                case ResizeMode.HeightConstraint:
                case ResizeMode.Fixed:
                case ResizeMode.Fit:
                case ResizeMode.Crop:
                case ResizeMode.FitNoPadding:
                    if (height <= 0)
                    {
                        throw new ArgumentOutOfRangeException("Height must be greater than 0");
                    }
                    break;
                case ResizeMode.Percentage:
                case ResizeMode.WidthContraint:
                default:
                    // Height not used
                    break;
            }

            Size imageSize = image.Size;
            Rectangle destinationRect = CalculateDestinationRectangle(imageSize, resize, anchorStyle, width, height);
            Rectangle srcRect = CalculateSourceRectangle(imageSize, resize, anchorStyle, width, height);
            Bitmap bitmap = CreateBitmap(resize, width, height, bitFormat, destinationRect);
            bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var attribs = new ImageAttributes())
            {
                attribs.SetWrapMode(WrapMode.TileFlipXY);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(backColor);
                    graphics.InterpolationMode = algorithm;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.DrawImage(image, destinationRect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, attribs);
                }
            }
            return bitmap;
        }


        private static Rectangle CalculateDestinationRectangle(Size imageSize, ResizeMode mode, AnchorStyles anchor, int width, int height)
        {
            float perc = 0f;

            switch (mode)
            {
                case ResizeMode.Percentage:
                    perc = ConvertToPercentage(width);
                    return new Rectangle(0, 0, (int)(imageSize.Width * perc), (int)(imageSize.Height * perc));

                case ResizeMode.WidthContraint:
                    perc = ((float)width) / ((float)imageSize.Width);
                    return new Rectangle(0, 0, width, (int)(imageSize.Height * perc));

                case ResizeMode.HeightConstraint:
                    perc = ((float)height) / ((float)imageSize.Height);
                    return new Rectangle(0, 0, (int)(imageSize.Width * perc), height);
                case ResizeMode.Fixed:
                    return new Rectangle(0, 0, width, height);
                case ResizeMode.Fill:
                    return new Rectangle(0, 0, width, height);
                case ResizeMode.Fit:
                    Size fitSize = CalculateFitSize(new Size(width, height), anchor, imageSize.Width, imageSize.Height);
                    return CalculateCropRectangle(new Size(width, height), anchor, fitSize.Width, fitSize.Height);
                case ResizeMode.FitNoPadding:
                    Size scaledSize = CalculateFitSize(new Size(width, height), anchor, imageSize.Width, imageSize.Height);
                    return new Rectangle(0, 0, scaledSize.Width, scaledSize.Height);
                case ResizeMode.Crop:
                    if ((imageSize.Width >= width) && (imageSize.Height >= height))
                    {
                        return new Rectangle(0, 0, width, height);
                    }
                    return CalculateCropRectangle(imageSize, anchor, width, height);
            }
            return new Rectangle();
        }
        private static Rectangle CalculateSourceRectangle(Size imageSize, ResizeMode mode, AnchorStyles anchor, int width, int height)
        {
            switch (mode)
            {
                case ResizeMode.Percentage:
                    return new Rectangle(0, 0, imageSize.Width, imageSize.Height);

                case ResizeMode.WidthContraint:
                    return new Rectangle(0, 0, imageSize.Width, imageSize.Height);

                case ResizeMode.HeightConstraint:
                    return new Rectangle(0, 0, imageSize.Width, imageSize.Height);

                case ResizeMode.Fixed:
                    return new Rectangle(0, 0, imageSize.Width, imageSize.Height);

                case ResizeMode.Fit:
                    return new Rectangle(0, 0, imageSize.Width, imageSize.Height);

                case ResizeMode.FitNoPadding:
                    return new Rectangle(0, 0, imageSize.Width, imageSize.Height);

                case ResizeMode.Fill:
                    return CalculateFillRectangle(imageSize, anchor, width, height);

                case ResizeMode.Crop:
                    if ((imageSize.Width >= width) && (imageSize.Height >= height))
                    {
                        return CalculateCropRectangle(imageSize, anchor, width, height);
                    }
                    return new Rectangle(0, 0, imageSize.Width, imageSize.Height);
            }
            return new Rectangle();
        }
        private static Rectangle CalculateCropRectangle(Size imageSize, AnchorStyles anchor, int width, int height)
        {
            int x = 0;
            int y = 0;
            int w = width;
            int h = height;
            float xRatio = 0f;
            float yRatio = 0f;
            xRatio = ((float)width) / ((float)imageSize.Width);
            yRatio = ((float)height) / ((float)imageSize.Height);
            if (width > imageSize.Width)
            {
                w = imageSize.Width;
            }
            if (height > imageSize.Height)
            {
                h = imageSize.Height;
            }
            if ((anchor & AnchorStyles.Top) == AnchorStyles.Top)
            {
                y = 0;
            }
            else if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
            {
                y = Math.Abs((int)(imageSize.Height - ((int)(yRatio * imageSize.Height))));
            }
            else
            {
                y = Math.Abs((int)((imageSize.Height - ((int)(yRatio * imageSize.Height))) / 2));
            }
            if ((anchor & AnchorStyles.Left) == AnchorStyles.Left)
            {
                x = 0;
            }
            else if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
            {
                x = Math.Abs((int)(imageSize.Width - ((int)(xRatio * imageSize.Width))));
            }
            else
            {
                x = Math.Abs((int)((imageSize.Width - ((int)(xRatio * imageSize.Width))) / 2));
            }
            return new Rectangle(x, y, w, h);
        }
        //private static Size CalculateFitSize(Size sourceImageSize, AnchorStyles anchor, int width, int height)
        //{
        //    int w = 0;
        //    int h = 0;
        //    float ratio = 0f;

        //    if (sourceImageSize.Width >= sourceImageSize.Height)
        //    {
        //        ratio = ((float)width) / ((float)sourceImageSize.Width);
        //        w = width;
        //        h = (int)(sourceImageSize.Height * ratio);
        //    }
        //    else
        //    {
        //        ratio = ((float)height) / ((float)sourceImageSize.Height);
        //        h = height;
        //        w = (int)(sourceImageSize.Width * ratio);
        //    }
        //    return new Size(w, h);
        //    //return CalculateCropRectangle(new Size(width, height), anchor, w, h);
        //}
        private static Size CalculateFitSize(Size sourceImageSize, AnchorStyles anchor, int width, int height)
        {
            int w = 0;
            int h = 0;
            float ratio = 0f;

            // make a switch or if-series testing the differences for negative
            // -H,-W  = its completely smaller, chose the smaller change %
            // +H,+W  = its completely larger, chose the smaller change %
            // -H,+W  = its partially smaller, chose H 
            // +H,-W  = its partially smaller, chose W 

            int widthDiff = (sourceImageSize.Width * 100) - width; // support up to 100 times bigger
            int heightDiff = (sourceImageSize.Height * 100) - height;  // support up to 100 times bigger
            float diffRatio = (float)widthDiff / (float)heightDiff;
            float targetRatio = ((float)width) / ((float)height);
            if (diffRatio < targetRatio)
            {
                // set height, width will be filled with color space
                ratio = ((float)height) / ((float)width);
                w = sourceImageSize.Width;
                h = (int)(w * ratio);
            }
            else
            {
                // set width, height width will be filled with color space
                ratio = ((float)width) / ((float)height);
                h = sourceImageSize.Height;
                w = (int)(h * ratio);
            }
            return new Size(w, h);
            //return CalculateCropRectangle(new Size(width, height), anchor, w, h);
        }
        private static Rectangle CalculateFillRectangle(Size sourceImageSize, AnchorStyles anchor, int width, int height)
        {
            int w = 0;
            int h = 0;
            float ratio = 0f;

            // make a switch or if-series testing the differences for negative
            // -H,-W  = its completely smaller, chose the smaller change %
            // +H,+W  = its completely larger, chose the smaller change %
            // -H,+W  = its partially smaller, chose H 
            // +H,-W  = its partially smaller, chose W 

            int widthDiff = (sourceImageSize.Width * 100) - width; // support up to 100 times bigger
            int heightDiff = (sourceImageSize.Height * 100) - height;  // support up to 100 times bigger
            float diffRatio = (float)widthDiff / (float)heightDiff;
            float targetRatio = ((float)width) / ((float)height);
            if (diffRatio < targetRatio)
            {
                // set width as full bleed
                ratio = ((float)height) / ((float)width);
                w = sourceImageSize.Width;
                h = (int)(w * ratio);
            }
            else
            {
                // set height as full bleed
                ratio = ((float)width) / ((float)height);
                h = sourceImageSize.Height;
                w = (int)(h * ratio);
            }

            return CalculateCropRectangle(sourceImageSize, anchor, w, h);
        }

        private static float ConvertToPercentage(int percentage)
        {
            if (percentage < 0)
            {
                throw new ArgumentOutOfRangeException("percentage");
            }
            return (((float)percentage) / 100f);
        }
        private static Bitmap CreateBitmap(ResizeMode resize, int width, int height, PixelFormat bitFormat, Rectangle destinationRect)
        {
            if (resize == ResizeMode.Crop || resize == ResizeMode.Fit || resize == ResizeMode.Fill)
            {
                return new Bitmap(width, height, bitFormat);
            }
            else
            {
                return new Bitmap(destinationRect.Width, destinationRect.Height, bitFormat);
            }

        }
    }
    [Flags]
    public enum AnchorStyles
    {
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8,
        // Centered vertically (y axis)
        Middle = 16,
        // Centered horizontally (x axis)
        Center = 32,
    }
    public enum ResizeMode
    {
        /// <summary>
        /// Resizes to specified percentage
        /// </summary>
        Percentage,
        /// <summary>
        /// Resizes to match width, preserves aspect ratio
        /// </summary>
        WidthContraint,
        /// <summary>
        /// Resizes to match height, preserves aspect ratio
        /// </summary>
        HeightConstraint,
        /// <summary>
        /// Ignore Aspect Ratio
        /// </summary>
        Fixed,
        /// <summary>
        /// Extracts a portion of the source image
        /// </summary>
        Crop,
        /// <summary>
        /// Resizes the image preserving aspect ratio and entire image contents.  Filling empty space with specified back color. [transparent will save as png instead of jpg]
        /// </summary>
        Fit,
        /// <summary>
        /// Resizes the image preserving aspect ratio and entire image contents. Fitting the image entirely inside the target rectangle, but not adding padding or cropping. Resulting dimensions will not match rectangle.
        /// </summary>
        FitNoPadding,
        /// <summary>
        /// Resizes the image preserving aspect ration and crops image to fill all empty space.
        /// </summary>
        Fill

    }
}
