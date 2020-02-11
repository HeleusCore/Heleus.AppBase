using System;
using Heleus.Apps.Shared;
using Heleus.Apps.Shared.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtPicker), typeof(ExtendedPickerRenderer))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
	class ExtendedPickerRenderer : PickerRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				if (!(Element is ExtPicker picker))
					return;

				picker.ThemeChangedAction = ThemeUpdated;
				Control.Font = Apple.Renderers.FontExtensions.ToUIFont(picker.FontFamily, (float)picker.FontSize, FontAttributes.None);
				Control.BorderStyle = UITextBorderStyle.None;
				ThemeUpdated();
			}
		}

		void ThemeUpdated()
		{
			if (Control != null)
			{
			}
		}
	}
}
