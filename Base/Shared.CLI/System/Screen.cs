using System;
using System.Linq;
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
				return 0;
			}
		}

		public static int CurrentScreenWidth
		{
			get
			{
				return 800;
            }
		}
	}
}
