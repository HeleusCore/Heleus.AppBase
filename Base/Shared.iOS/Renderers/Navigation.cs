using System;
using Heleus.Apps.Shared;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtNavigationPage), typeof(Heleus.Apps.Shared.iOS.Renderers.ExtNavigationRenderer))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
	public class ExtNavigationRenderer : NavigationRenderer
	{
		public override UIKit.UIStatusBarStyle PreferredStatusBarStyle()
		{
			if (Theme.WindowTheme == WindowTheme.Dark)
				return UIKit.UIStatusBarStyle.LightContent;

			return UIStatusBarStyle.Default;
		}

		void ThemeChanged()
		{
			try
			{
				if (NavigationBar != null)
				{
					var theme = Theme.WindowTheme;
					if (theme == WindowTheme.Dark)
						NavigationBar.BarStyle = UIBarStyle.Black;
					else
						NavigationBar.BarStyle = UIBarStyle.Default;
				}
			}
			catch
			{
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			(Element as ExtNavigationPage).ThemeChangedAction = ThemeChanged;
			ThemeChanged();
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //NavigationController.HidesBarsOnSwipe = true;
        }
    }
}
