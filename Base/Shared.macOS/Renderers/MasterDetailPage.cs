using System;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ExtMasterDetailPage), typeof(Heleus.Apps.Shared.macOS.Renderers.ExtMasterDetailPageRenderer))]

namespace Heleus.Apps.Shared.macOS.Renderers
{
	public class ExtMasterDetailPageRenderer : MasterDetailPageRenderer
	{
		protected override double MasterWidthPercentage
		{
			get
			{
				var width = View.Frame.Width;
				if (width == -1)
					return 0.3;


				return 250 / width;
			}
		}
	}
}
