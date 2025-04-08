using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Svg = Svg.Custom;


namespace UnBox3D.Utils
{
    public class SVGEditor
    {
        private const float MmToPx = 3.779527f;

        public static void ExportSvgPanels(string inputSvgPath, string outputDirectory, string filename, int pageIndex, float panelWidthMm, float panelHeightMm, float marginMm = 0f, bool includeLabels = false, bool storeMetadata = true)
        {
            Debug.WriteLine($"Processing Page: {pageIndex} - Filename: {inputSvgPath}");
            SvgDocument svgDocument = SvgDocument.Open(inputSvgPath);

            float panelWidth = panelWidthMm * MmToPx;
            float panelHeight = panelHeightMm * MmToPx;
            float margin = marginMm * MmToPx;

            float svgWidth = svgDocument.Width.Value * MmToPx;
            float svgHeight = svgDocument.Height.Value * MmToPx;

            int numPanelsX = (int)Math.Ceiling((svgWidth - 2 * margin) / panelWidth);
            int numPanelsY = (int)Math.Ceiling((svgHeight - 2 * margin) / panelHeight);

            Debug.WriteLine($"Total panels: {numPanelsX} x {numPanelsY}");

            string svgFolderPath = Path.Combine(outputDirectory, $"{filename}_Export");
            Directory.CreateDirectory(svgFolderPath);

            for (int x = 0; x < numPanelsX; x++)
            {
                for (int y = 0; y < numPanelsY; y++)
                {
                    float xOffset = x * panelWidth + margin;
                    float yOffset = y * panelHeight + margin;

                    var panelDoc = new SvgDocument
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

                        if (IsElementPartiallyInBounds(clonedElement, panelWidth, panelHeight))
                        {
                            panelDoc.Children.Add(clonedElement);
                        }
                    }

                    if (includeLabels)
                    {
                        AddAlignmentLabels(panelDoc, filename, pageIndex, x, y);
                    }

                    if (storeMetadata)
                    {
                        AddMetadata(panelDoc, filename, pageIndex, x, y);
                    }

                    string outputFilePath = Path.Combine(svgFolderPath, $"{filename}_panel_page{pageIndex}_{x}_{y}.svg");
                    panelDoc.Write(outputFilePath);
                    Debug.WriteLine($"Exported panel to {outputFilePath} with x-offset: {xOffset}, y-offset: {yOffset}");
                }
            }

            Debug.WriteLine($"SVG export folder created at: {svgFolderPath}");
        }

        private static bool IsElementPartiallyInBounds(SvgElement element, float panelWidth, float panelHeight)
        {
            foreach (var transform in element.Transforms)
            {
                if (transform is SvgTranslate translate)
                {
                    float buffer = 10;
                    if ((translate.X + buffer) < 0 || (translate.X - buffer) > panelWidth ||
                        (translate.Y + buffer) < 0 || (translate.Y - buffer) > panelHeight)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void AddAlignmentLabels(SvgDocument doc, string filename, int pageIndex, int x, int y)
        {
            var textLabel = new SvgText($"{filename} - Page {pageIndex} ({x}, {y})")
            {
                X = new SvgUnitCollection { new SvgUnit(10) },
                Y = new SvgUnitCollection { new SvgUnit(20) },
                FontSize = new SvgUnit(12),
                Fill = new SvgColourServer(Color.Black)
            };
            doc.Children.Add(textLabel);
        }

        private static void AddMetadata(SvgDocument doc, string filename, int pageIndex, int x, int y)
        {
            var metadata = new SvgDescription
            {
                Content = $"Filename: {filename}, Page: {pageIndex}, Panel: ({x}, {y})"
            };
            doc.Children.Add(metadata);
        }
    }
}
