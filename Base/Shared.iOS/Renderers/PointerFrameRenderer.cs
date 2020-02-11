using System;
using Foundation;
using Heleus.Apps.Shared;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(PointerFrame), typeof(Heleus.Apps.Shared.iOS.Renderers.PointerFrameRenderer))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
	public class PointerFrameRenderer : VisualElementRenderer<PointerFrame>
	{
		TouchEventHandlerIOS touchEvent = new TouchEventHandlerIOS();

		protected override void OnElementChanged(ElementChangedEventArgs<PointerFrame> e)
		{
			base.OnElementChanged(e);

			try
			{
				if (e.NewElement != null)
				{
					touchEvent.Receiver = (e.NewElement as IPointerEventReceiver);
					Layer.CornerRadius = 6;
				}
			}
			catch(Exception ex)
			{
				Console.Write(ex);
			}
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			touchEvent.TouchesBegan(touches, evt, this);
			base.TouchesBegan(touches, evt);
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			touchEvent.TouchesMoved(touches, evt, this);
			base.TouchesMoved(touches, evt);
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			touchEvent.TouchesCancelled(touches, evt);
			base.TouchesCancelled(touches, evt);
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			touchEvent.TouchesEnded(touches, evt);
			base.TouchesEnded(touches, evt);
		}
	}

}
