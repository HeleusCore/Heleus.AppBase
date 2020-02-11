using System;
using Android.Content;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtPicker), typeof(Heleus.Apps.Shared.Android.Renderers.ExtendedPickerRenderer))]

namespace Heleus.Apps.Shared.Android.Renderers
{
	public class ExtendedPickerRenderer : PickerRenderer
	{
		public ExtendedPickerRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			base.OnElementChanged(e);
			if (Control != null)
			{
				var picker = Element as ExtPicker;
				if (picker == null)
					return;

				Control.InputType |= global::Android.Text.InputTypes.TextFlagNoSuggestions;
				Control.TextSize = (float)picker.FontSize;
				Control.Typeface = TypefaceCache.CacheTypeFace(picker.FontFamily);

				Control.SetTextColor(picker.TextColor.ToAndroid());

				try
				{
					Control.Background = null;
					Control.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
				}
				catch
				{
				}
			}
		}
	}
}
