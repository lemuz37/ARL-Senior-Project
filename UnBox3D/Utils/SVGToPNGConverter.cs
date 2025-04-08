using SkiaSharp;
using Svg;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace UnBox3D.Utils
{
    public static class SVGToPNGConverter
    {
        public static void Convert(string svgPath, string pngPath)
        {
            try
            {
                SvgDocument svgDocument;
                using (var stream = File.OpenRead(svgPath))
                {
                    svgDocument = SvgDocument.Open<SvgDocument>(stream);
                }

                // Scale factor to improve line visibility in PNG preview
                int scaleFactor = 4;

                // Extract size, fallback to 1000x1000 if invalid
                int width = (int)((svgDocument.Width.Type == SvgUnitType.Pixel) ? svgDocument.Width.Value : 1000) * scaleFactor;
                int height = (int)((svgDocument.Height.Type == SvgUnitType.Pixel) ? svgDocument.Height.Value : 1000) * scaleFactor;

                // Optional cap to avoid over-scaling
                const int MaxSize = 4000;
                if (width > MaxSize || height > MaxSize)
                {
                    float scale = (float)MaxSize / Math.Max(width, height);
                    width = (int)(width * scale);
                    height = (int)(height * scale);
                }

                using var bitmap = new Bitmap(width, height);
                using var g = Graphics.FromImage(bitmap);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.White);
                svgDocument.Draw(g);

                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using var skBitmap = SKBitmap.Decode(memoryStream);
                using var surface = SKSurface.Create(new SKImageInfo(skBitmap.Width, skBitmap.Height));
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                canvas.DrawBitmap(skBitmap, 0, 0);

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                using var fileStream = File.OpenWrite(pngPath);
                data.SaveTo(fileStream);
            }
            catch (Exception ex)
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "conversion_errors.log");
                File.AppendAllText(logPath, $"{DateTime.Now} | Failed to convert: {svgPath} → {pngPath}\n{ex}\n\n");
                Console.WriteLine($"[SVGToPNGConverter] Failed: {ex.Message}");
            }
        }
    }
}
