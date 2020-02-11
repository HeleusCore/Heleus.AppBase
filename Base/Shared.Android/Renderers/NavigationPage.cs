using System;
using Android.Content;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(ExtNavigationPage), typeof(Heleus.Apps.Shared.Android.Renderers.ExtNavigationPageRenderer))]

namespace Heleus.Apps.Shared.Android.Renderers
{
	public class ExtNavigationPageRenderer : NavigationPageRenderer
	{
		public ExtNavigationPageRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Xamarin.Forms.NavigationPage> e)
		{
			SetBackgroundColor(global::Android.Graphics.Color.Transparent);
			SetFitsSystemWindows(true);

			base.OnElementChanged(e);
		}
	}
}
