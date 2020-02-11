using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ExtPicker), typeof(Heleus.Apps.Shared.UWP.Renderers.ExtendedPickerRenderer))]

namespace Heleus.Apps.Shared.UWP.Renderers
{
    public class ExtendedPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                //Control.VerticalContentAlignment = VerticalAlignment.Center;
                //Control.VerticalAlignment = VerticalAlignment.Center;

                Control.BorderThickness = new Windows.UI.Xaml.Thickness(0);
                UpdateTheme();

                if(e.NewElement is ExtPicker picker)
                {
                    picker.ThemeChangedAction = UpdateTheme;

                    if(picker.ThemeUseFontSize())
                        Control.FontSize = picker.FontSize;
                    if(picker.ThemeUseFontWeight() && picker.FontStyle != null)
                    {
                        var weight = picker.FontStyle.FontWeight.ToWindowsFontWeight();
                        if (!weight.Equals(Control.FontWeight))
                            Control.FontWeight = weight;
                    }
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
