using Heleus.Apps.Shared.WPF;
using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class Screen
	{
		public static int StackPageWidth
		{
			get
			{
				return Math.Min(CurrentScreenWidth, 800) - 2 * StackPagePadding;
			}
		}

		public static int StackPagePadding
		{
			get
			{
				if (UIApp.IsDesktop || (UIApp.IsUWP && Device.Idiom != TargetIdiom.Phone)) // a small margin on the borders on desktops, looks better imho
					return 25;

				return 0;
			}
		}

		public static int CurrentScreenWidth
		{
			get
			{
                if (MainWindow.Instance != null && MainWindow.Instance.ActualWidth > 0)
                {
                    var diff = 0;
                    if (UIApp.Current.MainMasterDetailPage.IsPresented)
                        diff = 300;
                    return (int)MainWindow.Instance.ActualWidth - diff;
                }

				return 800;
			}
		}
	}
}
