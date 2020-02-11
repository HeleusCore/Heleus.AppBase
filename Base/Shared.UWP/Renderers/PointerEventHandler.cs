using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace Heleus.Apps.Shared.UWP
{
    class PointerEventHandlerUWP : PointerEventHandler
    {
        public void PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("PointerEntered");
            if(e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                Enter();
        }

        public void PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("PointerExited");
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                Exit();
                Up(false);
            }
        }

        public void PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_receiver == null)
                return;

            var p = e.GetCurrentPoint(null);
            var l = e.GetCurrentPoint(sender as UIElement);
            Down(p.Position.X, p.Position.Y, l.Position.X, l.Position.Y);
        }

        public void PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_receiver == null)
                return;

            Up(true);
        }

        public void PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_receiver == null)
                return;

            Up(false);
        }

        public void PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_receiver == null)
                return;

            var p = e.GetCurrentPoint(null);
            var l = e.GetCurrentPoint(sender as UIElement);

            Moved(p.Position.X, p.Position.Y, l.Position.X, l.Position.Y);
        }
    }
}
