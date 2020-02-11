using System;
using Xamarin.Forms;
using Win = Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Heleus.Apps.Shared
{
    public class ExtMasterDetailPage : MasterDetailPage, IThemeable
	{
		public ExtMasterDetailPage ()
		{
			if (Device.Idiom != TargetIdiom.Phone)
                Win.MasterDetailPage.SetCollapseStyle (this, Win.CollapseStyle.Partial);

            Win.Page.SetToolbarPlacement (this, Win.ToolbarPlacement.Top);

			if (UIApp.IsGTK || UIApp.IsMacOS)
				MasterBehavior = MasterBehavior.Split;
			else
				MasterBehavior = MasterBehavior.Popover;

            if(UIApp.IsWPF)
                IsPresented = true;

            UpdateColors();
		}

		protected override bool OnBackButtonPressed ()
		{
			if (MasterBehavior == MasterBehavior.Popover && IsPresented)
			{
				IsPresented = false;
				return true;
			}

#if ANDROID
			var page = Detail;
			//Console.WriteLine ("ModalStack: " + page.Navigation.ModalStack.Count + ", NavigationStack: " + page.Navigation.NavigationStack.Count);
			if (page.Navigation.ModalStack.Count == 0 && page.Navigation.NavigationStack.Count == 1) {
				global::Heleus.Apps.Shared.Android.MainActivity.Current.MoveTaskToBack(true);
            	return true;
			}
#endif
			return base.OnBackButtonPressed ();
		}

		void UpdateColors()
		{
			if (UIApp.IsAndroid || UIApp.IsUWP)
				BackgroundColor = Color.Transparent;
			else
				BackgroundColor = Theme.PrimaryColor.Color;
		}

		public void ThemeChanged ()
		{
			(Detail as IThemeable).ThemeChanged ();
			(Master as IThemeable).ThemeChanged ();

			UpdateColors();
		}

		public MenuPage MenuPage
		{
			get
			{
				return Master as MenuPage;
			}
		}
	}
}
