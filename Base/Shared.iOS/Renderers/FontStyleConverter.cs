using System;
using Heleus.Base;
using UIKit;

namespace Heleus.Apps.Shared.Apple.Renderers
{
	public static class FontStyleConverter
	{
		public static UIFont GetFontByStyle(string family, float size)
		{
			if (string.IsNullOrEmpty(family))
				return null;

			var parts = family.Split(';');
			if (parts.Length == 2 && string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
			{
				var style = EnumHelper.StringToEnum(parts[1], FontWeight.None);
				if (style != FontWeight.None)
				{
					var nativeStyle = UIFontWeight.Regular;

					if (style == FontWeight.Thin)
						nativeStyle = UIFontWeight.Thin;
					else if (style == FontWeight.Light)
						nativeStyle = UIFontWeight.Light;
					else if (style == FontWeight.Regular)
						nativeStyle = UIFontWeight.Regular;
					else if (style == FontWeight.Medium)
						nativeStyle = UIFontWeight.Medium;
					else if (style == FontWeight.Bold)
						nativeStyle = UIFontWeight.Bold;


					return UIFont.SystemFontOfSize(size, nativeStyle);
				}
			}

			return null;
		}
	}
}
