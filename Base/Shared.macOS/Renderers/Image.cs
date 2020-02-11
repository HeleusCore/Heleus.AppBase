using System;
using System.ComponentModel;
using CoreAnimation;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ExtImage), typeof(Heleus.Apps.Shared.macOS.Renderers.ExtImageRenderer))]

namespace Heleus.Apps.Shared.macOS.Renderers
{
    public class ExtImageRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if(Control != null)
            {
                UpdateScaling();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if(e.PropertyName == Image.AspectProperty.PropertyName)
            {
                UpdateScaling();
            }
        }

        void UpdateScaling()
        {
            if(Element != null)
            {
                var aspect = Element.Aspect;

                /*
                Control.Layer.ContentsGravity = aspect.ToNSViewContentMode();
                Control.WantsLayer = true;

                Layer.ContentsGravity = aspect.ToNSViewContentMode();
                WantsLayer = true;

                return;
                */

                if (aspect == Aspect.Fill)
                    Control.ImageScaling = AppKit.NSImageScale.AxesIndependently;
                else if (aspect == Aspect.AspectFit)
                    Control.ImageScaling = AppKit.NSImageScale.ProportionallyUpOrDown;
                else if (aspect == Aspect.AspectFill)
                    Control.ImageScaling = AppKit.NSImageScale.ProportionallyUpOrDown;

            }
        }
    }
}
