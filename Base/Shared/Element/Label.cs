using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class ExtLabel : Label, IThemeable
    {
		public static readonly BindableProperty FontWeightProperty = 
			BindableProperty.Create("FontWeight", typeof(FontWeight), typeof(object), FontWeight.Regular);

		public FontWeight FontWeight
		{
			get { return (FontWeight)GetValue(FontWeightProperty); }
			set { SetValue(FontWeightProperty, value); }
		}
		
        string _text;
        public new string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
                base.Text = value;
            }
        }

		ColorStyle colorStyle;
		double alpha = 1.0;

		public ColorStyle ColorStyle
		{
			get
			{
				return colorStyle;
			}

			set
			{
				colorStyle = value;
				if (colorStyle != null)
				{
					if (alpha < 1.0)
						TextColor = colorStyle.Color.MultiplyAlpha(alpha);
					else
						TextColor = colorStyle.Color;
				}
			}
		}

		public double ColorStyleAlpha
		{
			get
			{
				return alpha;
			}

			set
			{
				alpha = value;
				ColorStyle = colorStyle;
			}
		}

		FontStyle fontStyle;
		public FontStyle FontStyle
		{
			get
			{
				return fontStyle;
			}

			set
			{
				fontStyle = value;
				if (this.ThemeUseFontWeight() && fontStyle != null)
				{
					FontFamily = FontExtension.GetFontFamily(FontFamily, fontStyle.FontWeight);
					FontWeight = fontStyle.FontWeight;
				}
				if (this.ThemeUseFontSize () && fontStyle != null)
					FontSize = fontStyle.FontSize;
			}
		}

		public void SetStyle(FontStyle fontStyle, ColorStyle colorStyle)
		{
			FontStyle = fontStyle;
			ColorStyle = colorStyle;
		}

        public void ThemeChanged()
        {
			FontStyle = fontStyle;
			ColorStyle = colorStyle;

            if (FormattedText != null)
            {
                foreach (var span in FormattedText.Spans)
                {
                    if(span.GetColorStyle() is ThemedColorStyle || span.GetFontStyle() is ThemedFontStyle)
                        span.SetStyle(span.GetFontStyle(), span.GetColorStyle());
                }
            }
        }
    }
}
