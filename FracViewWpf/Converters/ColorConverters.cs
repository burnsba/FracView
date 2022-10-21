using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SD = System.Drawing;
using SWM = System.Windows.Media;

namespace FracViewWpf.Converters
{
    /// <summary>
    /// Converts text to one of the standard color classes.
    /// </summary>
    public static class ColorConverters
    {
        private static Regex _sixDigits = new Regex("^[0-9a-fA-F]{6}$");

        /// <summary>
        /// Conversion methods for <see cref="System.Drawing.Color"/>.
        /// </summary>
        public static class Drawing
        {
            /// <summary>
            /// Converts a HTML hex color string to standard color object.
            /// Optional "#" at string start.
            /// Either a 3 digit or 6 digit value is required.
            /// </summary>
            /// <param name="value">HTML hex color.</param>
            /// <returns>Parsed color.</returns>
            public static SD.Color FromHtmlHex(string value)
            {
                var rgb = Common.FromHtmlHex(value);
                return SD.Color.FromArgb(rgb.Item1, rgb.Item2, rgb.Item3);
            }

            /// <summary>
            /// Attempts to parse a color described by a CSS style to a standard color object.
            /// This will first attempt to parse as a color with a leading "#".
            /// If that fails, will check if this is a "rgb(0,0,0)" formatted string.
            /// If that fails, will attempt to lookup the value according to standard
            /// color definition names ("black", etc).
            /// </summary>
            /// <param name="value">CSS style color value.</param>
            /// <returns>Parsed color.</returns>
            public static SD.Color FromCss(string value)
            {
                var rgb = Common.FromCss(value);
                return SD.Color.FromArgb(rgb.Item1, rgb.Item2, rgb.Item3);
            }

            /// <summary>
            /// Convert color to six digit hex string.
            /// </summary>
            /// <param name="c">Color.</param>
            /// <returns>Hex string.</returns>
            public static string ToHexSix(SD.Color c)
            {
                return $"{c.R:x2}{c.G:x2}{c.B:x2}";
            }

            /// <summary>
            /// Convert color to six digit hex string with additional leading "#" character.
            /// </summary>
            /// <param name="c">Color.</param>
            /// <returns>Hex string.</returns>
            public static string ToHexSeven(SD.Color c)
            {
                return $"#{c.R:x2}{c.G:x2}{c.B:x2}";
            }
        }

        /// <summary>
        /// Conversion methods for <see cref="System.Windows.Media.Color"/>.
        /// </summary>
        public static class WindowsMedia
        {
            /// <summary>
            /// Converts a HTML hex color string to standard color object.
            /// Optional "#" at string start.
            /// Either a 3 digit or 6 digit value is required.
            /// </summary>
            /// <param name="value">HTML hex color.</param>
            /// <returns>Parsed color.</returns>
            public static SWM.Color FromHtmlHex(string value)
            {
                var rgb = Common.FromHtmlHex(value);
                return SWM.Color.FromArgb(255, rgb.Item1, rgb.Item2, rgb.Item3);
            }

            /// <summary>
            /// Attempts to parse a color described by a CSS style to a standard color object.
            /// This will first attempt to parse as a color with a leading "#".
            /// If that fails, will check if this is a "rgb(0,0,0)" formatted string.
            /// If that fails, will attempt to lookup the value according to standard
            /// color definition names ("black", etc).
            /// </summary>
            /// <param name="value">CSS style color value.</param>
            /// <returns>Parsed color.</returns>
            public static SWM.Color FromCss(string value)
            {
                var rgb = Common.FromCss(value);
                return SWM.Color.FromArgb(255, rgb.Item1, rgb.Item2, rgb.Item3);
            }

            /// <summary>
            /// Convert color to six digit hex string.
            /// </summary>
            /// <param name="c">Color.</param>
            /// <returns>Hex string.</returns>
            public static string ToHexSix(SWM.Color c)
            {
                return $"{c.R:x2}{c.G:x2}{c.B:x2}";
            }

            /// <summary>
            /// Convert color to six digit hex string with additional leading "#" character.
            /// </summary>
            /// <param name="c">Color.</param>
            /// <returns>Hex string.</returns>
            public static string ToHexSeven(SWM.Color c)
            {
                return $"#{c.R:x2}{c.G:x2}{c.B:x2}";
            }
        }

        /// <summary>
        /// Common conversion methods. A container tuple of three bytes is used to curry RGB color info.
        /// </summary>
        private static class Common
        {
            /// <summary>
            /// Converts a HTML hex color string to standard color object.
            /// Optional "#" at string start.
            /// Either a 3 digit or 6 digit value is required.
            /// </summary>
            /// <param name="value">HTML hex color.</param>
            /// <returns>Parsed color.</returns>
            public static ValueTuple<byte,byte,byte> FromHtmlHex(string value)
            {
                var trimmed = value?.Trim() ?? null;

                if (string.IsNullOrEmpty(trimmed))
                {
                    // check on trimmed, but throw against value
                    throw new ArgumentException(nameof(value));
                }

                int r, g, b;

                if (trimmed[0] == '#')
                {
                    if (trimmed.Length == 4)
                    {
                        r = Convert.ToInt32(value!.Substring(1, 1), 16);
                        g = Convert.ToInt32(value!.Substring(2, 1), 16);
                        b = Convert.ToInt32(value!.Substring(3, 1), 16);
                    }
                    else if (trimmed.Length == 7)
                    {
                        r = Convert.ToInt32(value!.Substring(1, 2), 16);
                        g = Convert.ToInt32(value!.Substring(3, 2), 16);
                        b = Convert.ToInt32(value!.Substring(5, 2), 16);
                    }
                    else
                    {
                        throw new ArgumentException(nameof(value));
                    }

                    return new ValueTuple<byte, byte, byte>((byte)r, (byte)g, (byte)b);
                }
                else if (trimmed.Length == 3)
                {
                    r = Convert.ToInt32(value!.Substring(0, 1), 16);
                    g = Convert.ToInt32(value!.Substring(1, 1), 16);
                    b = Convert.ToInt32(value!.Substring(2, 1), 16);

                    return new ValueTuple<byte, byte, byte>((byte)r, (byte)g, (byte)b);
                }
                else if (_sixDigits.IsMatch(trimmed))
                {
                    r = Convert.ToInt32(value!.Substring(0, 2), 16);
                    g = Convert.ToInt32(value!.Substring(2, 2), 16);
                    b = Convert.ToInt32(value!.Substring(4, 2), 16);

                    return new ValueTuple<byte, byte, byte>((byte)r, (byte)g, (byte)b);
                }

                throw new NotSupportedException($"Can not resolve HTML hex color: \"{value}\"");
            }

            /// <summary>
            /// Attempts to parse a color described by a CSS style to a standard color object.
            /// This will first attempt to parse as a color with a leading "#".
            /// If that fails, will check if this is a "rgb(0,0,0)" formatted string.
            /// If that fails, will attempt to lookup the value according to standard
            /// color definition names ("black", etc).
            /// </summary>
            /// <param name="value">CSS style color value.</param>
            /// <returns>Parsed color.</returns>
            public static ValueTuple<byte, byte, byte> FromCss(string value)
            {
                var trimmed = value?.Trim() ?? null;

                if (string.IsNullOrEmpty(trimmed))
                {
                    // check on trimmed, but throw against value
                    throw new ArgumentException(nameof(value));
                }

                int r, g, b;

                // try leading hash color
                if (trimmed[0] == '#')
                {
                    if (trimmed.Length == 4)
                    {
                        // one digit hex values
                        r = Convert.ToInt32(trimmed.Substring(1, 1), 16);
                        g = Convert.ToInt32(trimmed.Substring(2, 1), 16);
                        b = Convert.ToInt32(trimmed.Substring(3, 1), 16);
                    }
                    else if (trimmed.Length == 7)
                    {
                        // two digit hex values
                        r = Convert.ToInt32(trimmed.Substring(1, 2), 16);
                        g = Convert.ToInt32(trimmed.Substring(3, 2), 16);
                        b = Convert.ToInt32(trimmed.Substring(5, 2), 16);
                    }
                    else
                    {
                        throw new ArgumentException(nameof(value));
                    }

                    return new ValueTuple<byte, byte, byte>((byte)r, (byte)g, (byte)b);
                }
                else if (trimmed.StartsWith("rgb"))
                {
                    // try rgb(0,0,0) color

                    int start = trimmed.IndexOf('(') + 1;
                    int end = trimmed.IndexOf(')');

                    if (!(start > 0) || !(end > -1) || (end <= start))
                    {
                        throw new ArgumentException(nameof(value));
                    }

                    var inner = trimmed.Substring(start, end - start);
                    var values = inner.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                    if (!values.Any() || values.Count != 3)
                    {
                        throw new ArgumentException(nameof(value));
                    }

                    // RGB values are decimal
                    r = Convert.ToInt32(values[0]);
                    g = Convert.ToInt32(values[1]);
                    b = Convert.ToInt32(values[2]);

                    return new ValueTuple<byte, byte, byte>((byte)r, (byte)g, (byte)b);
                }
                else if (_sixDigits.IsMatch(trimmed))
                {
                    // two digit hex values
                    r = Convert.ToInt32(trimmed.Substring(0, 2), 16);
                    g = Convert.ToInt32(trimmed.Substring(2, 2), 16);
                    b = Convert.ToInt32(trimmed.Substring(4, 2), 16);

                    return new ValueTuple<byte, byte, byte>((byte)r, (byte)g, (byte)b);
                }
                else
                {
                    // try standard name color
                    switch (trimmed.ToLower())
                    {
                        /*
                         * These are "standard"? CSS colors
                         */
                        case "aliceblue": return FromHtmlHexUnverified("#f0f8ff");
                        case "antiquewhite": return FromHtmlHexUnverified("#faebd7");
                        case "aqua": return FromHtmlHexUnverified("#00ffff");
                        case "aquamarine": return FromHtmlHexUnverified("#7fffd4");
                        case "azure": return FromHtmlHexUnverified("#f0ffff");
                        case "beige": return FromHtmlHexUnverified("#f5f5dc");
                        case "bisque": return FromHtmlHexUnverified("#ffe4c4");
                        case "black": return FromHtmlHexUnverified("#000000");
                        case "blanchedalmond": return FromHtmlHexUnverified("#ffebcd");
                        case "blue": return FromHtmlHexUnverified("#0000ff");
                        case "blueviolet": return FromHtmlHexUnverified("#8a2be2");
                        case "brown": return FromHtmlHexUnverified("#a52a2a");
                        case "burlywood": return FromHtmlHexUnverified("#deb887");
                        case "cadetblue": return FromHtmlHexUnverified("#5f9ea0");
                        case "chartreuse": return FromHtmlHexUnverified("#7fff00");
                        case "chocolate": return FromHtmlHexUnverified("#d2691e");
                        case "coral": return FromHtmlHexUnverified("#ff7f50");
                        case "cornflowerblue": return FromHtmlHexUnverified("#6495ed");
                        case "cornsilk": return FromHtmlHexUnverified("#fff8dc");
                        case "crimson": return FromHtmlHexUnverified("#dc143c");
                        case "cyan": return FromHtmlHexUnverified("#00ffff");
                        case "darkblue": return FromHtmlHexUnverified("#00008b");
                        case "darkcyan": return FromHtmlHexUnverified("#008b8b");
                        case "darkgoldenrod": return FromHtmlHexUnverified("#b8860b");
                        case "darkgray": return FromHtmlHexUnverified("#a9a9a9");
                        case "darkgrey": return FromHtmlHexUnverified("#a9a9a9");
                        case "darkgreen": return FromHtmlHexUnverified("#006400");
                        case "darkkhaki": return FromHtmlHexUnverified("#bdb76b");
                        case "darkmagenta": return FromHtmlHexUnverified("#8b008b");
                        case "darkolivegreen": return FromHtmlHexUnverified("#556b2f");
                        case "darkorange": return FromHtmlHexUnverified("#ff8c00");
                        case "darkorchid": return FromHtmlHexUnverified("#9932cc");
                        case "darkred": return FromHtmlHexUnverified("#8b0000");
                        case "darksalmon": return FromHtmlHexUnverified("#e9967a");
                        case "darkseagreen": return FromHtmlHexUnverified("#8fbc8f");
                        case "darkslateblue": return FromHtmlHexUnverified("#483d8b");
                        case "darkslategray": return FromHtmlHexUnverified("#2f4f4f");
                        case "darkslategrey": return FromHtmlHexUnverified("#2f4f4f");
                        case "darkturquoise": return FromHtmlHexUnverified("#00ced1");
                        case "darkviolet": return FromHtmlHexUnverified("#9400d3");
                        case "deeppink": return FromHtmlHexUnverified("#ff1493");
                        case "deepskyblue": return FromHtmlHexUnverified("#00bfff");
                        case "dimgray": return FromHtmlHexUnverified("#696969");
                        case "dimgrey": return FromHtmlHexUnverified("#696969");
                        case "dodgerblue": return FromHtmlHexUnverified("#1e90ff");
                        case "firebrick": return FromHtmlHexUnverified("#b22222");
                        case "floralwhite": return FromHtmlHexUnverified("#fffaf0");
                        case "forestgreen": return FromHtmlHexUnverified("#228b22");
                        case "fuchsia": return FromHtmlHexUnverified("#ff00ff");
                        case "gainsboro": return FromHtmlHexUnverified("#dcdcdc");
                        case "ghostwhite": return FromHtmlHexUnverified("#f8f8ff");
                        case "gold": return FromHtmlHexUnverified("#ffd700");
                        case "goldenrod": return FromHtmlHexUnverified("#daa520");
                        case "gray": return FromHtmlHexUnverified("#808080");
                        case "grey": return FromHtmlHexUnverified("#808080");
                        case "green": return FromHtmlHexUnverified("#008000");
                        case "greenyellow": return FromHtmlHexUnverified("#adff2f");
                        case "honeydew": return FromHtmlHexUnverified("#f0fff0");
                        case "hotpink": return FromHtmlHexUnverified("#ff69b4");
                        case "indianred": return FromHtmlHexUnverified("#cd5c5c");
                        case "indigo": return FromHtmlHexUnverified("#4b0082");
                        case "ivory": return FromHtmlHexUnverified("#fffff0");
                        case "khaki": return FromHtmlHexUnverified("#f0e68c");
                        case "lavender": return FromHtmlHexUnverified("#e6e6fa");
                        case "lavenderblush": return FromHtmlHexUnverified("#fff0f5");
                        case "lawngreen": return FromHtmlHexUnverified("#7cfc00");
                        case "lemonchiffon": return FromHtmlHexUnverified("#fffacd");
                        case "lightblue": return FromHtmlHexUnverified("#add8e6");
                        case "lightcoral": return FromHtmlHexUnverified("#f08080");
                        case "lightcyan": return FromHtmlHexUnverified("#e0ffff");
                        case "lightgoldenrodyellow": return FromHtmlHexUnverified("#fafad2");
                        case "lightgray": return FromHtmlHexUnverified("#d3d3d3");
                        case "lightgrey": return FromHtmlHexUnverified("#d3d3d3");
                        case "lightgreen": return FromHtmlHexUnverified("#90ee90");
                        case "lightpink": return FromHtmlHexUnverified("#ffb6c1");
                        case "lightsalmon": return FromHtmlHexUnverified("#ffa07a");
                        case "lightseagreen": return FromHtmlHexUnverified("#20b2aa");
                        case "lightskyblue": return FromHtmlHexUnverified("#87cefa");
                        case "lightslategray": return FromHtmlHexUnverified("#778899");
                        case "lightslategrey": return FromHtmlHexUnverified("#778899");
                        case "lightsteelblue": return FromHtmlHexUnverified("#b0c4de");
                        case "lightyellow": return FromHtmlHexUnverified("#ffffe0");
                        case "lime": return FromHtmlHexUnverified("#00ff00");
                        case "limegreen": return FromHtmlHexUnverified("#32cd32");
                        case "linen": return FromHtmlHexUnverified("#faf0e6");
                        case "magenta": return FromHtmlHexUnverified("#ff00ff");
                        case "maroon": return FromHtmlHexUnverified("#800000");
                        case "mediumaquamarine": return FromHtmlHexUnverified("#66cdaa");
                        case "mediumblue": return FromHtmlHexUnverified("#0000cd");
                        case "mediumorchid": return FromHtmlHexUnverified("#ba55d3");
                        case "mediumpurple": return FromHtmlHexUnverified("#9370d8");
                        case "mediumseagreen": return FromHtmlHexUnverified("#3cb371");
                        case "mediumslateblue": return FromHtmlHexUnverified("#7b68ee");
                        case "mediumspringgreen": return FromHtmlHexUnverified("#00fa9a");
                        case "mediumturquoise": return FromHtmlHexUnverified("#48d1cc");
                        case "mediumvioletred": return FromHtmlHexUnverified("#c71585");
                        case "midnightblue": return FromHtmlHexUnverified("#191970");
                        case "mintcream": return FromHtmlHexUnverified("#f5fffa");
                        case "mistyrose": return FromHtmlHexUnverified("#ffe4e1");
                        case "moccasin": return FromHtmlHexUnverified("#ffe4b5");
                        case "navajowhite": return FromHtmlHexUnverified("#ffdead");
                        case "navy": return FromHtmlHexUnverified("#000080");
                        case "oldlace": return FromHtmlHexUnverified("#fdf5e6");
                        case "olive": return FromHtmlHexUnverified("#808000");
                        case "olivedrab": return FromHtmlHexUnverified("#6b8e23");
                        case "orange": return FromHtmlHexUnverified("#ffa500");
                        case "orangered": return FromHtmlHexUnverified("#ff4500");
                        case "orchid": return FromHtmlHexUnverified("#da70d6");
                        case "palegoldenrod": return FromHtmlHexUnverified("#eee8aa");
                        case "palegreen": return FromHtmlHexUnverified("#98fb98");
                        case "paleturquoise": return FromHtmlHexUnverified("#afeeee");
                        case "palevioletred": return FromHtmlHexUnverified("#d87093");
                        case "papayawhip": return FromHtmlHexUnverified("#ffefd5");
                        case "peachpuff": return FromHtmlHexUnverified("#ffdab9");
                        case "peru": return FromHtmlHexUnverified("#cd853f");
                        case "pink": return FromHtmlHexUnverified("#ffc0cb");
                        case "plum": return FromHtmlHexUnverified("#dda0dd");
                        case "powderblue": return FromHtmlHexUnverified("#b0e0e6");
                        case "purple": return FromHtmlHexUnverified("#800080");
                        case "red": return FromHtmlHexUnverified("#ff0000");
                        case "rosybrown": return FromHtmlHexUnverified("#bc8f8f");
                        case "royalblue": return FromHtmlHexUnverified("#4169e1");
                        case "saddlebrown": return FromHtmlHexUnverified("#8b4513");
                        case "salmon": return FromHtmlHexUnverified("#fa8072");
                        case "sandybrown": return FromHtmlHexUnverified("#f4a460");
                        case "seagreen": return FromHtmlHexUnverified("#2e8b57");
                        case "seashell": return FromHtmlHexUnverified("#fff5ee");
                        case "sienna": return FromHtmlHexUnverified("#a0522d");
                        case "silver": return FromHtmlHexUnverified("#c0c0c0");
                        case "skyblue": return FromHtmlHexUnverified("#87ceeb");
                        case "slateblue": return FromHtmlHexUnverified("#6a5acd");
                        case "slategray": return FromHtmlHexUnverified("#708090");
                        case "slategrey": return FromHtmlHexUnverified("#708090");
                        case "snow": return FromHtmlHexUnverified("#fffafa");
                        case "springgreen": return FromHtmlHexUnverified("#00ff7f");
                        case "steelblue": return FromHtmlHexUnverified("#4682b4");
                        case "tan": return FromHtmlHexUnverified("#d2b48c");
                        case "teal": return FromHtmlHexUnverified("#008080");
                        case "thistle": return FromHtmlHexUnverified("#d8bfd8");
                        case "tomato": return FromHtmlHexUnverified("#ff6347");
                        case "turquoise": return FromHtmlHexUnverified("#40e0d0");
                        case "violet": return FromHtmlHexUnverified("#ee82ee");
                        case "wheat": return FromHtmlHexUnverified("#f5deb3");
                        case "white": return FromHtmlHexUnverified("#ffffff");
                        case "whitesmoke": return FromHtmlHexUnverified("#f5f5f5");
                        case "yellow": return FromHtmlHexUnverified("#ffff00");
                        case "yellowgreen": return FromHtmlHexUnverified("#9acd32");

                        default: throw new NotSupportedException($"Can not resolve CSS color: {trimmed}");
                    }
                }
            }

            // convert without safety checks
            private static ValueTuple<byte, byte, byte> FromHtmlHexUnverified(string value)
            {
                int r, g, b;
                r = Convert.ToInt32(value.Substring(1, 2), 16);
                g = Convert.ToInt32(value.Substring(3, 2), 16);
                b = Convert.ToInt32(value.Substring(5, 2), 16);
                return new ValueTuple<byte, byte, byte>((byte)r, (byte)g, (byte)b);
            }
        }
    }
}
