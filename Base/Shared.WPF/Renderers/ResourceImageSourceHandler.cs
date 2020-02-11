using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportImageSourceHandler(typeof(ResourceImageSource), typeof(Heleus.Apps.Shared.WPF.Renderers.ResourceImageSourceHandler))]

namespace Heleus.Apps.Shared.WPF.Renderers
{
    public sealed class ResourceImageSourceHandler : IImageSourceHandler
    {
        public Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesoure, CancellationToken cancelationToken = new CancellationToken())
        {
            System.Windows.Media.ImageSource image = null;
            var filesource = imagesoure as ResourceImageSource;
            if (filesource != null)
            {
                string file = "Resources\\" + filesource.File;
                image = new BitmapImage(new Uri(file, UriKind.RelativeOrAbsolute));
            }
            return Task.FromResult(image);
        }
    }
}
