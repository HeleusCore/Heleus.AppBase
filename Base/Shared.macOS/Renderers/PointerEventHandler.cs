using System;
using AppKit;
using CoreGraphics;

namespace Heleus.Apps.Shared
{
	public class PointerEventHandlerMacOS : PointerEventHandler
    {
		public void MouseUp(NSEvent theEvent, CGPoint localLocation)
		{
            if (_receiver == null)
                return;
            
            Up(true);
		}

		public void MouseDown(NSEvent theEvent, CGPoint localLocation)
		{
            if (_receiver == null)
                return;

            var p = theEvent.LocationInWindow;
			Down(p.X, p.Y, localLocation.X, localLocation.Y);
		}

		public void MouseMoved(NSEvent theEvent, CGPoint localLocation)
		{
            if (_receiver == null)
                return;

            var p = theEvent.LocationInWindow;
			Moved(p.X, p.Y, localLocation.X, localLocation.Y);
		}

		public void MouseEntered(NSEvent theEvent)
		{
            if (_receiver == null)
                return;

            Enter();
		}

		public void MouseExited(NSEvent theEvent)
		{
            if (_receiver == null)
                return;

            Exit();
			Up(false);
		}

		public void SubmitKey()
		{
			if (_receiver == null)
				return;
			Submit();
		}
	}
}
