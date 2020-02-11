using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace AppBuilder
{
    abstract class ImageOperation
    {
        public readonly string SourcImage;
        public readonly string TargetImage;

        public readonly bool IsPNG;
        public readonly bool IsSolid;

        protected ImageOperation(string sourceImage, string targetImage, string[] options)
        {
            if (targetImage.EndsWith(".png", StringComparison.Ordinal))
                IsPNG = true;

            foreach(var option in options)
            {
                var o = option.Trim().ToLower();
                if (o == "png")
                    IsPNG = true;
                else if (o == "solid")
                    IsSolid = true;
            }

            SourcImage = sourceImage.Trim();
            TargetImage = targetImage.Trim();
        }

        protected void Save(Image<Rgba32> image, DirectoryInfo targetPath)
        {
            var target = Path.Combine(targetPath.FullName, TargetImage);
            Directory.CreateDirectory(Path.GetDirectoryName(target));

            Console.WriteLine($"Processing image {target}.");

            if (IsPNG)
            {
                image.Save(target, new PngEncoder { ColorType = IsSolid ? PngColorType.Rgb : PngColorType.RgbWithAlpha });
            }
            else
            {
                image.Save(target);
            }
        }

        public abstract void Process(DirectoryInfo infoPath, DirectoryInfo targetPath);
    }

    sealed class ResizeImageOperation : ImageOperation
    {
        public const string OpName = "resize";

        public readonly int Width;
        public readonly int Height;

        public ResizeImageOperation(string[] parameters, string sourceImage, string targetImage, string[] options) : base(sourceImage, targetImage, options)
        {
            var parts = parameters[0].Split("x");

            Width = int.Parse(parts[0].Trim());
            Height = int.Parse(parts[1].Trim());
        }

        public override void Process(DirectoryInfo infoPath, DirectoryInfo targetPath)
        {
            using (var image = Image.Load<Rgba32>(Path.Combine(infoPath.FullName, SourcImage)))
            {
                image.Mutate(x => x.Resize(Width, Height));

                Save(image, targetPath);
            }
        }
    }

    sealed class BlendImageOperation : ImageOperation
    {
        public const string OpName = "blend";

        public readonly int ImageWidth;
        public readonly int ImageHeight;

        public readonly int BlendWidth;
        public readonly int BlendHeight;

        public BlendImageOperation(string[] parameters, string sourceImage, string targetImage, string[] options) : base(sourceImage, targetImage, options)
        {
            var imagesize = parameters[0].Split("x");
            var blendsize = parameters[1].Split("x");

            ImageWidth = int.Parse(imagesize[0].Trim());
            ImageHeight = int.Parse(imagesize[1].Trim());

            BlendWidth = int.Parse(blendsize[0].Trim());
            BlendHeight = int.Parse(blendsize[1].Trim());
        }

        public override void Process(DirectoryInfo infoPath, DirectoryInfo targetPath)
        {
            using (var image = new Image<Rgba32>(ImageWidth, ImageHeight))
            {
                using(var blend = Image.Load(Path.Combine(infoPath.FullName, SourcImage)))
                {
                    blend.Mutate(x => x.Resize(BlendWidth, BlendHeight));

                    var xp = image.Width / 2 - blend.Width / 2;
                    var yp = image.Height / 2 - blend.Height / 2;

                    image.Mutate(x => x.DrawImage(blend, new Point(xp, yp), 1));

                    Save(image, targetPath);
                }
            }
        }
    }

    // imageblend newsize, blendimagesize, blendimage
    sealed class ImageBlendOperation : ImageOperation
    {
        public const string OpName = "imageblend";

        public readonly int ImageWidth;
        public readonly int ImageHeight;

        public readonly int BlendWidth;
        public readonly int BlendHeight;

        public readonly string BlendImage;

        public ImageBlendOperation(string[] parameters, string sourceImage, string targetImage, string[] options) : base(sourceImage, targetImage, options)
        {
            var imagesize = parameters[0].Split("x");
            var blendsize = parameters[1].Split("x");

            BlendImage = parameters[2];

            ImageWidth = int.Parse(imagesize[0].Trim());
            ImageHeight = int.Parse(imagesize[1].Trim());

            BlendWidth = int.Parse(blendsize[0].Trim());
            BlendHeight = int.Parse(blendsize[1].Trim());
        }

        public override void Process(DirectoryInfo infoPath, DirectoryInfo targetPath)
        {
            using (var finaleImage = new Image<Rgba32>(ImageWidth, ImageHeight))
            {
                using (var image = Image.Load(Path.Combine(infoPath.FullName, SourcImage)))
                {
                    image.Mutate(x => x.Resize(ImageWidth, ImageHeight));
                    finaleImage.Mutate(x => x.DrawImage(image, new Point(0, 0), 1));

                    using(var blendImage = Image.Load(Path.Combine(infoPath.FullName, BlendImage)))
                    {
                        blendImage.Mutate(x => x.Resize(BlendWidth, BlendHeight));

                        var xp = image.Width / 2 - blendImage.Width / 2;
                        var yp = image.Height / 2 - blendImage.Height / 2;

                        finaleImage.Mutate(x => x.DrawImage(blendImage, new Point(xp, yp), 1));

                        Save(finaleImage, targetPath);
                    }
                }
            }
        }
    }
}
