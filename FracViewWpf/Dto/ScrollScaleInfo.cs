using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracViewWpf.Dto
{
    public record ScrollScaleInfo(double ContentHorizontalOffset, double ExtentWidth, double ContentVerticalOffset, double ExtentHeight, double DesiredSizeX, double DesiredSizeY)
    {
    }
}
