using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

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
				return 25;// a small margin on the borders on desktops, looks better imho
			}
		}

		public static int CurrentScreenWidth
		{
			get
			{
				var window = FormsWindow.MainWindow;
				if (window == null)
					return 800 - 300;
				
				window.GetSize(out var width, out var height);
				return width - 300; // split view
			}
		}
	}
}
