using System;
using Android.Content;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FontIcon), typeof(Heleus.Apps.Shared.Android.Renderers.FontIconRenderer))]

namespace Heleus.Apps.Shared.Android.Renderers
{
	public class FontIconRenderer : LabelRenderer
	{
		public FontIconRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Label.TextColorProperty.PropertyName)
			{
				UpdateFont();
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);

			UpdateFont();
		}

		void UpdateFont()
		{
			if (Control != null)
				Control.Typeface = TypefaceCache.CacheTypeFace(Element.FontFamily);
		}
	}
}
