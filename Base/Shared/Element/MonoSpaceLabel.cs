using Heleus.Apps.Shared;

namespace Heleus.Apps.Shared
{
    public class MonoSpaceLabel : Xamarin.Forms.Label
    {
        public MonoSpaceLabel()
        {
            if (UIApp.IsAndroid)
                FontFamily = "Droid Sans Mono";
            else
                FontFamily = "Courier";

            TextColor = Theme.TextColor.Color;
        }
    }
}
