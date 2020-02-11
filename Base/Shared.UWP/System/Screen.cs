using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public static class Screen
    {
        public static int StackPageWidth
        {
            get
            {
                var border = 10;
                if (Device.Idiom != TargetIdiom.Phone)
                    border = 40;

                return Math.Min(CurrentScreenWidth, 560) - border;
            }
        }

        public static int CurrentScreenWidth
        {
            get
            {
                var bounds = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBounds.Width;
                if (Device.Idiom != TargetIdiom.Phone)
                    bounds -= UIApp.WindowsPartialCollapseSize; // Hamburger Menu

                return (int)bounds;
            }
        }
    }
}
