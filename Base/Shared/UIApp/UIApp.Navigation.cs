using System;
using System.Threading.Tasks;
using Heleus.Base;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    partial class UIApp
    {
        public bool IsTabbed => MainTabbedPage != null;

		public ExtTabbedPage MainTabbedPage { get; private set; }
		public ExtMasterDetailPage MainMasterDetailPage { get; private set; }
        public ExtNavigationPage MainNavigationPage { get; private set; }

        public async Task PopAllModal()
		{
			if (MainTabbedPage != null)
			{
				try
				{
					while (MainTabbedPage.CurrentPage.Navigation.ModalStack.Count > 0)
						await MainTabbedPage.CurrentPage.Navigation.PopModalAsync();

				}
                catch(Exception ex)
				{
                    Log.IgnoreException(ex);
				}
			}
			else if (MainMasterDetailPage != null)
			{
				while (MainMasterDetailPage.Detail.Navigation.ModalStack.Count > 1)
					await MainMasterDetailPage.Detail.Navigation.PopModalAsync();
			}
			else if (MainNavigationPage != null)
			{
				try
				{
					while (MainNavigationPage.Navigation.ModalStack.Count > 0)
						await MainNavigationPage.Navigation.PopModalAsync();

				}
				catch (Exception ex)
				{
                    Log.IgnoreException(ex);
				}
			}
		}

		public async Task PushModal(ExtContentPage page, bool useNavPage = true, bool animated = true)
		{
			Page newPage = page;
			if (useNavPage && !IsUWP)
			{
                var navPage = new ExtNavigationPage(page as ExtContentPage);
				newPage = navPage;
			}

			if (MainTabbedPage != null)
				await MainTabbedPage.CurrentPage.Navigation.PushModalAsync(newPage, animated);
			else if (MainMasterDetailPage != null)
			{
				if (IsUWP)
					await MainMasterDetailPage.Detail.Navigation.PushAsync(newPage, animated);
				else
					await MainMasterDetailPage.Detail.Navigation.PushModalAsync(newPage, animated);
			}
			else if (MainNavigationPage != null)
				await MainNavigationPage.Navigation.PushModalAsync(newPage, animated);
		}

		public async Task PopModal(ExtContentPage self, int count = 1)
		{
			if (IsUWP)
			{
				for (int i = 0; i < count; i++)
				{
					if (MainNavigationPage != null)
						await MainNavigationPage.Navigation.PopAsync();
					if (MainMasterDetailPage != null)
						await MainMasterDetailPage.Detail.Navigation.PopAsync();
				}
			}
			else
				await self.Navigation.PopModalAsync();
		}

        public async Task ShowPage(Type pageType, bool popToRoot = false)
		{
			if (MainTabbedPage != null)
			{
				while (MainTabbedPage.CurrentPage.Navigation.ModalStack.Count > 0)
					await MainTabbedPage.CurrentPage.Navigation.PopModalAsync();

                if (MainTabbedPage.ShowPage(pageType))
                {
                    while (MainTabbedPage.CurrentPage.Navigation.ModalStack.Count > 0)
                        await MainTabbedPage.CurrentPage.Navigation.PopModalAsync();

                    if (popToRoot)
                    {
                        while (MainTabbedPage.CurrentPage.Navigation.NavigationStack.Count > 1)
                            await MainTabbedPage.CurrentPage.Navigation.PopAsync();
                    }
                }
            }
			else if (MainMasterDetailPage != null)
			{
				await MainMasterDetailPage.MenuPage.ShowPage(pageType);
			}
			else if (MainNavigationPage != null)
			{
                await MainNavigationPage.ShowPage(pageType);
			}
		}

		public ExtContentPage CurrentPage
		{
			get
			{
				if (MainTabbedPage != null)
				{
					var page = MainTabbedPage.CurrentPage;
					return page.Navigation.NavigationStack[page.Navigation.NavigationStack.Count - 1] as ExtContentPage;
				}
				else if (MainMasterDetailPage != null)
				{
					var page = MainMasterDetailPage.Detail;
					return page.Navigation.NavigationStack[page.Navigation.NavigationStack.Count - 1] as ExtContentPage;
				}
				else if (MainNavigationPage != null)
				{
					var page = MainNavigationPage;
					return page.Navigation.NavigationStack[page.Navigation.NavigationStack.Count - 1] as ExtContentPage;
				}
				else
					return null;
			}
		}
    }
}
