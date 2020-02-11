using Heleus.Apps.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportImageSourceHandler(typeof(ResourceImageSource), typeof(Heleus.Apps.Shared.UWP.Renderers.ResourceImageSourceHandler))]

namespace Heleus.Apps.Shared.UWP.Renderers
{
    public sealed class ResourceImageSourceHandler : IImageSourceHandler, IRegisterable
    {
        public Task<Windows.UI.Xaml.Media.ImageSource> LoadImageAsync(Xamarin.Forms.ImageSource imagesoure, CancellationToken cancelationToken = default(CancellationToken))
        {
            Windows.UI.Xaml.Media.ImageSource result = null;
            try
            {
                var fileImageSource = imagesoure as ResourceImageSource;
                if (fileImageSource != null)
                {
                    string file = fileImageSource.File;
                    if(file.StartsWith("ms-"))
                        result = new BitmapImage(new Uri(file));
                    else
                        result = new BitmapImage(new Uri("ms-appx:///Assets/" + file));
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            return Task.FromResult<Windows.UI.Xaml.Media.ImageSource>(result);
        }
    }
}
