using System;
using AppKit;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(PointerFrame), typeof(Heleus.Apps.Shared.macOS.Renderers.PointerFrameRenderer))]

namespace Heleus.Apps.Shared.macOS.Renderers
{
	public class PointerFrameRenderer : VisualElementRenderer<PointerFrame>
    {
        readonly PointerEventHandlerMacOS touchEvent = new PointerEventHandlerMacOS();
		NSTrackingArea trackingArea;

		public PointerFrameRenderer()
		{
			//AutoPackage = false;
			AutoTrack = false;
		}

		protected override void Dispose(bool disposing)
		{
			if(touchEvent != null)
			{
				touchEvent.Receiver = null;
			}

			try
			{
				if (trackingArea != null)
				{
					RemoveTrackingArea(trackingArea);
                    trackingArea.Dispose();
					trackingArea = null;
				}
			} catch {}
			
			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<PointerFrame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				touchEvent.Receiver = (e.NewElement as IPointerEventReceiver);
				Layer.CornerRadius = 6;
			}
		}

        public override void UpdateTrackingAreas()
        {
            if (trackingArea != null)
            {
                RemoveTrackingArea(trackingArea);
                trackingArea.Dispose();
            }
            
            var options = (NSTrackingAreaOptions.ActiveAlways | NSTrackingAreaOptions.InVisibleRect | NSTrackingAreaOptions.MouseEnteredAndExited | NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.CursorUpdate);
			trackingArea = new NSTrackingArea(Bounds, options, this, null);
            AddTrackingArea(trackingArea);

			var mouseLocation = Window.MouseLocationOutsideOfEventStream;
			mouseLocation = ConvertPointFromView(mouseLocation, null);

			if(Bounds.Contains(mouseLocation))
			{
				touchEvent.MouseEntered(null);
			}
			else
			{
				//if (Window.FirstResponder == this)
					//Window.MakeFirstResponder(null);
				touchEvent.MouseExited(null);
			}

			Window.InvalidateCursorRectsForView(this);
			base.UpdateTrackingAreas();
        }

		public override void ResetCursorRects()
		{
			base.ResetCursorRects();
			//AddCursorRect(Frame, NSCursor.PointingHandCursor);
		}

		public override bool IsFlipped
		{
			get
			{
				return true;
			}
		}

        public override void MouseEntered(NSEvent theEvent)
        {
            touchEvent.MouseEntered(theEvent);
			base.MouseEntered(theEvent);
        }

        public override void MouseExited(NSEvent theEvent)
        {
			if (Window.FirstResponder == this)
				Window.MakeFirstResponder(null);
            touchEvent.MouseExited(theEvent);
			base.MouseExited(theEvent);
		}

        public override void MouseUp(NSEvent theEvent)
        {
			var localLocation = ConvertPointFromView(theEvent.LocationInWindow, null);
            
			touchEvent.MouseUp(theEvent, localLocation);
            base.MouseUp(theEvent);
        }

        public override void MouseDown(NSEvent theEvent)
        {
			var localLocation = ConvertPointFromView(theEvent.LocationInWindow, null);

			touchEvent.MouseDown(theEvent, localLocation);
			if (Window.FirstResponder != this)
				Window.MakeFirstResponder(null);
			
			base.MouseDown(theEvent);
        }

		public override void MouseDragged(NSEvent theEvent)
		{
			var localLocation = ConvertPointFromView(theEvent.LocationInWindow, null);
			touchEvent.MouseMoved(theEvent, localLocation);

			base.MouseDragged(theEvent);
		}

        public override void MouseMoved(NSEvent theEvent)
        {
			var localLocation = ConvertPointFromView(theEvent.LocationInWindow, null);
			touchEvent.MouseMoved(theEvent, localLocation);

            base.MouseMoved(theEvent);
        }

		/*
		public override bool BecomeFirstResponder()
		{
			touchEvent.MouseEntered(null);
			return true;
		}

		public override bool ResignFirstResponder()
		{
			touchEvent.MouseExited(null);
			return true;
		}

		public override void KeyDown(NSEvent theEvent)
		{
			try {
				var c = theEvent.CharactersIgnoringModifiers[0];
				if (c == 13 || c == 3 || c == 32)
				{
					touchEvent.SubmitKey();
					return;
				}
			} catch {}

			base.KeyDown(theEvent);
		}

		public override bool CanBecomeKeyView
		{
			get
			{
				return true;
			}
		}

		public override bool AcceptsFirstResponder()
		{
			return true;
		}
		*/
	}
}
