using AppKit;
using CoreGraphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Heleus.Apps.Shared.macOS.Renderers
{
    internal static class CellNSView
    {
        static readonly CGColor s_defaultHeaderViewsBackground = NSColor.LightGray.CGColor;

        internal static NSView GetNativeCell(NSTableView tableView, Cell cell, string templateId = "", bool isHeader = false,
            bool isRecycle = false)
        {
            var reusable = tableView.MakeView(templateId, tableView);
            NSView nativeCell;
            if (reusable == null || !isRecycle)
            {
                var renderer = (CellRenderer)Xamarin.Forms.Internals.Registrar.Registered.GetHandlerForObject<IRegisterable>(cell);
                nativeCell = renderer.GetCell(cell, null, tableView);
            }
            else
            {
                nativeCell = reusable;
            }

            //if (string.IsNullOrEmpty(nativeCell.Identifier))
                nativeCell.Identifier = templateId;

            if (!isHeader) return nativeCell;
            if (nativeCell.Layer != null) nativeCell.Layer.BackgroundColor = s_defaultHeaderViewsBackground;
            return nativeCell;
        }
    }
}