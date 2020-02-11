using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtSearchBar : SearchBar
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
					TextColor = textColorStyle.Color;
			}
		}

		ColorStyle placeholderColorStyle;
		public ColorStyle PlaceHolderColorStyle
		{
			get
			{
				return placeholderColorStyle;
			}

			set
			{
				placeholderColorStyle = value;
				if (placeholderColorStyle != null)
					PlaceholderColor = placeholderColorStyle.Color;
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
					FontFamily = FontExtension.GetFontFamily(FontFamily, fontStyle.FontWeight);
				if (this.ThemeUseFontSize() && fontStyle != null)
					FontSize = fontStyle.FontSize;
			}
		}

		public void ThemeChanged()
		{
			TextColorStyle = textColorStyle;
			PlaceHolderColorStyle = placeholderColorStyle;

			ThemeChangedAction?.Invoke();
		}

		public ExtSearchBar()
		{
			//BackgroundColor = Color.Transparent;
		}
	}
}
