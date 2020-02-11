using System;
using System.Reflection;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public static class SpanExtension
	{
		public static readonly BindableProperty FontStyleProperty = BindableProperty.Create("FontStyle", typeof(FontStyle), typeof(Span), null);
		public static readonly BindableProperty ColorStylePoperty = BindableProperty.Create ("ColorStyle", typeof (ColorStyle), typeof (Span), null);

		public static Span FontWeight(this Span span, FontWeight fontWeight)
		{
			span.SetValue(ExtLabel.FontWeightProperty, fontWeight);
			return span;
		}

		public static FontWeight FontWeight(this Span span)
		{
			return (FontWeight)span.GetValue(ExtLabel.FontWeightProperty);
		}

		public static Span SetFontStyle(this Span span, FontStyle fontStyle)
		{
			span.SetValue(FontStyleProperty, fontStyle);

			if (fontStyle != null)
			{
				span.FontWeight(fontStyle.FontWeight);
				span.FontFamily = FontExtension.GetFontFamily(span.FontFamily, fontStyle.FontWeight);
				span.FontSize = fontStyle.FontSize;
			}

			return span;
		}

		public static FontStyle GetFontStyle(this Span span)
		{
			return (FontStyle)span.GetValue(FontStyleProperty);
		}

		public static Span SetColorStyle(this Span span, ColorStyle colorStyle)
		{
			span.SetValue(ColorStylePoperty, colorStyle);

			if (colorStyle != null)
			{
				span.ForegroundColor = colorStyle.Color;
			}

			return span;
		}

		public static ColorStyle GetColorStyle(this Span span)
		{
			return (ColorStyle)span.GetValue(ColorStylePoperty);
		}

		public static Span SetStyle(this Span span, FontStyle fontStyle, ColorStyle colorStyle)
		{
			span.SetColorStyle (colorStyle);
			span.SetFontStyle (fontStyle);
			return span;
		}
	}
}
