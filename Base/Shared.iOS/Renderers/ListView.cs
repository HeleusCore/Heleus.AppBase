using System;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RowListView), typeof(Heleus.Apps.Shared.iOS.Renderers.ListViewRenderer))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
    public class ListViewRenderer : Xamarin.Forms.Platform.iOS.ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.AllowsSelection = false;
            }
        }
    }
}
