using System;
using System.IO;

namespace SkiaSharp
{
    public class SKBitmap : IDisposable
    {
        public int Width;
        public int Height;

        public SKAlphaType AlphaType;
        public SKColorType ColorType;

        public SKBitmap(int width, int height, bool isOpaque)
        {

        }

        public SKBitmap Resize(SKImageInfo imageInfo, SKFilterQuality filterQuality)
        {
            return null;
        }

        public SKBitmap Copy()
        {
            return null;
        }

        public static SKBitmap Decode(Stream stream)
        {
            return null;
        }

        public static SKBitmap Decode(byte[] data)
        {
            return null;
        }

        public void Dispose()
        {
        }
    }

    public class SKImage : IDisposable
    {
        public SKImage Encode(SKEncodedImageFormat format, int quality)
        {
            return null;
        }

        public static SKImage FromBitmap(SKBitmap bitmap)
        {
            return null;
        }

        public void SaveTo(Stream stream)
        {

        }

        public byte[] ToArray()
        {
            return null;
        }

        public void Dispose()
        {
        }
    }

    public class SKPoint
    {
        public SKPoint(int x, int y)
        {

        }
    }

    public class SKRect
    {
        public SKRect(int x, int y, int width, int height)
        {

        }
    }

    public class SKImageInfo
    {
        public SKImageInfo(int weight, int height, SKColorType colorType, SKAlphaType AlphaType)
        {

        }
    }

    public enum SKColors
    {
        Transparent
    }

    public enum SKAlphaType
    {
        Opaque,
        Unknown
    }

    public enum SKFilterQuality
    {
        High
    }

    public enum SKColorType
    {

    }

    public enum SKEncodedImageFormat
    {
        Png,
        Jpeg
    }

    public class SKCanvas : IDisposable
    {
        public SKCanvas(SKBitmap bitmap)
        {

        }

        public void DrawBitmap(SKBitmap bitmap, SKPoint point)
        {

        }

        public void DrawBitmap(SKBitmap bitmap, SKRect rect)
        {

        }

        public void DrawBitmap(SKBitmap bitmap, SKRect rect, SKRect rect2)
        {

        }

        public void Clear(SKColors color)
        {

        }

        public void Dispose()
        {
        }
    }
}