using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Gfx
{
    public class ColorRampEventArgs : EventArgs
    {
        public ColorRamp? ColorRamp { get; set; }

        public ColorRampEventArgs(ColorRamp cr)
        {
            ColorRamp = cr;
        }
    }
}
