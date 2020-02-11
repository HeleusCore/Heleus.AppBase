using System;
using AppKit;
using Xamarin.Forms;

namespace Heleus.Apps.Shared.Apple.Renderers
{
    internal static class AlignmentExtensions
    {
        internal static NSTextAlignment ToNativeTextAlignment(this TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.Center:
                    return NSTextAlignment.Center;
                case TextAlignment.End:
                    return NSTextAlignment.Right;
                default:
                    return NSTextAlignment.Left;
            }
        }
    }
}