using System;
using Xamarin.Forms;
using Foundation;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.iOS;
using System.Threading;
using UIKit;
using System.IO;
using Heleus.Apps.Shared;

[assembly: ExportImageSourceHandler(typeof(ResourceImageSource), typeof(Heleus.Apps.Shared.iOS.Renderers.ResourceImageSourceHandler))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
	public sealed class ResourceImageSourceHandler : IImageSourceHandler, IRegisterable
	{
		public Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			UIImage uIImage = null;
			if (imagesource is ResourceImageSource fileImageSource)
			{
				string file = fileImageSource.File;
				if (!string.IsNullOrEmpty(file))
				{
                    NSData data;
					if (File.Exists(file))
					{
						data = NSData.FromFile(file);
					}
					else
					{
						string path = NSBundle.MainBundle.PathForResource(file, Path.GetExtension(file));
						data = NSData.FromFile(path);
					}

					uIImage = UIImage.LoadFromData(data);
				}
			}
			return Task.FromResult(uIImage);
		}
	}
}

