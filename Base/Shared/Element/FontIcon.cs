using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public enum IconSet
    {
		// Font Awesome
        Light,
        Regular,
        Solid,
        Brands
    }

    public class FontIcon : ExtLabel
    {
#if WPF
        static readonly string WPFFontUri = new Uri(AppDomain.CurrentDomain.BaseDirectory).AbsoluteUri;
#else
        static readonly string WPFFontUri = string.Empty;
#endif
        public FontIcon()
        {
            HorizontalTextAlignment = TextAlignment.Center;
            VerticalTextAlignment = TextAlignment.Center;
			this.ThemeUseFontWeight(false);
			InputTransparent = true;
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                throw new ArgumentException();
            }
        }

        public void SetIcon(IconSet iconSet, char c)
        {
			if (c == ' ')
			{
				base.Text = string.Empty;
				return;
			}

			var fontInfo = GetIconFontInfo(c, iconSet);
			if (fontInfo == null)
				throw new ArgumentException();

            IconSet = iconSet;

			var family = string.Empty;
			if (UIApp.IsIOS || UIApp.IsGTK)
				family = fontInfo.PostScriptName;
			else if (UIApp.IsAndroid)
				family = fontInfo.FileName;
			else if (UIApp.IsUWP)
				family = "/Assets/" + fontInfo.Windows;
			else if (UIApp.IsMacOS)
				family = fontInfo.FullName;
            else if (UIApp.IsWPF)
                family = WPFFontUri + fontInfo.Windows;

            if (FontFamily != family)
				FontFamily = family;

			base.Text = c.ToString();
        }

        public IconSet IconSet
        {
            get;
            private set;
        } = IconSet.Light;

        public char Icon
        {
            set
            {
                SetIcon(IconSet, value);
            }

			get
			{
				if(!string.IsNullOrEmpty(base.Text))
				{
					return base.Text[0];
				}
				
				return (char)0;
			}
        }

        public class FontInfo
        {
            readonly char startRange;
            readonly char endRange;
            readonly IconSet iconSet;

            public string PostScriptName;
            public string FileName;
            public string Windows;
            public string FullName;
            
            public FontInfo(char firstChar, char lastChar, IconSet iconSet = IconSet.Light)
            {
                startRange = firstChar;
                endRange = lastChar;
                this.iconSet = iconSet;
            }

            public bool Contains(IconSet iconSet, char c)
            {
                return (c >= startRange && c <= endRange && this.iconSet == iconSet);
            }
        }

        static List<FontInfo> infoList = new List<FontInfo>();

        FontInfo GetIconFontInfo(char c, IconSet iconSet)
        {
            foreach (var info in infoList)
            {
                if (info.Contains(iconSet, c))
                {
                    return info;
                }
            }
            return null;
        }

        public static void RegisterIconFont(FontInfo fontInfo)
        {
            infoList.Add(fontInfo);
        }
	}
}
