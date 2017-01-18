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
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Text;
using Android.Graphics;
using Android.Util;
using Stencil.Native.Droid.Core;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    /// <summary>
    /// A Drawable object that draws text.
    /// A TextDrawable accepts most of the same parameters that can be applied to TextView for displaying and formatting text.
    /// Optionally, a Path may be supplied on which to draw the text.
    /// A TextDrawable has an intrinsic size equal to that required to draw all the text it has been supplied, when possible. 
    /// In cases where a Path has been supplied, the caller must explicitly call setBounds() to provide the Drawable size based on the Path constraints.
    /// </summary>
    public class TextDrawable : Drawable
    {
        public TextDrawable(Context context)
            : base()
        {
            this.Initialize(context);
        }

        private const int SANS = 1;
        private const int SERIF = 2;
        private const int MONOSPACE = 3;

        private Resources _resources;
        private TextPaint _textPaint;
        private StaticLayout _textLayout;
        private Layout.Alignment _textAlignment = Layout.Alignment.AlignNormal;
        private Path _textPath;
        private ColorStateList _textColors;
        private Rect _textBounds;
        private string _text = "";
        private Context _context;

        private static int[] _themeAttributes = new int[] { Resource.Style.TextAppearance_AppCompat };
        private static int[] _appearanceAttributes = new int[] {
            Resource.Styleable.TextAppearance_android_textSize,
            Resource.Styleable.TextAppearance_android_typeface,
            Resource.Styleable.TextAppearance_android_textStyle,
            Resource.Styleable.TextAppearance_android_textColor
        };

        protected void Initialize(Context context)
        {
            _context = context;
            //Used to load and scale resource items
            _resources = context.Resources;
            //Definition of this drawables size
            _textBounds = new Rect();
            //Paint to use for the text
            _textPaint = new TextPaint(PaintFlags.AntiAlias);
            _textPaint.Density = _resources.DisplayMetrics.Density;
            _textPaint.Dither = true;

            int textSize = 15;
            ColorStateList textColor = null;
            TypefaceStyle styleIndex = TypefaceStyle.Normal;
            int typefaceIndex = -1;

            //Set default parameters from the current theme
            TypedArray a = context.Theme.ObtainStyledAttributes(_themeAttributes);
            int appearanceId = a.GetResourceId(0, -1);
            a.Recycle();

            TypedArray ap = null;
            if (appearanceId != -1)
            {
                ap = context.ObtainStyledAttributes(appearanceId, _appearanceAttributes);
            }
            if (ap != null)
            {
                for (int i = 0; i < ap.IndexCount; i++)
                {
                    int attr = ap.GetIndex(i);
                    switch (attr)
                    {
                        case 0: //Text Size
                            textSize = a.GetDimensionPixelSize(attr, textSize);
                            break;
                        case 1: //Typeface
                            typefaceIndex = a.GetInt(attr, typefaceIndex);
                            break;
                        case 2: //Text Style
                            styleIndex = (TypefaceStyle)a.GetInt(attr, (int)styleIndex);
                            break;
                        case 3: //Text Color
                            textColor = a.GetColorStateList(attr);
                            break;
                        default:
                            break;
                    }
                }

                ap.Recycle();
            }

            this.SetTextColor(textColor != null ? textColor : ColorStateList.ValueOf(Color.Black));
            this.SetRawTextSize(textSize);

            Typeface tf = null;
            switch (typefaceIndex)
            {
                case SANS:
                    tf = Typeface.SansSerif;
                    break;

                case SERIF:
                    tf = Typeface.Serif;
                    break;

                case MONOSPACE:
                    tf = Typeface.Monospace;
                    break;
            }

            this.SetTypeface(tf, styleIndex);
        }

        public void SetCustomFont(string assetPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(assetPath))
                {
                    Typeface face = FontLoader.GetFont(_context.Assets, assetPath);
                    if (face != null)
                    {
                        TypefaceStyle style = TypefaceStyle.Normal;
                        if (this.Typeface != null) //Takes care of android:textStyle=""
                        {
                            style = Typeface.Style;
                        }
                        this.SetTypeface(face, style);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Container.Track.LogWarning(ex.Message, "TextDrawable");
            }
        }
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                _text = value;
                this.MeasureContent();
            }
        }

        public float TextSize
        {
            get
            {
                return _textPaint.TextSize;
            }
            set
            {
                this.SetTextSize(ComplexUnitType.Sp, value);
            }
        }
        public void SetTextSize(ComplexUnitType unit, float size)
        {
            float dimension = TypedValue.ApplyDimension(unit, size, _resources.DisplayMetrics);
            this.SetRawTextSize(dimension);
        }
        protected void SetRawTextSize(float size)
        {
            if (size != _textPaint.TextSize)
            {
                _textPaint.TextSize = size;
                this.MeasureContent();
            }
        }

        public float TextScaleX
        {
            get
            {
                return _textPaint.TextScaleX;
            }
            set
            {
                if (value != _textPaint.TextScaleX)
                {
                    _textPaint.TextScaleX = value;
                    this.MeasureContent();
                }
            }
        }

        public Layout.Alignment TextAlign
        {
            get
            {
                return _textAlignment;
            }
            set
            {
                if (_textAlignment != value)
                {
                    _textAlignment = value;
                    this.MeasureContent();
                }
            }
        }

        public Typeface Typeface
        {
            get
            {
                return _textPaint.Typeface;
            }
            set
            {
                if (_textPaint.Typeface != value)
                {
                    _textPaint.SetTypeface(value);
                    this.MeasureContent();
                }
            }
        }
       
        public void SetTypeface(Typeface tf, TypefaceStyle style)
        {
            if (style > 0)
            {
                if (tf == null)
                {
                    tf = Typeface.DefaultFromStyle(style);
                }
                else
                {
                    tf = Typeface.Create(tf, style);
                }

                this.Typeface = tf;
                // now compute what (if any) algorithmic styling is needed
                TypefaceStyle typefaceStyle = tf != null ? tf.Style : TypefaceStyle.Normal;
                TypefaceStyle need = style & ~typefaceStyle;

                _textPaint.FakeBoldText = ((need & TypefaceStyle.Bold) != 0);
                _textPaint.TextSkewX = ((need & TypefaceStyle.Italic) != 0 ? -0.25f : 0);
            }
            else
            {
                _textPaint.FakeBoldText = false;
                _textPaint.TextSkewX = 0;
                this.Typeface = tf;
            }
        }



        public Color TextColor
        {
            set
            {
                this.SetTextColor(ColorStateList.ValueOf(value));
            }
        }
        public void SetTextColor(ColorStateList colorStateList)
        {
            _textColors = colorStateList;
            this.UpdateTextColors(this.GetState());
        }
        private bool UpdateTextColors(int[] stateSet)
        {
            int newColor = _textColors.GetColorForState(stateSet, Color.White);
            if (_textPaint.Color != newColor)
            {
                _textPaint.Color = new Color(newColor);
                return true;
            }
            return false;
        }
        
        public Path TextPath
        {
            get
            {
                return _textPath;
            }
            set
            {
                if (_textPath != value)
                {
                    _textPath = value;
                    this.MeasureContent();
                }
            }
        }


        private void MeasureContent()
        {
            //If drawing to a path, we cannot measure intrinsic bounds
            //We must rely on setBounds being called externally
            if (_textPath != null)
            {
                //Clear any previous measurement
                _textLayout = null;
                _textBounds.SetEmpty();
            }
            else
            {
                //Measure text bounds
                double desired = Math.Ceiling(Layout.GetDesiredWidth(_text, _textPaint));
                _textLayout = new StaticLayout(_text, _textPaint, (int)desired, _textAlignment, 1.0f, 0.0f, false);
                _textBounds.Set(0, 0, _textLayout.Width, _textLayout.Height);
            }

            //We may need to be redrawn
            this.InvalidateSelf();
        }

        protected override void OnBoundsChange(Rect bounds)
        {
            _textBounds.Set(bounds);
        }
        public override bool IsStateful
        {
            get
            {
                return _textColors.IsStateful;
            }
        }
        protected override bool OnStateChange(int[] state)
        {
            return this.UpdateTextColors(state);
        }
        public override int IntrinsicHeight
        {
            get
            {
                if (_textBounds.IsEmpty)
                {
                    return -1;
                }
                else
                {
                    return (_textBounds.Bottom - _textBounds.Top);
                }
            }
        }
        public override int IntrinsicWidth
        {
            get
            {
                if (_textBounds.IsEmpty)
                {
                    return -1;
                }
                else
                {
                    return (_textBounds.Right - _textBounds.Left);
                }
            }
        }
        public override void Draw(Canvas canvas)
        {
            Rect bounds = this.Bounds;
            int count = canvas.Save();
            canvas.Translate(bounds.Left, bounds.Top);
            if (_textPath == null)
            {
                //Allow the layout to draw the text
                _textLayout.Draw(canvas);
            }
            else
            {
                //Draw directly on the canvas using the supplied path
                canvas.DrawTextOnPath(_text, _textPath, 0, 0, _textPaint);
            }
            canvas.RestoreToCount(count);
        }
        public override void SetAlpha(int alpha)
        {
            this.Alpha = alpha;
        }

        public override int Alpha
        {
            get
            {
                return _textPaint.Alpha;
            }

            set
            {
                if (_textPaint.Alpha != value)
                {
                    _textPaint.Alpha = value;
                }
            }
        }
        public override int Opacity
        {
            get
            {
                return _textPaint.Alpha;
            }
        }
        
        public override void SetColorFilter(ColorFilter colorFilter)
        {
            if (_textPaint.ColorFilter != colorFilter)
            {
                _textPaint.SetColorFilter(colorFilter);
            }
        }
    }
}