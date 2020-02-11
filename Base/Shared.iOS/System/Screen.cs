using System;
using UIKit;

namespace Heleus.Apps.Shared
{
    public static class Screen
    {
		public static int StackPageWidth
		{
			get
			{
				var width = CurrentScreenWidth;
				if (width >= 1366) // iPad Pro
					return 900;
				if (width >= 1024)
					return 800;
				if (width >= 768) // iPad Portrait
					return 700;

				return Math.Min (CurrentScreenWidth, 510) - 10;
			}
		}

        public static int CurrentScreenWidth
        {
            get 
            {
                return (int)UIScreen.MainScreen.Bounds.Width;
            }
        }
    }
}
