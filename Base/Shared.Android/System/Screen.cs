using System;
using Heleus.Apps.Shared.Android;

namespace Heleus.Apps.Shared
{
    public static class Screen
    {
		public static int StackPageWidth
		{
			get
			{
				return Math.Min (CurrentScreenWidth, 500) - 20;
			}
		}

        public static int CurrentScreenWidth
        {
            get 
            {
				var displayMetrics = MainActivity.Current.Resources.DisplayMetrics;
				return (int)(displayMetrics.WidthPixels / displayMetrics.Density);
			}
        }
    }
}
