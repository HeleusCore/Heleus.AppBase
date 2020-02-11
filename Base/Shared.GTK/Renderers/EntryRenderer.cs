using System;
using System.ComponentModel;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Renderers;

[assembly: ExportRenderer(typeof(ExtEntry), typeof(Heleus.Apps.Shared.GTK.Renderers.ExtEntryRenderer))]

namespace Heleus.Apps.Shared.GTK.Renderers
{
	public class ExtEntryRenderer : EntryRenderer
	{
		new ExtEntry Element => (ExtEntry)base.Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else
				base.OnElementPropertyChanged(sender, e);
		}

		void UpdateFont()
		{
			var fontDescription = FontDescriptionHelper.CreateFontDescription(
				Element.FontSize, Element.FontFamily, Element.FontAttributes, Element.FontWeight);
			Control.SetFont(fontDescription);
		}
	}
}
