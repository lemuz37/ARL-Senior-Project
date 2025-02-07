using OpenTK.Mathematics;

namespace UnBox3D.Utils
{
    public static class Colors
    {
        public static readonly Vector3 Red = new Vector3(1.0f, 0.0f, 0.0f);
        public static readonly Vector3 Orange = new Vector3(1.0f, 0.65f, 0.0f);
        public static readonly Vector3 Yellow = new Vector3(1.0f, 1.0f, 0.0f);
        public static readonly Vector3 Green = new Vector3(0.0f, 1.0f, 0.0f);
        public static readonly Vector3 Blue = new Vector3(0.0f, 0.0f, 1.0f);
        public static readonly Vector3 Indigo = new Vector3(0.29f, 0.0f, 0.51f);
        public static readonly Vector3 Violet = new Vector3(0.93f, 0.51f, 0.93f);
        public static readonly Vector3 Brown = new Vector3(0.65f, 0.16f, 0.16f);
        public static readonly Vector3 Cyan = new Vector3(0.0f, 1.0f, 1.0f);
        public static readonly Vector3 Grey = new Vector3(0.5f, 0.5f, 0.5f);
        public static readonly Vector3 Lime = new Vector3(0.75f, 1.0f, 0.0f);
        public static readonly Vector3 Magenta = new Vector3(1.0f, 0.0f, 1.0f);
        public static readonly Vector3 Maroon = new Vector3(0.5f, 0.0f, 0.0f);
        public static readonly Vector3 Navy = new Vector3(0.0f, 0.0f, 0.5f);
        public static readonly Vector3 Olive = new Vector3(0.5f, 0.5f, 0.0f);
        public static readonly Vector3 Pink = new Vector3(1.0f, 0.75f, 0.8f);
        public static readonly Vector3 Purple = new Vector3(0.5f, 0.0f, 0.5f);
        public static readonly Vector3 Silver = new Vector3(0.75f, 0.75f, 0.75f);
        public static readonly Vector3 Teal = new Vector3(0.0f, 0.5f, 0.5f);
        public static readonly Vector3 White = new Vector3(1.0f, 1.0f, 1.0f);
        public static readonly Vector3 Black = new Vector3(0.0f, 0.0f, 0.0f);

        // This dictionary maps color names (in nospacelowercase) to their respective Vector3 values.
        // For quick reference, the colors and their opposites on the color wheel are:
        // Red    ---> Cyan
        // Orange ---> Blue
        // Yellow ---> Violet
        // Green  ---> Magenta
        // Blue   ---> Orange
        // Indigo ---> Lime
        // Violet ---> Yellow
        //
        public static readonly Dictionary<string, Vector3> colorMap = new Dictionary<string, Vector3>()
        {
            { "red", Red },
            { "orange", Orange },
            { "yellow", Yellow },
            { "green", Green },
            { "blue", Blue },
            { "indigo", Indigo },
            { "violet", Violet },
            { "brown", Brown },
            { "cyan", Cyan },
            { "grey", Grey },
            { "lime", Lime },
            { "magenta", Magenta },
            { "maroon", Maroon },
            { "navy", Navy },
            { "olive", Olive },
            { "pink", Pink },
            { "purple", Purple },
            { "silver", Silver },
            { "teal", Teal },
            { "white", White },
            { "black", Black }
        };
    }

    public static class BackgroundColors
    {
        public static readonly Vector4 LightGrey = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
        public static readonly Vector4 DarkGrey = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        public static readonly Vector4 Beige = new Vector4(0.96f, 0.96f, 0.86f, 1.0f);
        public static readonly Vector4 LightBlue = new Vector4(0.68f, 0.85f, 0.9f, 1.0f);
        public static readonly Vector4 LightGreen = new Vector4(0.56f, 0.93f, 0.56f, 1.0f);
        public static readonly Vector4 Ivory = new Vector4(1.0f, 1.0f, 0.94f, 1.0f);
        public static readonly Vector4 Lavender = new Vector4(0.9f, 0.9f, 0.98f, 1.0f);
        public static readonly Vector4 MistyRose = new Vector4(1.0f, 0.89f, 0.88f, 1.0f);
        public static readonly Vector4 PeachPuff = new Vector4(1.0f, 0.85f, 0.73f, 1.0f);
        public static readonly Vector4 AliceBlue = new Vector4(0.94f, 0.97f, 1.0f, 1.0f);
        public static readonly Vector4 Honeydew = new Vector4(0.94f, 1.0f, 0.94f, 1.0f);
        public static readonly Vector4 WhiteSmoke = new Vector4(0.96f, 0.96f, 0.96f, 1.0f);
        public static readonly Vector4 White = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public static readonly Vector4 Black = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        public static readonly Vector4 Transparent = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        // This dictionary maps background color names to their respective Vector4 values.
        // Below are some good combinations for good contrast and for color blindness:
        //
        // High contrast mesh-background combos:
        //  - LightGrey (background)  ---> Red, Blue, Black (mesh colors)
        //  - DarkGrey (background)   ---> Yellow, White, Cyan (mesh colors)
        //  - Beige (background)      ---> Navy, Magenta, Black (mesh colors)
        //  - LightBlue (background)  ---> Orange, Violet, Red (mesh colors)
        //  - White (background)      ---> Black, Green, Navy (mesh colors)
        //  - Black (background)      ---> White, Yellow, LightGreen (mesh colors)
        //
        // For color blindness-friendly combos:
        //  - Protanopia/Deuteranopia (red-green color blindness):
        //    - DarkGrey (background)   ---> Blue, Cyan, Magenta (mesh colors)
        //    - LightGrey (background)  ---> Blue, Violet, Cyan (mesh colors)
        //  - Tritanopia (blue-yellow color blindness):
        //    - LightBlue (background)  ---> Red, Magenta, Orange (mesh colors)
        //    - DarkGrey (background)   ---> Red, Orange, Magenta (mesh colors)
        //
        public static readonly Dictionary<string, Vector4> backgroundColorMap = new Dictionary<string, Vector4>()
        {
            { "lightgrey", LightGrey },
            { "darkgrey", DarkGrey },
            { "beige", Beige },
            { "lightblue", LightBlue },
            { "lightgreen", LightGreen },
            { "ivory", Ivory },
            { "lavender", Lavender },
            { "mistyrose", MistyRose },
            { "peachpuff", PeachPuff },
            { "aliceblue", AliceBlue },
            { "honeydew", Honeydew },
            { "whitesmoke", WhiteSmoke },
            { "white", White },
            { "black", Black },
            { "transparent", Transparent }
        };
    }
}
