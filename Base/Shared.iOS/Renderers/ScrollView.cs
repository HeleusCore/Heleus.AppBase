using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using Heleus.Apps.Shared;

[assembly: ExportRenderer(typeof(ExtScrollView), typeof(Heleus.Apps.Shared.iOS.Renderers.ExtendedScrollViewRenderer))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
	class ExtendedScrollViewRenderer : ScrollViewRenderer
	{
		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;

			if (Element is ExtScrollView scrollView && scrollView.DisableTouchCanel)
			{
				PanGestureRecognizer.CancelsTouchesInView = false;
				CanCancelContentTouches = false;
			}
		}
	}
}