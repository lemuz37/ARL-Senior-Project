using System;
using System.IO;

namespace UnBox3D
{
    public static class BeamArrayExporter
    {
        public static void GenerateRightAngleBeamsObjFile(
            string filePath,
            float shortDepth,
            float mediumDepth,
            float longDepth,
            float legLength = 3.75f,
            float hypotenuse = 5.5f,
            float spacing = 5f // space between beams
        )
        {
            using StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine("# Right-angle triangle beams");

            int vertexOffset = 1;

            // Write 4 short beams
            for (int i = 0; i < 4; i++)
            {
                float x = i * (shortDepth + spacing);
                WriteRightAngleBeam(writer, shortDepth, legLength, x, 0, 0, ref vertexOffset);
            }

            // 4 medium beams, shifted in Z
            for (int i = 0; i < 4; i++)
            {
                float x = i * (mediumDepth + spacing);
                WriteRightAngleBeam(writer, mediumDepth, legLength, x, 0, -10, ref vertexOffset);
            }

            // 4 long beams, shifted further in Z
            for (int i = 0; i < 4; i++)
            {
                float x = i * (longDepth + spacing);
                WriteRightAngleBeam(writer, longDepth, legLength, x, 0, -20, ref vertexOffset);
            }
        }

        private static void WriteRightAngleBeam(StreamWriter writer, float depth, float leg, float offsetX, float offsetY, float offsetZ, ref int vertexOffset)
        {
            // Triangle base is in the YZ plane, extruded along X
            var front = new (float x, float y, float z)[]
            {
                (0, 0, 0),           // v0 - corner
                (0, leg, 0),         // v1 - vertical leg
                (0, 0, leg)          // v2 - horizontal leg
            };

            var back = new (float x, float y, float z)[]
            {
                (depth, 0, 0),       // v3
                (depth, leg, 0),     // v4
                (depth, 0, leg)      // v5
            };

            // Write vertices
            foreach (var (x, y, z) in front)
                writer.WriteLine($"v {x + offsetX} {y + offsetY} {z + offsetZ}");
            foreach (var (x, y, z) in back)
                writer.WriteLine($"v {x + offsetX} {y + offsetY} {z + offsetZ}");

            int v = vertexOffset;

            // Faces
            writer.WriteLine($"f {v} {v + 1} {v + 2}");             // front face
            writer.WriteLine($"f {v + 3} {v + 5} {v + 4}");         // back face
            writer.WriteLine($"f {v} {v + 3} {v + 4} {v + 1}");     // side 1
            writer.WriteLine($"f {v} {v + 2} {v + 5} {v + 3}");     // side 2
            writer.WriteLine($"f {v + 1} {v + 4} {v + 5} {v + 2}"); // hypotenuse face

            vertexOffset += 6;
        }
    }
}


