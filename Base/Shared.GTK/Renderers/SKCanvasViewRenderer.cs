#if !GTK

using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Gtk.SKWidget;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(Heleus.Apps.Shared.GTK.Renderers.SKCanvasViewRenderer))]

namespace Heleus.Apps.Shared.GTK.Renderers
{
    public class SKCanvasViewRenderer: SKCanvasViewRendererBase<SKFormsView, SKNativeView>
    {
        
    }
}

#endif