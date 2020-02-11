using Heleus.Apps.Shared;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(PointerFrame), typeof(Heleus.Apps.Shared.UWP.Renderers.PointerFrameRenderer))]

namespace Heleus.Apps.Shared.UWP.Renderers
{
    public class PointerFrameRenderer : ViewRenderer<PointerFrame, Windows.UI.Xaml.Controls.Border>
    {
        PointerEventHandlerUWP pointerEvent = new PointerEventHandlerUWP();

        public PointerFrameRenderer()
        {
            AutoPackage = false;

            PointerEntered += pointerEvent.PointerEntered;
            PointerExited += pointerEvent.PointerExited;

            PointerPressed += pointerEvent.PointerPressed;
            PointerReleased += pointerEvent.PointerReleased;
            PointerMoved += pointerEvent.PointerMoved;
            PointerCanceled += pointerEvent.PointerCanceled;
            PointerCaptureLost += pointerEvent.PointerCanceled;
            PointerWheelChanged += pointerEvent.PointerCanceled;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<PointerFrame> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                    SetNativeControl(new Border());
                Control.CornerRadius = new Windows.UI.Xaml.CornerRadius(6);

                pointerEvent.Receiver = e.NewElement;

                PackChild();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (pointerEvent != null)
                {
                    PointerEntered -= pointerEvent.PointerEntered;
                    PointerExited -= pointerEvent.PointerExited;

                    PointerPressed -= pointerEvent.PointerPressed;
                    PointerReleased -= pointerEvent.PointerReleased;
                    PointerMoved -= pointerEvent.PointerMoved;
                    PointerCanceled -= pointerEvent.PointerCanceled;
                    PointerCaptureLost -= pointerEvent.PointerCanceled;
                    PointerWheelChanged -= pointerEvent.PointerCanceled;

                    pointerEvent = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Content")
            {
                PackChild();
            }
        }

        void PackChild()
        {
            if (Element.Content == null)
                return;

            IVisualElementRenderer renderer = Element.Content.GetOrCreateRenderer();
            Control.Child = renderer.ContainerElement;
        }
    }
}