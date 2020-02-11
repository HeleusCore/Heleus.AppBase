using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	class DesktopMenuPage : MenuPage
	{
		readonly List<ButtonRow> _buttons = new List<ButtonRow>();
		readonly ButtonRow _backButton;

		public DesktopMenuPage(ExtMasterDetailPage master, ExtNavigationPage navigation) : base(master, navigation)
		{
			Padding = new Thickness(10, 10, 10, 0);
			StackLayout.Padding = new Thickness(0);

			this.AddLogo(200);

			ScrollView.Content = null;
			RootLayout.Children.Remove(ScrollView);
			SetRootContent(StackLayout);

            if (!UIApp.IsWPF)
            {
                var popping = false;
                _backButton = AddButtonRow("Back", async (button) =>
                {
                    if (popping)
                        return;

                    popping = true;
                    if (navigation.Navigation.NavigationStack.Count > 1)
                        await navigation.PopAsync();
                    popping = false;
                });

                navigation.Popped += NavigationChanged;
                navigation.Pushed += NavigationChanged;

                //_backButton.Label.FontStyle = TextFont;
                _backButton.FontIcon.Icon = ' ';
                var fontIcon = _backButton.SetDetailViewIcon(Icons.BackButton, IconSet.Light);
                //fontIcon.Margin = new Thickness(2, 0, 0, 0);
                //fontIcon.ThemeUseFontWeight(false);
                //fontIcon.FontStyle = Theme.RowIconFont;

                _buttons.Add(_backButton);
            }

			NavigationChanged(null, null);
		}

		void NavigationChanged(object sender, NavigationEventArgs e)
		{
            if(_backButton != null)
			    _backButton.IsEnabled = NavigationPage.Navigation.NavigationStack.Count > 1;
		}

		void UpdatePageButton(ButtonRow button)
		{
			/*
			button.ResetColorSTyle();

			if (Theme.WindowTheme == WindowTheme.Dark)
			{
				button.ColorStyle = Black;
				button.Label.ColorStyle = White;
				button.FontIcon.ColorStyle = White;
				(button.DetailView as FontIcon).ColorStyle = White;
			}
			else
			{
				button.ColorStyle = White;
				button.Label.ColorStyle = Black;
				button.FontIcon.ColorStyle = Black;
				(button.DetailView as FontIcon).ColorStyle = Black;
			}
			*/
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
            //pageButton.Label.FontStyle = TextFont;
            pageButton.FontIcon.Icon = ' ';

            var fontIcon = pageButton.SetDetailViewIcon(icon, IconSet.Light);
            //fontIcon.Margin = new Thickness(2, 0, 0, 0);
            //fontIcon.ThemeUseFontWeight(false);
            //fontIcon.FontStyle = Theme.RowIconFont;
            //fontIcon.FontStyle = IconFont;

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
