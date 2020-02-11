using System;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;

//[assembly: ExportRenderer(typeof(ExtMasterDetailPage), typeof(shared.forms.renderers.ExtMasterDetailPageRenderer))]

namespace shared.forms.renderers
{
	public class ExtMasterDetailPageRenderer : MasterDetailRenderer
	{
		public ExtMasterDetailPageRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(Xamarin.Forms.VisualElement oldElement, Xamarin.Forms.VisualElement newElement)
		{
			SetBackgroundColor(Android.Graphics.Color.Transparent);
			SetFitsSystemWindows(true);

			base.OnElementChanged(oldElement, newElement);
		}
	}
}
