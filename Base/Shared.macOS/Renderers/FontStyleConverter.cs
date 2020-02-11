using System;
using AppKit;
using Heleus.Base;

namespace Heleus.Apps.Shared.Apple.Renderers
{
	public static class FontStyleConverter
	{
		public static NSFont GetFontByStyle(string family, float size)
		{
			if (string.IsNullOrEmpty(family))
				return null;

			var parts = family.Split(';');
			if (parts.Length == 2 && string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
			{
				var style = EnumHelper.StringToEnum(parts[1], FontWeight.None);
				if (style != FontWeight.None)
				{
                    var nativeStyle = NSFontWeight.Regular;

					if (style == FontWeight.Thin)
						nativeStyle = NSFontWeight.Thin;
					else if (style == FontWeight.Light)
						nativeStyle = NSFontWeight.Light;
					else if (style == FontWeight.Regular)
						nativeStyle = NSFontWeight.Regular;
					else if (style == FontWeight.Medium)
						nativeStyle = NSFontWeight.Medium;
					else if (style == FontWeight.Bold)
						nativeStyle = NSFontWeight.Bold;


                    return NSFont.SystemFontOfSize(size, nativeStyle);
				}
			}

			return null;
		}
	}
}
