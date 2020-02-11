using System;
using Heleus.Base;
using Xamarin.Forms;
#if !GTK && !CLI
using SkiaSharp.Views.Forms;
#endif

namespace Heleus.Apps.Shared
{
	static class PageExtension
	{
#if !GTK && !CLI
		//void Background_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
		static void Background_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			try
			{
				//var target = e.RenderTarget;
				var target = e.Info;
				var canvas = e.Surface.Canvas;

				UIApp.UpdateBackgroundCanvas(canvas, target.Width, target.Height);
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}
		}

		class ThemedSKCanvasView : SKCanvasView, IThemeable
		{
			public void ThemeChanged()
			{
				InvalidateSurface();
			}
		}
#endif

		public static void EnableSkiaBackground(this ExtContentPage page)
		{
#if !GTK && !CLI
			if (UIApp.IsAndroid)
				return;

			var background = new ThemedSKCanvasView();

			AbsoluteLayout.SetLayoutFlags(background, AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.HeightProportional);
			AbsoluteLayout.SetLayoutBounds(background, new Rectangle(0, 0, 1, 1));

			background.PaintSurface += Background_PaintSurface;
			background.InputTransparent = true;

			page.RootLayout.Children.Insert(0, background);
#endif
		}

		public static ExtImage AddLogo(this StackPage page, int size = 300)
		{
			var image = new ExtImage
			{
				Source = new ResourceImageSource("logo.png"),
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = size,
				HeightRequest = size / 3,
				Margin = new Thickness(0, 0, 0, 0)
			};

			page.AddView(image);
			return image;
		}
	}
}
