using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Svg;
using Svg.Transforms;
using System.IO;
using System.Drawing.Imaging;    // For ImageFormat
/* Values are in pixels
// SOMEONE GET ON THIS
// Margins don't work? It seems to crop out rather than provide that 2in buffer
// Needs a proper buffer instead of translating/offsetting the view
// OPTIMIZATION
// Rotation and eliminating empty boxes are nice to implement but not urgent 
*/

namespace UnBox3D.Utils
{
    public class SVGEditor
    {
        private const float MmToPx = 3.779527f;
        private readonly ILogger _logger;

        public SVGEditor(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ExportSvgPanels(string inputSvgPath, string outputDirectory, string filename, int pageIndex, float panelWidthMm, float panelHeightMm, float marginMm = 0f)
        {
            _logger.Info($"Processing Page: {pageIndex} - Filename: {inputSvgPath}");
            SvgDocument svgDocument = SvgDocument.Open(inputSvgPath);

            try
            {
                if (File.Exists(inputSvgPath))
                {
                    File.Delete(inputSvgPath);
                    _logger.Info($"Deleted SVG file: {inputSvgPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete {inputSvgPath}: {ex.Message}");
            }

            float panelWidth = panelWidthMm * MmToPx;
            float panelHeight = panelHeightMm * MmToPx;
            float margin = marginMm * MmToPx;

            float svgWidth = svgDocument.Width.Value * MmToPx;
            float svgHeight = svgDocument.Height.Value * MmToPx;

            int numPanelsX = (int)Math.Ceiling((svgWidth - 2 * margin) / panelWidth);
            int numPanelsY = (int)Math.Ceiling((svgHeight - 2 * margin) / panelHeight);

            for (int x = 0; x < numPanelsX; x++)
            {
                for (int y = 0; y < numPanelsY; y++)
                {
                    float xOffset = x * panelWidth + margin;
                    float yOffset = y * panelHeight + margin;

                    SvgDocument panelDoc = new SvgDocument
                    {
                        Width = new SvgUnit(panelWidth),
                        Height = new SvgUnit(panelHeight)
                    };

                    foreach (SvgElement element in svgDocument.Children)
                    {
                        SvgElement clonedElement = (SvgElement)element.DeepCopy();
                        clonedElement.Transforms = new SvgTransformCollection
                        {
                            new SvgScale(MmToPx),
                            new SvgTranslate(-xOffset / MmToPx, -yOffset / MmToPx)
                        };
                        panelDoc.Children.Add(clonedElement);
                    }

                    string outputFilePath = Path.Combine(outputDirectory, $"{filename}_panel_page{pageIndex}_{x}_{y}.svg");
                    panelDoc.Write(outputFilePath);
                    _logger.Info($"Exported panel to {outputFilePath} with x-offset: {xOffset}, y-offset: {yOffset}");
                }
            }
        }

        public bool ExportToPdf(string svgFile, PdfDocument pdf)
        {
            _logger.Info($"Combining SVG: {svgFile}");

            try
            {
                SvgDocument svgDoc;
                using (var fs = File.OpenRead(svgFile))
                {
                    svgDoc = SvgDocument.Open<SvgDocument>(fs);
                }

                using var bmp = svgDoc.Draw();
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                var page = pdf.AddPage();
                page.Width = XUnit.FromPoint(bmp.Width);
                page.Height = XUnit.FromPoint(bmp.Height);

                using var gfx = XGraphics.FromPdfPage(page);
                using var xImage = XImage.FromStream(() => ms);
                gfx.DrawImage(xImage, 0, 0);

                return true;
            }
            catch (Svg.Exceptions.SvgMemoryException ex)
            {
                _logger.Error($"Caught SvgMemoryException: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing SVG: {ex.Message}");
                return false;
            }
        }
    }
}
