using System;
using System.ComponentModel;
using AppKit;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ExtScrollView), typeof(Heleus.Apps.Shared.macOS.Renderers.ExtendedScrollViewRenderer))]

namespace Heleus.Apps.Shared.macOS.Renderers
{
	public class ExtendedScrollViewRenderer : ScrollViewRenderer
	{
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(trackingArea != null)
				{
					RemoveTrackingArea(trackingArea);
					trackingArea = null;
				}
			}
			base.Dispose(disposing);
		}
		
		NSTrackingArea trackingArea;

		public override void MouseDown(NSEvent theEvent)
		{
			base.MouseDown(theEvent);
			//Window.MakeFirstResponder(null);
		}

		public override void UpdateTrackingAreas()
		{
			if (trackingArea != null)
				RemoveTrackingArea(trackingArea);

			var options = (NSTrackingAreaOptions.ActiveAlways | NSTrackingAreaOptions.InVisibleRect | NSTrackingAreaOptions.MouseEnteredAndExited | NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.CursorUpdate);
			trackingArea = new NSTrackingArea(Bounds, options, this, null);
			AddTrackingArea(trackingArea);

			var mouseLocation = Window.MouseLocationOutsideOfEventStream;
			mouseLocation = ConvertPointFromView(mouseLocation, this);

			base.UpdateTrackingAreas();
		}
	}
}
