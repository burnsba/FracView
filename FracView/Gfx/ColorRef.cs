using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace FracView.Gfx
{
    /// <summary>
    /// Contains static reference colors used in the application.
    /// </summary>
    public class ColorRef
    {
        private static SKColor _black = new SKColor(0, 0, 0);
        private static SKColor _white = new SKColor(255, 255, 255);

        public static SKColor Black => _black;
        public static SKColor White => _white;
    }
}
