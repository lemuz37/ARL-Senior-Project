using SkiaSharp;
using Svg.Skia;
using System;
using System.IO;

namespace UnBox3D.Utils
{
    public static class SVGToPNGConverter
    {
        public static void Convert(string svgPath, string pngPath)
        {
            try
            {
                using var stream = File.OpenRead(svgPath);
                var svg = new SKSvg();
                svg.Load(stream);

                var picture = svg.Picture;
                if (picture == null) return;

                var info = new SKImageInfo((int)picture.CullRect.Width, (int)picture.CullRect.Height);
                using var surface = SKSurface.Create(info);
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                canvas.DrawPicture(picture);
                canvas.Flush();

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                using var fileStream = File.OpenWrite(pngPath);
                data.SaveTo(fileStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting SVG to PNG: {ex.Message}");
            }
        }
    }
}
