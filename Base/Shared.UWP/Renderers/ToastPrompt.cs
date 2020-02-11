using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Text;

namespace Heleus.Apps.Shared.UWP.Renderers
{
    class ToastPrompt : Coding4Fun.Toolkit.Controls.ToastPrompt
    {
        public ToastPrompt()
        {
            TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Height = MaxHeight = MinHeight = 80;
            MillisecondsUntilHidden = 2000;

            Background = Theme.SecondaryColor.Color.ToBrush();
            Foreground = Xamarin.Forms.Color.White.ToBrush();

            Tapped += ToastPrompt_Tapped;
        }

        private void ToastPrompt_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Hide();
            Tapped -= ToastPrompt_Tapped;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var panel = this.FindFirstControl<Windows.UI.Xaml.Controls.StackPanel>();
            panel.Margin = new Windows.UI.Xaml.Thickness(10, 0, 0, 0);

            try
            {
                var textBlock = panel.Children[1] as Windows.UI.Xaml.Controls.TextBlock;
                if (textBlock != null)
                    textBlock.FontWeight = FontWeights.Bold;
            }
            catch
            {
            }
        }
    }
}
