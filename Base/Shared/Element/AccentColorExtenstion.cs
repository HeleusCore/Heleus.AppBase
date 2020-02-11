using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public static class AccentColorExtenstion
    {
        public const int DefaultAccentColorWith = 5;

        public sealed class AccentColor : BoxView
        {

        }

        public static void SetAccentColor(this AbsoluteLayout layout, Color accentColor, int width = DefaultAccentColorWith)
        {
            for (var i = layout.Children.Count - 1; i >= 0; i--)
            {
                if (layout.Children[i] is AccentColor c)
                {
                    c.Color = c.BackgroundColor = accentColor;
                    return;
                }
            }

            var color = new AccentColor { Color = accentColor, BackgroundColor = accentColor };
            AbsoluteLayout.SetLayoutFlags(color, AbsoluteLayoutFlags.HeightProportional);
            AbsoluteLayout.SetLayoutBounds(color, new Rectangle(0, 0, width, 1));

            layout.Children.Add(color);
        }

        public static void RemoveAccentColor(this AbsoluteLayout layout)
        {
            for (var i = layout.Children.Count - 1; i >= 0; i--)
            {
                if (layout.Children[i] is AccentColor)
                {
                    layout.Children.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
