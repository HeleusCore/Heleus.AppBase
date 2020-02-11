using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public abstract class MenuPage : StackPage
	{
		public readonly ExtMasterDetailPage MasterDetailPage;
		public readonly ExtNavigationPage NavigationPage;
		protected readonly List<Type> _pageTypeList = new List<Type>();

		public async Task ShowPage(Type pageType)
		{
			if (MasterDetailPage.MasterBehavior == MasterBehavior.Popover)
			{
				if (MasterDetailPage.IsPresented && !UIApp.IsWPF)
					MasterDetailPage.IsPresented = false;
			}

            await NavigationPage.ShowPage(pageType);
		}

        public async Task ShowPage(Func<Page, bool> isTargetPage, Func<Page> newPage)
        {
            if (MasterDetailPage.MasterBehavior == MasterBehavior.Popover)
            {
                if (MasterDetailPage.IsPresented && !UIApp.IsWPF)
                    MasterDetailPage.IsPresented = false;
            }

            await NavigationPage.ShowPage(isTargetPage, newPage);
        }

		protected MenuPage(ExtMasterDetailPage master, ExtNavigationPage rootPage) : base("MenuPage")
		{
			MasterDetailPage = master;
			NavigationPage = rootPage;

			//DisableAutoPadding = true;
		}

		protected override void SetupPadding() // ignore padding
		{
		}

		abstract public void AddPage(Type pageType, string title, char icon, int iconSize);
		abstract public void AddPage(Type pageType, string title, char icon);
        abstract public ButtonRow AddButton(Func<ButtonRow, Task> callback, string title, char icon);
	}
}
