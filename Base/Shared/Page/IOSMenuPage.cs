using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	class IOSMenuPage : MenuPage
	{
		public IOSMenuPage(ExtMasterDetailPage master, ExtNavigationPage rootPage) : base(master, rootPage)
		{
            SetTitle("☰");
            Padding = new Thickness(10, 10, 10, 0);
            StackLayout.Padding = new Thickness(0);

            var logo = this.AddLogo(200);
            logo.Margin = new Thickness(0, 60, 0, 20);
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

            var fontIcon = pageButton.SetDetailViewIcon(icon, IconSet.Light);
            fontIcon.ThemeUseFontWeight(false);
            fontIcon.FontStyle = Theme.RowIconFont;

            return pageButton;
        }

        protected override void UpdateColors()
		{
			BackgroundColor = Theme.PrimaryColor.Color.MultiplyAlpha(0.8);
		}
	}
}
