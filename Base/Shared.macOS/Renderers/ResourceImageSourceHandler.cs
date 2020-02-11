using System;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Heleus.Apps.Shared;
using Heleus.Base;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportImageSourceHandler(typeof(ResourceImageSource), typeof(Heleus.Apps.Shared.macOS.Renderers.ResourceImageSourceHandler))]

namespace Heleus.Apps.Shared.macOS.Renderers
{
    public sealed class ResourceImageSourceHandler : IImageSourceHandler, IRegisterable
	{
		public Task<NSImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			NSImage image = null;
			try 
			{
				var fileImageSource = (imagesource as ResourceImageSource);
				var file = fileImageSource.File.Replace(".png", "").Replace(".jpg", "");
                image = NSImage.ImageNamed(file);
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}

			return Task.FromResult(image);
		}
	}
}
