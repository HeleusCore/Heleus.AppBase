using System;
using Android.Content;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtActivityIndicator), typeof(Heleus.Apps.Shared.Android.Renderers.ExtendedActivityIndicatorRenderer))]

namespace Heleus.Apps.Shared.Android.Renderers
{
	public class ExtendedActivityIndicatorRenderer : ActivityIndicatorRenderer
	{
		public ExtendedActivityIndicatorRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			var bar = Control;
			if (bar == null)
			{
				var progressBar = new global::Android.Widget.ProgressBar(Context, null, global::Android.Resource.Attribute.ProgressBarStyleHorizontal) { Indeterminate = true };
				SetNativeControl(progressBar);
			}

			base.OnElementChanged(e);
		}
	}
}
