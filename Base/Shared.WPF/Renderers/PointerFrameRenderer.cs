using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(PointerFrame), typeof(Heleus.Apps.Shared.WPF.Renderers.PointerFrameRenderer))]

namespace Heleus.Apps.Shared.WPF.Renderers
{
    public class PointerBorder : Border
    {
        readonly IPointerHandler _pointerHandler;

        public PointerBorder(IPointerHandler handler)
        {
            _pointerHandler = handler;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if(_pointerHandler != null)
            {
                var local = e.GetPosition(this);
                var global = e.GetPosition(null);

                _pointerHandler.PointerDown(global.X, global.Y, local.X, local.Y);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _pointerHandler?.PointerUp(true);
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_pointerHandler != null)
            {
                var local = e.GetPosition(this);
                var global = e.GetPosition(null);

                _pointerHandler.PointerMoved(global.X, global.Y, local.X, local.Y);
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            _pointerHandler?.PointerEnter();

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _pointerHandler?.PointerExit();
            _pointerHandler?.PointerUp(false);

            base.OnMouseLeave(e);
        }
    }

    public class PointerFrameRenderer : ViewRenderer<PointerFrame, PointerBorder>
    {
        VisualElement _currentView;

        public PointerFrameRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<PointerFrame> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    SetNativeControl(new PointerBorder((e.NewElement as IPointerEventReceiver)?.Handler));
                    Control.CornerRadius = new System.Windows.CornerRadius(6);
                }

                UpdateContent();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Xamarin.Forms.Frame.ContentProperty.PropertyName)
                UpdateContent();
        }

        protected override void UpdateBackground()
        {
            Control.UpdateDependencyColor(Border.BackgroundProperty, Element.BackgroundColor);
        }

        void UpdateContent()
        {
            if (_currentView != null)
            {
                _currentView.Cleanup(); // cleanup old view
            }

            _currentView = Element.Content;
            Control.Child = _currentView != null ? Platform.GetOrCreateRenderer(_currentView).GetNativeElement() : null;
        }
    }
}

