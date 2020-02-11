using Gdk;
using Heleus.Apps.Shared;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Renderers;

[assembly: ExportImageSourceHandler(typeof(ResourceImageSource), typeof(Heleus.Apps.Shared.GTK.Renderers.ResourceImageSourceHandler))]

namespace Heleus.Apps.Shared.GTK.Renderers
{
	public sealed class ResourceImageSourceHandler : IImageSourceHandler
	{
		public Task<Pixbuf> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			Pixbuf image = null;

            if (imagesource is ResourceImageSource filesource)
            {
                var file = filesource.File;
                if (!string.IsNullOrEmpty(file))
                {
                    var imagePath = Path.Combine(GtkApp.ResourceDirectory.FullName, file);
                    if (File.Exists(imagePath))
                    {
                        var data = File.ReadAllBytes(imagePath);
                        image = new Pixbuf(data);
                    }
                }
            }

            return Task.FromResult(image);
		}
	}
}
