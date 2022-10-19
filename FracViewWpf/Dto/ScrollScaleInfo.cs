using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracViewWpf.Dto
{
    /// <summary>
    /// Container for communicating scrollview area size.
    /// </summary>
    /// <param name="ContentHorizontalOffset"></param>
    /// <param name="ExtentWidth"></param>
    /// <param name="ContentVerticalOffset"></param>
    /// <param name="ExtentHeight"></param>
    /// <param name="DesiredSizeX"></param>
    /// <param name="DesiredSizeY"></param>
    public record ScrollScaleInfo(double ContentHorizontalOffset, double ExtentWidth, double ContentVerticalOffset, double ExtentHeight, double DesiredSizeX, double DesiredSizeY)
    {
    }
}
