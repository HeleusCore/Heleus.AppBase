using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(ExtLabel), typeof(Heleus.Apps.Shared.WPF.Renderers.ExtLabelRenderer))]

namespace Heleus.Apps.Shared.WPF.Renderers
{
    class ExtLabelRenderer : LabelRenderer
    {
        new ExtLabel Element => (ExtLabel)base.Element;

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            UpdateFontWeight();
            UpdateText();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ExtLabel.FontWeightProperty.PropertyName)
                UpdateFontWeight();

            if (e.PropertyName == Label.TextProperty.PropertyName || e.PropertyName == Label.FormattedTextProperty.PropertyName)
            {
                UpdateText();
            }
        }

        void UpdateFontWeight()
        {
            if (Control != null && Element != null)
            {
                Control.FontWeight = FontStyleExtension.ToWindowsFontWeight(Element.FontWeight);
            }
        }

        void UpdateText()
        {
            var spans = Element?.FormattedText?.Spans;
            if(spans != null && Control != null && spans.Count == Control.Inlines.Count)
            {
                var i = 0;
                foreach(var inline in Control.Inlines)
                {
                    inline.FontWeight = FontStyleExtension.ToWindowsFontWeight(spans[i].FontWeight());
                    i++;
                }
            }
        }
    }
}
