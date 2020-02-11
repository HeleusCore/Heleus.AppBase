using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ExtSwitch), typeof(Heleus.Apps.Shared.UWP.Renderers.ExtendedSwitchRenderer))]

namespace Heleus.Apps.Shared.UWP.Renderers
{
    public class ExtendedSwitchRenderer : SwitchRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.OffContent = null;
                Control.OnContent = null;
                UpdateTheme();

                if ((e.NewElement is ExtSwitch swt))
                {
                    swt.ThemeChangedAction = UpdateTheme;
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
