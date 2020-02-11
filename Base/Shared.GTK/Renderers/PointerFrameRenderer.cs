using System.ComponentModel;
using Heleus.Apps.Shared;
using Heleus.Apps.Shared.GTK.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Extensions;

[assembly: ExportRenderer(typeof(PointerFrame), typeof(PointerFrameRenderer))]

namespace Heleus.Apps.Shared.GTK.Renderers
{
	public class PointerFrameRenderer : ViewRenderer<PointerFrame, PointerFrameControl>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<PointerFrame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new PointerFrameControl(e.NewElement as IPointerEventReceiver));
				}

				PackChild();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName.Equals("Content", System.StringComparison.InvariantCultureIgnoreCase))
			{
				PackChild();
			}
		}

		private void PackChild()
		{
			if (Element.Content == null)
				return;

			var renderer = Element.Content.GetOrCreateRenderer();
			Control.Child = renderer.Container;
			renderer.Container.ShowAll();
		}
	}
}
