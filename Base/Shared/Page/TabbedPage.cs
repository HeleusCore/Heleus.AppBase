using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtTabbedPage : TabbedPage, IThemeable
	{
		public Action ThemeChangedAction = null;

		public ExtTabbedPage()
		{
			Xamarin.Forms.PlatformConfiguration.macOSSpecific.TabbedPage.SetTabsStyle(this, TabsStyle.OnNavigation);
		}

        protected override void OnCurrentPageChanged()
        {
            ((CurrentPage as ExtNavigationPage)?.CurrentPage as ExtContentPage)?.OnOpen();
        }

        public void AddPage(Type pageType, string title, string icon)
        {
            var navigation = new ExtNavigationPage((Xamarin.Forms.Page)Activator.CreateInstance(pageType), false) { Title = Tr.Get(title), IconImageSource = icon };
            Children.Add(navigation);
        }

        public bool ShowPage(Type pageType)
        {
            foreach(var child in Children)
            {
                var navigation = child as ExtNavigationPage;
                if(navigation.RootPage.GetType() == pageType)
                {
                    CurrentPage = navigation;
                    return true;
                }
            }
            return false;
        }

		public void ThemeChanged()
		{
			ThemeChangedAction?.Invoke();

			foreach (var page in Children)
			{
                if (page is IThemeable p)
                {
                    p.ThemeChanged();
                }
            }
        }
	}
}
