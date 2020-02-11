using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Win = Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Mac = Xamarin.Forms.PlatformConfiguration.macOSSpecific;
using iOSspec = Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Heleus.Apps.Shared
{
	public class ExtNavigationPage : NavigationPage, IThemeable
	{
		public async Task ShowPage(Type pageType)
		{
            for (int i = 0; i < Navigation.NavigationStack.Count; i++)
            {
                var p = Navigation.NavigationStack[i];
                if (p.GetType() == pageType)
                {
                    while (true)
                    {
                        var currentType = Navigation.NavigationStack[Navigation.NavigationStack.Count - 1].GetType();
                        if (currentType == pageType)
                        {
                            (p as ExtContentPage)?.OnOpen();
                            break;
                        }

                        await Navigation.PopAsync();
                    }
                    return;
                }
            }

            var page = (Xamarin.Forms.Page)Activator.CreateInstance(pageType);
            (page as ExtContentPage)?.OnOpen();
            await Navigation.PushAsync(page);
        }

        public async Task ShowPage(Func<Page, bool> isTargetPage, Func<Page> newPage)
        {
            var pageFound = false;
            var pageCount = Navigation.NavigationStack.Count - 1;

            for (var i = pageCount; i >= 0; i--)
            {
                var page = Navigation.NavigationStack[i];
                if (isTargetPage.Invoke(page))
                {
                    pageFound = true;
                    break;
                }
            }

            if (pageFound)
            {
                for (var i = pageCount; i >= 0; i--)
                {
                    var page = Navigation.NavigationStack[i];
                    if (isTargetPage.Invoke(page))
                    {
                        (page as ExtContentPage)?.OnOpen();
                        return;
                    }

                    if (i > 0)
                        await Navigation.PopAsync();
                }
                return;
            }

            var p = newPage.Invoke();
            (p as ExtContentPage)?.OnOpen();
            await Navigation.PushAsync(p);
        }

        public Action ThemeChangedAction = null;

		public ExtNavigationPage(Page page, bool executeOnOpen = true) : base(page)
		{
			if (page is ExtContentPage)
			{
				Popped += PagePopped;
			}

            if(executeOnOpen)
                (CurrentPage as ExtContentPage)?.OnOpen();

			Win.Page.SetToolbarPlacement(this, Win.ToolbarPlacement.Top);
			Mac.NavigationPage.SetNavigationTransitionStyle(this, Mac.NavigationTransitionStyle.Crossfade, Mac.NavigationTransitionStyle.None);

			iOSspec.NavigationPage.SetIsNavigationBarTranslucent(this, true);
			iOSspec.NavigationPage.SetStatusBarTextColorMode(this, iOSspec.StatusBarTextColorMode.MatchNavigationBarTextLuminosity);
			//iOS.NavigationPage.SetStatusBarTextColorMode(this, iOS.StatusBarTextColorMode.MatchNavigationBarTextLuminosity);
			//iOS.NavigationPage.EnableTranslucentNavigationBar(On<Xamarin.Forms.PlatformConfiguration.iOS>());
			//On<Xamarin.Forms.PlatformConfiguration.iOS> ().EnableTranslucentNavigationBar ();

			UpdateColors();
		}

		protected override bool OnBackButtonPressed()
		{
#if ANDROID
            //Console.WriteLine ("ModalStack: " + page.Navigation.ModalStack.Count + ", NavigationStack: " + page.Navigation.NavigationStack.Count);
            if (Navigation.ModalStack.Count == 0 && Navigation.NavigationStack.Count == 1) {
				Android.MainActivity.Current.MoveTaskToBack(true);
                return true;
            }
#endif
			return base.OnBackButtonPressed();
		}

		void PagePopped (object sender, NavigationEventArgs e)
		{
            (CurrentPage as ExtContentPage)?.OnOpen();
            (e.Page as ExtContentPage)?.OnPopped();
		}

		void UpdateColors ()
		{
            if (UIApp.IsAndroid)
            {
                BackgroundColor = Color.Transparent;
                BarBackgroundColor = Color.Black.MultiplyAlpha(0.2);
            }
            else if (UIApp.IsUWP)
            {
                BackgroundColor = Color.Transparent;
            }
            else
            {
				BackgroundColor = Theme.PrimaryColor.Color;
            }

			if (UIApp.IsIOS || UIApp.IsUWP)
			{
				if (Theme.WindowTheme == WindowTheme.Dark)
					BarTextColor = Color.White;
				else
					BarTextColor = Color.Black;
			}

            if(UIApp.IsUWP || UIApp.IsWPF)
            {
				BarBackgroundColor = Theme.SecondaryColor.Color;
            }
		}

		public virtual void ThemeChanged ()
		{
			UpdateColors ();
			ThemeChangedAction?.Invoke();

			for (int i = 0; i < Navigation.NavigationStack.Count; i++) {
				if (Navigation.NavigationStack [i] is IThemeable p)
					p.ThemeChanged ();
			}
		}
	}
}
