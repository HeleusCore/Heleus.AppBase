using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(ExtEntry), typeof(Heleus.Apps.Shared.WPF.Renderers.ExtEntryRenderer))]
[assembly: ExportRenderer(typeof(ExtEditor), typeof(Heleus.Apps.Shared.WPF.Renderers.ExtEditorRenderer))]

namespace Heleus.Apps.Shared.WPF.Renderers
{
    class ExtEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if(Control != null)
            {
                Control.BorderBrush = Color.Transparent.ToBrush();
                Control.BorderThickness = new System.Windows.Thickness(0);
                Control.FontWeight = FontStyleExtension.ToWindowsFontWeight((Element as ExtEntry).FontWeight);
            }
        }
    }

    class ExtEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.BorderBrush = Color.Transparent.ToBrush();
                Control.BorderThickness = new System.Windows.Thickness(0);
                Control.FontWeight = FontStyleExtension.ToWindowsFontWeight((Element as ExtEditor).FontWeight);
            }
        }
    }
}
