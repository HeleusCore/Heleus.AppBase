using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Heleus.Apps.Shared.UWP.Renderers
{
    internal static class ColorExtension
    {
        public static Windows.UI.Xaml.Media.Brush ToBrush(this Color color)
        {
            return new Windows.UI.Xaml.Media.SolidColorBrush(color.ToWindowsColor());
        }

        public static Windows.UI.Color ToWindowsColor(this Color color)
        {
            return Windows.UI.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
        }
    }
}
