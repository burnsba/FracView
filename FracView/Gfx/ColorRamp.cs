using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Gfx
{
    public class ColorRamp
    {
        public List<Keyframe<Color, double>> Keyframes { get; set; } = new List<Keyframe<Color, double>>();

        public Color InterpolateFromKeyframes(double value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 1)
            {
                value = 1;
            }

            var kf = Keyframes.FirstOrDefault(x => value >= x.IntervalStart && value <= x.IntervalEnd);
            if (object.ReferenceEquals(null, kf))
            {
                throw new InvalidOperationException("keyframe not found for value");
            }

            int rval = (int)((double)(kf.ValueEnd.R - kf.ValueStart.R) * value) + kf.ValueStart.R;
            if (rval < 0)
            {
                rval = 0;
            }
            else if (rval > 255)
            {
                rval = 255;
            }

            int gval = (int)((double)(kf.ValueEnd.G - kf.ValueStart.G) * value) + kf.ValueStart.G;
            if (gval < 0)
            {
                gval = 0;
            }
            else if (gval > 255)
            {
                gval = 255;
            }

            int bval = (int)((double)(kf.ValueEnd.B - kf.ValueStart.B) * value) + kf.ValueStart.B;
            if (bval < 0)
            {
                bval = 0;
            }
            else if (bval > 255)
            {
                bval = 255;
            }

            return Color.FromArgb(rval, gval, bval);
        }
    }
}
