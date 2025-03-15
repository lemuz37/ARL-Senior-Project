using Svg;
using Svg.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

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

        public static void ExportSvgPanels(string inputSvgPath, string outputDirectory, string filename, int pageIndex, float panelWidthMm, float panelHeightMm, float marginMm = 0f)
        {
            Debug.WriteLine($"panelWidthMM: {panelWidthMm} - panelHeightMM: {panelHeightMm}");

            Debug.WriteLine($"Processing Page: {pageIndex} - Filename: {inputSvgPath}");
            SvgDocument svgDocument = SvgDocument.Open(inputSvgPath);

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
                    Debug.WriteLine($"Exported panel to {outputFilePath} with x-offset: {xOffset}, y-offset: {yOffset}");
                }
            }
        }
    }
}