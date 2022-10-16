using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace FracView.Gfx
{
    public class ColorRamp
    {
        public List<Keyframe<SKColor, double>> Keyframes { get; set; } = new List<Keyframe<SKColor, double>>();

        public ColorRamp Clone()
        {
            var keyframeClone = Keyframes.Select(KeyFrameClone).ToList();

            return new ColorRamp()
            {
                Keyframes = keyframeClone,
            };
        }

        public SKColor InterpolateFromKeyframes(double value)
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

            if (kf.IntervalEnd == kf.IntervalStart)
            {
                throw new DivideByZeroException("keyframe range is zero (start and end are same)");
            }

            var range = Math.Abs(kf.IntervalEnd - kf.IntervalStart);
            var scaledValue = (value - kf.IntervalStart) / range;

            int rval = (int)((double)(kf.ValueEnd.Red - kf.ValueStart.Red) * scaledValue) + kf.ValueStart.Red;
            if (rval < 0)
            {
                rval = 0;
            }
            else if (rval > 255)
            {
                rval = 255;
            }

            int gval = (int)((double)(kf.ValueEnd.Green - kf.ValueStart.Green) * scaledValue) + kf.ValueStart.Green;
            if (gval < 0)
            {
                gval = 0;
            }
            else if (gval > 255)
            {
                gval = 255;
            }

            int bval = (int)((double)(kf.ValueEnd.Blue - kf.ValueStart.Blue) * scaledValue) + kf.ValueStart.Blue;
            if (bval < 0)
            {
                bval = 0;
            }
            else if (bval > 255)
            {
                bval = 255;
            }

            return new SKColor((byte)rval, (byte)gval, (byte)bval);
        }

        private Keyframe<SKColor, double> KeyFrameClone(Keyframe<SKColor, double> kf)
        {
            return new Keyframe<SKColor, double>()
            {
                IntervalStart = kf.IntervalStart,
                IntervalEnd = kf.IntervalEnd,
                ValueStart = new SKColor((uint)kf.ValueStart),
                ValueEnd = new SKColor((uint)kf.ValueEnd),
            };
        }
    }
}
