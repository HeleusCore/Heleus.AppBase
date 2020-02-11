using System;
using System.Collections.Generic;
using System.Text;

namespace Heleus.Apps.Shared.WPF.Renderers
{
    static class FontStyleExtension
    {
        public static System.Windows.FontWeight ToWindowsFontWeight(FontWeight weight)
        {

            if (weight == FontWeight.Thin)
                return System.Windows.FontWeights.Thin;
            if (weight == FontWeight.Light)
                return System.Windows.FontWeights.Light;
            if (weight == FontWeight.Regular)
                return System.Windows.FontWeights.Normal;
            if (weight == FontWeight.Medium)
                return System.Windows.FontWeights.Medium;
            if (weight == FontWeight.Bold)
                return System.Windows.FontWeights.Bold;

            return System.Windows.FontWeights.Normal;
        }
    }
}
