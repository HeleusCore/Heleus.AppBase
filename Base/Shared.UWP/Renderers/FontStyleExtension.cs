using System;
using System.Collections.Generic;
using System.Text;

namespace Heleus.Apps.Shared.UWP.Renderers
{
    static class FontStyleExtension
    {
        public static Windows.UI.Text.FontWeight ToWindowsFontWeight(this FontWeight weight)
        {
            if (weight == FontWeight.Thin)
                return Windows.UI.Text.FontWeights.Thin;
            if (weight == FontWeight.Light)
                return Windows.UI.Text.FontWeights.Light;
            if (weight == FontWeight.Regular)
                return Windows.UI.Text.FontWeights.Normal;
            if (weight == FontWeight.Medium)
                return Windows.UI.Text.FontWeights.Medium;
            if (weight == FontWeight.Bold)
                return Windows.UI.Text.FontWeights.Bold;

            return Windows.UI.Text.FontWeights.Normal;
        }
    }
}
