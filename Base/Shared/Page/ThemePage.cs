using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System;

namespace Heleus.Apps.Shared
{
	class ThemePage : StackPage
	{
		async Task ChangeColor(ButtonRow button)
		{
			var style = button.Tag as ThemedColorStyle;
			await Navigation.PushAsync(new ColorPickerPage(style, async (newColor) =>
			{
				style.UpdateColorStyle(newColor);
				(button.DetailView as ExtBoxView).BackgroundColor = newColor;
				await Theme.PropagateChanges();
			}));
		}

		async Task ChangeFont(ButtonRow button)
		{
			var style = button.Tag as ThemedFontStyle;
			await Navigation.PushAsync(new FontPickerPage(style, async (fontWeight, size) => {
				style.UpdateFontStyle(fontWeight, size);
				await Theme.PropagateChanges();
			}));
		}

		async Task WindowsThemeChanged(WindowTheme item)
		{
			if (item != Theme.WindowTheme)
			{
                Theme.WindowTheme = item;
                await Theme.PropagateChanges();
			}
		}

		public ThemePage() : base("ThemePage")
		{
			AddTitleRow("Title");

			var swtch = AddSwitchRow("Enable", true);
			swtch.Switch.IsToggled = Theme.ThemeMode == ThemeMode.Custom;
			swtch.Switch.ToggledAsync = async (sw) =>
			{
				await Theme.SwitchTheme(sw.IsToggled ? ThemeMode.Custom : ThemeMode.Default);

				var row = GetRow<SelectionRow<WindowTheme>>("WindowTheme");
				if (row != null)
					row.Selection = Theme.WindowTheme;
			};

			AddFooterRow();

			AddHeaderRow("ColorSection");

			var colorStyles = ColorStyle.ColorStyles.Values;

			ColorStyle last = null;
			foreach (var colorStyle in colorStyles)
			{
				if (colorStyle is ThemedColorStyle)
					last = colorStyle;
			}


			foreach (var colorStyle in colorStyles)
			{
				if (colorStyle is ThemedColorStyle themedStyle)
				{
#if !DEBUG
					if (colorStyle.ThemeColorStyle == ThemeColorStyle.None)
						continue;
#endif
					var button = AddButtonRow(Tr.Get("Theme.Colors." + themedStyle.ThemeColorStyle.ToString()), ChangeColor, colorStyle == last);
					button.SetDetailView(new ExtBoxView { BackgroundColor = themedStyle.ThemedColor});
					button.Tag = themedStyle;
				}
			}

			AddFooterRow ();

			if(UIApp.IsMacOS || UIApp.IsIOS || UIApp.IsUWP)
            {
                var items = new SelectionItemList<WindowTheme>
                {
					new SelectionItem<WindowTheme>(WindowTheme.Light, Tr.Get("Theme.Window.Light")),
					new SelectionItem<WindowTheme>(WindowTheme.Dark, Tr.Get("Theme.Window.Dark"))
                };

                AddHeaderRow("WindowSection");
                var theme = AddSelectionRows(items, Theme.WindowTheme, "WindowTheme");
                theme.SelectionChanged = WindowsThemeChanged;
                AddFooterRow();
            }

			AddHeaderRow("FontSection");

			var fontStyles = FontStyle.FontStyles.Values;
			FontStyle lastFont = null;

			foreach (var fontStyle in fontStyles)
			{
				if (fontStyle is ThemedFontStyle)
					lastFont = fontStyle;
			}

			foreach (var fontStyle in fontStyles)
			{
				if (fontStyle is ThemedFontStyle themedStyle)
				{
					var button = AddButtonRow(Tr.Get("Theme.Fonts." + themedStyle.ThemeFontStyle.ToString()), ChangeFont, fontStyle == lastFont);
					button.Tag = fontStyle;
				}
			}

			AddFooterRow();
        }
    }
}
