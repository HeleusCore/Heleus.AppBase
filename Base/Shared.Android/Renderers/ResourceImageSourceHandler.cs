using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.Widget;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportImageSourceHandler(typeof(ResourceImageSource), typeof(Heleus.Apps.Shared.Android.Renderers.ResourceImageSourceHandler))]

namespace Heleus.Apps.Shared.Android.Renderers
{
    public sealed class ResourceImageSourceHandler : IImageSourceHandler, IImageViewHandler
    {
        public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
        {
            string file = ((ResourceImageSource)imagesource).File;
            Bitmap bitmap;
            if (File.Exists(file))
                bitmap = await BitmapFactory.DecodeFileAsync(file).ConfigureAwait(false);
            else
                bitmap = await context.Resources.GetBitmapAsync(file).ConfigureAwait(false);

            return bitmap;
        }

        public Task LoadImageAsync(ImageSource imagesource, ImageView imageView, CancellationToken cancellationToken = default(CancellationToken))
        {
            string file = ((ResourceImageSource)imagesource).File;
            if (File.Exists(file))
            {
                var uri = Uri.Parse(file);
                if (uri != null)
                    imageView.SetImageURI(uri);
            }
            else
            {
                var drawable = ResourceManager.GetDrawable(imageView.Context, file);
                if (drawable != null)
                    imageView.SetImageDrawable(drawable);
            }

            return Task.FromResult(true);
        }
    }
}

