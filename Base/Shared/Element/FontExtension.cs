using System;

namespace Heleus.Apps.Shared
{
	public enum FontWeight
	{
		None,
		Thin,
		Light,
		Regular,
		Medium,
		Bold
	}

    public static class FontExtension
    {
        public static string GetFontFamily(string family, FontWeight fontStyle)
        {
            if (family == null)
                family = string.Empty;
            
            switch (fontStyle)
            {
                case FontWeight.Thin:
                    if (UIApp.IsIOS || UIApp.IsMacOS)
                        family = ";Thin";
                    else if (UIApp.IsAndroid)
                        family = "sans-serif-thin";
					break;

                case FontWeight.Light:
					if (UIApp.IsIOS || UIApp.IsMacOS)
						family = ";Light";
					else if (UIApp.IsAndroid)
						family = "sans-serif-light";
					break;

                case FontWeight.Regular:
					if (UIApp.IsIOS || UIApp.IsMacOS)
						family = ";Regular";
					else if (UIApp.IsAndroid)
						family = "sans-serif";
					break;

                case FontWeight.Medium:
					if (UIApp.IsIOS || UIApp.IsMacOS)
						family = ";Medium";
					else if (UIApp.IsAndroid)
						family = "sans-serif-medium";
					break;

                case FontWeight.Bold:
					if (UIApp.IsIOS || UIApp.IsMacOS)
						family = ";Bold";
					else if (UIApp.IsAndroid)
						family = "sans-serif-bold";
					break;
            }

            return family;
        }
    }
}
