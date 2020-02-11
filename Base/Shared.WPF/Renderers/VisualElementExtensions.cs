using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace Heleus.Apps.Shared.WPF.Renderers
{
    public static class VisualElementExtensions
    {
        internal static void Cleanup(this VisualElement self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            IVisualElementRenderer renderer =  Platform.GetRenderer(self);

            foreach (Element element in self.Descendants())
            {
                var visual = element as VisualElement;
                if (visual == null)
                    continue;

                IVisualElementRenderer childRenderer = Platform.GetRenderer(visual);
                if (childRenderer != null)
                {
                    childRenderer.Dispose();
                    Platform.SetRenderer(visual, null);
                }
            }

            if (renderer != null)
            {
                renderer.Dispose();
                Platform.SetRenderer(self, null);
            }
        }
    }
}
