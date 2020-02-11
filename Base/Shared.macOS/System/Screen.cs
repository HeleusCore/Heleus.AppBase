using System;
using System.Linq;
using AppKit;
using Heleus.Apps.Shared.macOS;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public static class Screen
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
				if (UIApp.IsMacOS || (UIApp.IsUWP && Device.Idiom != TargetIdiom.Phone)) // a small margin on the borders on desktops, looks better imho
					return 25;
				
				return 0;
			}
		}

		public static int CurrentScreenWidth
		{
			get
			{
				//var window = NSApplication.SharedApplication.DangerousWindows.FirstOrDefault();
				var window = AppDelegate.Window;
				if (window != null)
					return (int)window.Frame.Width;
				return 800;
            }
		}
	}
}
