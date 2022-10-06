﻿using System;
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

            int rval = (int)((double)(kf.ValueEnd.Red - kf.ValueStart.Red) * value) + kf.ValueStart.Red;
            if (rval < 0)
            {
                rval = 0;
            }
            else if (rval > 255)
            {
                rval = 255;
            }

            int gval = (int)((double)(kf.ValueEnd.Green - kf.ValueStart.Green) * value) + kf.ValueStart.Green;
            if (gval < 0)
            {
                gval = 0;
            }
            else if (gval > 255)
            {
                gval = 255;
            }

            int bval = (int)((double)(kf.ValueEnd.Blue - kf.ValueStart.Blue) * value) + kf.ValueStart.Blue;
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
    }
}
