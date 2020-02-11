using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ExtEntry), typeof(Heleus.Apps.Shared.UWP.Renderers.ExtendedEntryRenderer))]
[assembly: ExportRenderer(typeof(ExtEditor), typeof(Heleus.Apps.Shared.UWP.Renderers.ExtendedEditorRenderer))]

namespace Heleus.Apps.Shared.UWP.Renderers
{
    public class ExtendedEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.BorderThickness = new Windows.UI.Xaml.Thickness(0);
                Control.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
                UpdateTheme();

                if (e.NewElement is ExtEditor editor)
                {
                    editor.ThemeChangedAction = UpdateTheme;

                    var fontStyle = editor.FontStyle;
                    if (e.NewElement.ThemeUseFontWeight() && fontStyle != null)
                    {
                        var weight = fontStyle.FontWeight.ToWindowsFontWeight();
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

    public class ExtendedEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                //Control.VerticalContentAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                //Control.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

                Control.BorderThickness = new Windows.UI.Xaml.Thickness(0);
                UpdateTheme();

                if (e.NewElement is ExtEntry entry)
                {
                    entry.ThemeChangedAction = UpdateTheme;

                    var fontStyle = entry.FontStyle;
                    if(e.NewElement.ThemeUseFontWeight() && fontStyle != null)
                    {
                        var weight = fontStyle.FontWeight.ToWindowsFontWeight();
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
