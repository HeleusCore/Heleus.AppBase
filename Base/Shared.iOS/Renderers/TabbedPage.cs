using System;
using Heleus.Apps.Shared;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PRESERVE = Xamarin.Forms.Internals.PreserveAttribute;

[assembly: ExportRenderer(typeof(ExtTabbedPage), typeof(Heleus.Apps.Shared.iOS.Renderers.ExtendedTabbedPageRenderer))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
	[PRESERVE]
	class ExtendedTabbedPageRenderer : TabbedRenderer
	{
		Page Page => Element as Page;

		[PRESERVE]
		public ExtendedTabbedPageRenderer()
		{

		}

		void ThemeChanged()
		{
			try
			{
				if (TabBar != null)
				{
                    TabBar.TintColor = UITabBar.Appearance.TintColor;

                    var windowTheme = Theme.WindowTheme;
                    if (windowTheme == WindowTheme.Dark)
					{
						TabBar.BarStyle = UIBarStyle.Black;
						TabBar.UnselectedItemTintColor = Color.White.ToUIColor();
					}
					else
					{
						TabBar.BarStyle = UIBarStyle.Default;
						TabBar.UnselectedItemTintColor = Color.Black.ToUIColor();
					}
				}
			}
			catch
			{
			}
		}

		public override void ViewWillAppear(bool animated)
		{
			//EdgesForExtendedLayout = UIRectEdge.All;

			base.ViewWillAppear(animated);
            ThemeChanged();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			TabBar.Translucent = true;
			//TabBar.UnselectedItemTintColor = Theme.PrimaryColor.Color.ToUIColor();
			ThemeChanged();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			var frame = View.Frame;
			Page.ContainerArea = new Rectangle(0, 0, frame.Width, frame.Height);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (TabBar == null)
				return;

			(Element as ExtTabbedPage).ThemeChangedAction = ThemeChanged;
		}
	}
}
