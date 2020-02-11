using System;
using System.Linq;
using Foundation;
using Heleus.Base;
using UIKit;

namespace Heleus.Apps.Shared.iOS.Renderers
{
	public class TouchEventHandlerIOS : PointerEventHandler
	{
		public void TouchesBegan(NSSet touches, UIEvent evt, UIView view)
		{
			if (_receiver == null)
				return;

			if (touches != null && touches.Count == 1)
			{
				try
				{
					var t = touches.ToArray<UITouch>().FirstOrDefault();

					var p = t.LocationInView(null);
					var l = t.LocationInView(view);
					Down(p.X, p.Y, l.X, l.Y);
				}
				catch (Exception ex)
				{
					Log.IgnoreException(ex);
				}
			}
		}

		public void TouchesMoved(NSSet touches, UIEvent evt, UIView view)
		{
			if (_receiver == null)
				return;

			try
			{
				var t = touches.ToArray<UITouch>().FirstOrDefault();

				var p = t.LocationInView(null);
				var l = t.LocationInView(view);

				Moved(p.X, p.Y, l.X, l.Y);
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}
		}

		public void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			if (_receiver == null)
				return;

			Up(false);
		}

		public void TouchesEnded(NSSet touches, UIEvent evt)
		{
			if (_receiver == null)
				return;

			Up(true);
		}
	}
}
