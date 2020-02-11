using System.Diagnostics.Contracts;
using Pango;
using Xamarin.Forms;

namespace Heleus.Apps.Shared.GTK.Renderers
{
	internal static class FontDescriptionHelper
	{
		internal static FontDescription CreateFontDescription(double fontSize, string fontFamily, FontAttributes attributes, FontWeight fontWeight)
		{
			FontDescription fontDescription = new FontDescription();
			fontDescription.Size = (int)(fontSize * Scale.PangoScale);
			fontDescription.Family = fontFamily;
			fontDescription.Style = attributes == FontAttributes.Italic ? Pango.Style.Italic : Pango.Style.Normal;
			fontDescription.Weight = GetWeight(fontWeight);

			return fontDescription;
		}

		static Weight GetWeight(FontWeight fontWeight)
		{
			switch (fontWeight)
			{
				case FontWeight.Thin:
					return Weight.Ultralight;
				case FontWeight.Light:
					return Weight.Light;
				case FontWeight.Medium:
					return Weight.Semibold;
				case FontWeight.Bold:
					return Weight.Bold;
				default:
					return Weight.Normal;
			}
		}
	}
}
