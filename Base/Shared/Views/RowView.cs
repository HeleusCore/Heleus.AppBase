using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class RowView : StackLayout
    {
        readonly string _translationPrefix;

        protected (ExtLabel, ExtLabel) AddRow(string titleText, string valueText)
        {
            return AddRow(titleText, valueText, true, false);
        }

        protected (ExtLabel, ExtLabel) AddLastRow(string titleText, string valueText)
        {
            return AddRow(titleText, valueText, true, true);
        }

        protected (ExtLabel, ExtLabel) AddRow(string titleText, string valueText, bool titleFirst)
        {
            return AddRow(titleText, valueText, titleFirst, false);
        }

        protected (ExtLabel, ExtLabel) AddLastRow(string titleText, string valueText, bool titleFirst)
        {
            return AddRow(titleText, valueText, titleFirst, true);
        }

        protected (ExtLabel, ExtLabel) AddRow(string titleText, string valueText, bool titleFirst, bool lastRow)
        {
            var title = new ExtLabel { Text = Tr.Get(_translationPrefix + titleText), FontStyle = Theme.DetailFont, ColorStyle = Theme.TextColor, Margin = new Thickness(0, 0, 0, (lastRow && !titleFirst) ? 0 : !titleFirst ? 10 : 0), InputTransparent = true };
            var value = new ExtLabel { Text = valueText, FontStyle = Theme.RowFont, ColorStyle = Theme.TextColor, Margin = new Thickness(0, 0, 0, (lastRow && titleFirst) ? 0 : titleFirst ? 10 : 0), InputTransparent = true };

            if (titleFirst)
            {
                Children.Add(title);
                Children.Add(value);
            }
            else
            {
                Children.Add(value);
                Children.Add(title);
            }

            return (title, value);
        }

        protected RowView(string translationPrefix)
        {
            _translationPrefix = translationPrefix + ".";
            //CompressedLayout.SetIsHeadless(this, true);
            InputTransparent = true;
            Spacing = 0;
            //Margin = new Thickness(15, 15, 15, 5);
            SizeChanged += ViewSizeChanged;
        }

        void ViewSizeChanged(object sender, EventArgs e)
        {
            var w = (int)Width;
            if (Width <= 0)
                return;

            foreach (var child in Children)
            {
                if(child is ExtLabel label)
                {
                    if (w != (int)label.WidthRequest)
                        label.WidthRequest = w;
                }
            }
        }
    }
}
