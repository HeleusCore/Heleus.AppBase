using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtEntry : Entry, IThemeable
	{
		public Action ThemeChangedAction = null;

        public FontWeight FontWeight
		{
			get { return (FontWeight)GetValue(ExtLabel.FontWeightProperty); }
			private set { SetValue(ExtLabel.FontWeightProperty, value); }
		}

		ColorStyle textColorStyle;
		public ColorStyle TextColorStyle
		{
			get
			{
				return textColorStyle;
			}

			set
			{
				textColorStyle = value;
				if (textColorStyle != null)
				{
					TextColor = textColorStyle.Color;
					if (UIApp.IsGTK)
						PlaceholderColor = Color.Gray;
					else
                        PlaceholderColor = textColorStyle.Color.MultiplyAlpha(0.5);
				}
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
				if (this.ThemeUseFontSize() && fontStyle != null)
					FontSize = fontStyle.FontSize;
			}
		}

		public void ThemeChanged ()
		{
			TextColorStyle = textColorStyle;
			ThemeChangedAction?.Invoke ();
		}

		public ExtEntry ()
		{
			BackgroundColor = Color.Transparent;
		}
	}

	public class ExtEditor : Editor, IThemeable
	{
		public Action ThemeChangedAction = null;

        ColorStyle textColorStyle;
        public ColorStyle TextColorStyle
        {
            get
            {
                return textColorStyle;
            }

            set
            {
                textColorStyle = value;
                if (textColorStyle != null)
                {
                    TextColor = textColorStyle.Color;
                    if (UIApp.IsGTK)
                        PlaceholderColor = Color.Gray;
                    else
                        PlaceholderColor = textColorStyle.Color.MultiplyAlpha(0.5);
                }
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
                if (this.ThemeUseFontSize() && fontStyle != null)
                    FontSize = fontStyle.FontSize;
            }
		}

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(ExtLabel.FontWeightProperty); }
            private set { SetValue(ExtLabel.FontWeightProperty, value); }
        }

        public void ThemeChanged ()
		{
			TextColorStyle = textColorStyle;
			ThemeChangedAction?.Invoke ();
		}

		public ExtEditor ()
		{
			BackgroundColor = Color.Transparent;
		}
	}
}
