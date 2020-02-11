using Xamarin.Forms.Platform.WPF;
using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.WPF.SKElement;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(Heleus.Apps.Base.WPF.Renderers.SKCanvasViewRenderer))]

namespace Heleus.Apps.Base.WPF.Renderers
{
    public class SKCanvasViewRenderer: SKCanvasViewRendererBase<SKFormsView, SKNativeView>
    {
        
    }
}