using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ExtLabel), typeof(Heleus.Apps.Shared.UWP.Renderers.ExtendedLabelRenderer))]

namespace Heleus.Apps.Shared.UWP.Renderers
{
    public class ExtendedLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            if (Control != null && Element != null)
            {
                UpdateText();
            }
        }

        void UpdateText()
        {
            var label = (Element as ExtLabel);
            if (label == null)
                return;

            try
            {
                Control.LineStackingStrategy = Windows.UI.Xaml.LineStackingStrategy.BaselineToBaseline;
                if (label.ThemeUseFontWeight())
                {
                    var weight = label.FontWeight.ToWindowsFontWeight();
                    if (!weight.Equals(Control.FontWeight))
                        Control.FontWeight = weight;
                }

                if (Element.FormattedText != null && (Element.FormattedText.Spans.Count == Control.Inlines.Count))
                {
                    for(int i = 0; i < Element.FormattedText.Spans.Count; i++)
                    {
                        var span = Element.FormattedText.Spans[i];
                        var inline = Control.Inlines[i];

                        var weight = span.FontWeight().ToWindowsFontWeight();
                        if (!weight.Equals(inline.FontWeight))
                            inline.FontWeight = weight;
                    }
                }
            }
            catch { }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == Label.TextProperty.PropertyName || e.PropertyName == Label.FormattedTextProperty.PropertyName || e.PropertyName == ExtLabel.FontWeightProperty.PropertyName)
            {
                UpdateText();
            }
        }
    }
}
