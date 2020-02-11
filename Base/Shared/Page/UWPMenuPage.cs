using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	class UWPMenuPage : MenuPage
	{
		static readonly FontStyle TextFont = new ThemedFontStyle(ThemeFontStyle.None, FontWeight.Regular, 18);
		static readonly FontStyle IconFont = new ThemedFontStyle(ThemeFontStyle.None, FontWeight.Light, 28);

		static readonly ColorStyle Black = new ColorStyle(ThemeColorStyle.None, Color.Black);
		static readonly ColorStyle White = new ColorStyle(ThemeColorStyle.None, Color.White);

        public static readonly ColorStyle LightRowColor = new ColorStyle(ThemeColorStyle.None, Color.White.MultiplyAlpha(Theme.RowAlpha), true);
        public static readonly ColorStyle LightRowHoverColor = new ColorStyle(ThemeColorStyle.None, Color.White.MultiplyAlpha(Theme.RowHoverAlpha), true);
        public static readonly ColorStyle LightRowHighlightColor = new ColorStyle(ThemeColorStyle.None, Color.White.MultiplyAlpha(Theme.RowHighlightAlpha), true);
        public static readonly ColorStyle LightRowDisabledColor = new ColorStyle(ThemeColorStyle.None, Color.White.MultiplyAlpha(Theme.RowDisabledAlpha), true);

        class LightButtonStyle : IButtonStyle
        {
            public ColorStyle Color => LightRowColor;
            public ColorStyle HoverColor => LightRowHoverColor;
            public ColorStyle HighlightColor => LightRowHighlightColor;
            public ColorStyle DisabledColor => LightRowDisabledColor;
        }

        readonly List<ButtonRow> _buttons = new List<ButtonRow>();

		public UWPMenuPage(ExtMasterDetailPage master, ExtNavigationPage navigation) : base(master, navigation)
		{
			Padding = new Thickness(0, 10, 0, 0);
			StackLayout.Padding = new Thickness(0);
		}

		void UpdatePageButton(ButtonRow button)
		{
			//button.RowAlpha = 0;

			if (Theme.WindowTheme == WindowTheme.Dark)
			{
                //button.ColorStyle = Black;
                button.RowStyle = Theme.RowButton;

                button.Label.ColorStyle = White;
				button.FontIcon.ColorStyle = White;
				(button.DetailView as FontIcon).ColorStyle = White;
			}
			else
			{
                button.RowStyle = new LightButtonStyle();

                button.Label.ColorStyle = Black;
				button.FontIcon.ColorStyle = Black;
				(button.DetailView as FontIcon).ColorStyle = Black;
			}

            button.ResetColorStyle();
        }

        public override void AddPage(Type pageType, string title, char icon, int iconSize)
		{
			AddPage(pageType, title, icon);
		}

		public override void AddPage(Type pageType, string title, char icon)
		{
			var pageButton = AddButton((button) => ShowPage(button.Tag as Type), title, icon);
			pageButton.Tag = pageType;
		}

        public override ButtonRow AddButton(Func<ButtonRow, Task> callback, string title, char icon)
        {
            var pageButton = AddButtonRow(null, callback);

            pageButton.Label.Text = Tr.Get(title);
            pageButton.Label.FontStyle = TextFont;

            var fontIcon = pageButton.SetDetailViewIcon(icon, IconSet.Light, 30);
            fontIcon.Margin = new Thickness(2, 0, 0, 0);
            fontIcon.ThemeUseFontWeight(false);
            fontIcon.FontStyle = IconFont;

            _buttons.Add(pageButton);
            UpdatePageButton(pageButton);

            return pageButton;
        }

        public override void ThemeChanged()
		{
			base.ThemeChanged();

			foreach (var button in _buttons)
				UpdatePageButton(button);
		}

		protected override void UpdateColors()
		{
			BackgroundColor = Theme.PrimaryColor.Color;
		}
    }
}
