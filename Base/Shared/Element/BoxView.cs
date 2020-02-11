using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtBoxView : BoxView, IThemeable
	{
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
						BackgroundColor = colorStyle.Color.MultiplyAlpha(alpha);
					else
						BackgroundColor = colorStyle.Color;
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

		public virtual void ThemeChanged ()
		{
			ColorStyle = colorStyle;
		}
	}

	public class Separator : ExtBoxView
	{
		public Separator(double height = 1)
		{
			HeightRequest = height;
			ColorStyle = Theme.RowColor;
		}

		public Separator(double height, Color color)
		{
			HeightRequest = height;
			Color = color;
		}
	}
}
