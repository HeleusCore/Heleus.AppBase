using System;
using System.IO;
using System.Threading.Tasks;
using Heleus.Base;
using SkiaSharp;



namespace Heleus.Apps.Shared
{
    public enum ImageHandlerCropMode
    {
        Fit,
        Fill
    }

    public class ImageHandler : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsOpaque { get; private set; }
        public double Aspect =>  Width / (double) Height;

        readonly SKBitmap _bitmap;

        public ImageHandler(int width, int height, bool isOpaque) : this(new SKBitmap(width, height, isOpaque))
        {
            if(!isOpaque)
            {
                using (var canvas = new SKCanvas(_bitmap))
                {
                    canvas.Clear(SKColors.Transparent);
                }
            }
        }

        public ImageHandler(ImageHandler imageHandler) : this(imageHandler._bitmap.Copy())
        {
        }

        public ImageHandler(SKBitmap bitmap)
        {
            _bitmap = bitmap;

            Width = bitmap.Width;
            Height = bitmap.Height;
            IsOpaque = bitmap.AlphaType == SKAlphaType.Opaque || bitmap.AlphaType == SKAlphaType.Unknown;
        }

        public static Task<ImageHandler> FromFile(string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        var data = File.ReadAllBytes(filePath);
                        return FromData(data);
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex); 
                }
                return null;
            });
        }

        public static Task<ImageHandler> FromData(byte[] data)
        {
            return Task.Run(() =>
            {
                try
                {
                    var bitmap = SKBitmap.Decode(data);
                    if (bitmap == null)
                        return null;
                    return new ImageHandler(bitmap);
                }
                catch(Exception ex)
                {
                    Log.IgnoreException(ex);
                }
                return null;
            });
        }

        public static Task<ImageHandler> FromStream(Stream stream)
        {
            return Task.Run(() =>
            {
                try
                {
                    var bitmap = SKBitmap.Decode(stream);
                    if (bitmap == null)
                        return null;
                    return new ImageHandler(bitmap);
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
                return null;
            });
        }

        public Task<ImageHandler> Resize(int maxAxisSize)
        {
            return Task.Run(() =>
            {
                var w = Width;
                var h = Height;

                if (Aspect >= 1.0 && Width > maxAxisSize)
                {
                    var scale = maxAxisSize / (double)Width;

                    w = maxAxisSize;
                    h = Math.Min((int)Math.Ceiling(Height * scale), maxAxisSize);
                }

                else if (Aspect < 1.0 && Height > maxAxisSize)
                {
                    var scale = maxAxisSize / (double)Height;

                    w = Math.Min((int)Math.Ceiling(Width * scale), maxAxisSize);
                    h = maxAxisSize;
                }

                return new ImageHandler(_bitmap.Resize(new SKImageInfo(w, h, _bitmap.ColorType, _bitmap.AlphaType), SKFilterQuality.High));
            });
        }

        public async Task<ImageHandler> Crop(int imageWidth, int imageHeight, ImageHandlerCropMode cropMode)
        {
            int x = 0, y = 0;

            var aspect = Aspect;
            var scale = 1.0;

            if (cropMode == ImageHandlerCropMode.Fit && aspect >= 1 || cropMode == ImageHandlerCropMode.Fill && aspect < 1)
            {
                scale = imageWidth / (double)Width;
                y = imageHeight / 2 - (int)(Height * scale) / 2;
            }
            else
            {
                scale = imageHeight / (double)Height;
                x = imageWidth / 2 - (int)(Width * scale) / 2;
            }

            var croppedImage = new ImageHandler(imageWidth, imageHeight, IsOpaque);
            await croppedImage.Blend(this, x, y, (int)(Width * scale), (int)(Height * scale));

            return croppedImage;
        }

        public Task Blend(ImageHandler imageHandler, int x, int y)
        {
            return Task.Run(() =>
            {
                using (var canvas = new SKCanvas(_bitmap))
                {
                    canvas.DrawBitmap(imageHandler._bitmap, new SKPoint(x, y));
                }
            });
        }

        public Task Blend(ImageHandler imageHandler, int x, int y, int width, int height)
        {
            return Task.Run(() =>
            {
                using (var canvas = new SKCanvas(_bitmap))
                {
                    canvas.DrawBitmap(imageHandler._bitmap, new SKRect(x, y, width, height));
                }
            });
        }

        public Task Blend(ImageHandler imageHandler, int x, int y, int width, int height, int sourceX, int sourceY, int sourceWith, int sourceHeight)
        {
            return Task.Run(() =>
            {
                using (var canvas = new SKCanvas(_bitmap))
                {
                    canvas.DrawBitmap(imageHandler._bitmap, new SKRect(sourceX, sourceY, sourceWith, sourceHeight), new SKRect(x, y, width, height));
                }
            });
        }

        public async Task<ImageHandler> CopySection(int x, int y, int width, int height)
        {
            var img = new ImageHandler(width, height, IsOpaque);

            await img.Blend(this, 0, 0, width, height, x, y, x + width, y + height);

            return img;
        }

        public Task<bool> SavePNG(string filePath)
        {
            return Save(filePath, SKEncodedImageFormat.Png, 100);
        }

        public Task<byte[]> SavePNG()
        {
            return Save(SKEncodedImageFormat.Png, 100);
        }

        public Task<bool> SaveJPG(string filePath, int quality)
        {
            return Save(filePath, SKEncodedImageFormat.Png, quality);
        }

        public Task<byte[]> SaveJPG(int quality)
        {
            return Save(SKEncodedImageFormat.Jpeg, quality);
        }

        public Task<byte[]> Save(int quality = 60)
        {
            if (IsOpaque)
                return SaveJPG(quality);
            return SavePNG();
        }

        public Task<bool> Save(string filePath, int quality = 60)
        {
            if (IsOpaque)
                return SaveJPG(filePath, quality);
            return SavePNG(filePath);
        }

        Task<bool> Save(string filePath, SKEncodedImageFormat format, int quality)
        {
            return Task.Run(() =>
            {
                try
                {
                    var image = SKImage.FromBitmap(_bitmap);
                    var encoded = image.Encode(format, quality);
                    using (var stream = File.OpenWrite(filePath))
                    {
                        encoded.SaveTo(stream);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);

                }

                return true;
            });
        }

        Task<byte[]> Save(SKEncodedImageFormat format, int quality)
        {
            return Task.Run(() =>
            {
                try
                {
                    var image = SKImage.FromBitmap(_bitmap);
                    return image.Encode(format, quality).ToArray();
                }
                catch(Exception ex) 
                {
                    Log.IgnoreException(ex);
                }

                return null;
            });
        }

        public void Dispose()
        {
            _bitmap?.Dispose();
        }
    }
}
