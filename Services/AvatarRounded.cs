using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.IO.Compression;

namespace DarlingBotNet.Services
{
    static class SixLaborsImage
    {
        //        public static Image ApplyRoundedCorners(this Image<Rgba32> img, int cor)
        //        {
        //            return img.Clone(x => x.ConvertToAvatar(new Size(img.Width, img.Height), cor));
        //        }

        public static MemoryStream ToStream(this Image<Rgba32> img, string format = null)
        {
            var imageStream = new MemoryStream();
            if (format == "GIF") img.SaveAsGif(imageStream);
            else img.SaveAsPng(imageStream, new PngEncoder() { CompressionLevel = 6 });
            imageStream.Position = 0;
            return imageStream;
        }
        //        private static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext processingContext, Size size, float cornerRadius)
        //        {
        //            return processingContext.Resize(new ResizeOptions
        //            {
        //                Size = size,
        //                Mode = ResizeMode.Crop
        //            }).ApplyRoundedCorners(cornerRadius);
        //        }
        //        private static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
        //        {
        //            Size size = ctx.GetCurrentSize();
        //            IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

        //            ctx.SetGraphicsOptions(new GraphicsOptions()
        //            {
        //                Antialias = true,
        //                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
        //            });

        //            foreach (var c in corners)
        //            {
        //                ctx = ctx.Fill(Color.Red, c);
        //            }
        //            return ctx;
        //        }

        //        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        //        {
        //            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

        //            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

        //            float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
        //            float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

        //            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
        //            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
        //            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

        //            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        //        }
    }
}