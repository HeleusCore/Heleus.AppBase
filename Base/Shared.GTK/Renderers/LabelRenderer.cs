using System;
using System.ComponentModel;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Renderers;

[assembly: ExportRenderer(typeof(ExtLabel), typeof(Heleus.Apps.Shared.GTK.Renderers.ExtLabelRenderer))]

namespace Heleus.Apps.Shared.GTK.Renderers
{
	public class ExtLabelRenderer : LabelRenderer
	{
		public new ExtLabel Element => (ExtLabel)base.Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);

			if(e.NewElement != null)
			{
				UpdateText();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.FontAttributesProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.FormattedTextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == ExtLabel.FontWeightProperty.PropertyName)
				UpdateText();
			else
				base.OnElementPropertyChanged(sender, e);
		}

		private void UpdateText()
		{
			var markupText = string.Empty;
			var formatted = Element.FormattedText;

			if (formatted != null)
			{
				Control.SetTextFromFormatted(formatted);
			}
			else
			{
				var span = new Span()
				{
					FontAttributes = Element.FontAttributes,
					FontFamily = Element.FontFamily,
					FontSize = Element.FontSize,
					Text = GLib.Markup.EscapeText(Element.Text ?? string.Empty)
				}.FontWeight(Element.FontWeight);

				Control.SetTextFromSpan(span);
			}
		}
	}
}
