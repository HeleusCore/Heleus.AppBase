using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ExtScrollView), typeof(Heleus.Apps.Shared.UWP.Renderers.ExtendedScrollViewRenderer))]

namespace Heleus.Apps.Shared.UWP.Renderers
{
    public class ExtendedScrollViewRenderer : ScrollViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.CacheMode = null;
                UpdateTheme();

                if (Element is ExtScrollView scroller)
                {
                    scroller.ThemeChangedAction = UpdateTheme;
                }
            }
        }

        void UpdateTheme()
        {
            try
            {
                if (Theme.WindowTheme == WindowTheme.Dark)
                    Control.RequestedTheme = Windows.UI.Xaml.ElementTheme.Dark;
                else
                    Control.RequestedTheme = Windows.UI.Xaml.ElementTheme.Light;
            }
            catch { }
        }
    }
}
