using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace FracViewCmd
{
    internal record CmdRunSettings
    {
        public DateTime RunTime { get; set; } = DateTime.Now;

        public SKEncodedImageFormat OutputFormat { get; set; }
    }
}
