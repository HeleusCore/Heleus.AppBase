using System;
using AppKit;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ExtBoxView), typeof(Heleus.Apps.Shared.macOS.Renderers.ExtBoxViewRenderer))]

namespace Heleus.Apps.Shared.macOS.Renderers
{
	public class ExtBoxViewRenderer : VisualElementRenderer<ExtBoxView>
	{
		public ExtBoxViewRenderer()
		{
			//AutoPackage = false;
			AutoTrack = false;
		}
	}
}
