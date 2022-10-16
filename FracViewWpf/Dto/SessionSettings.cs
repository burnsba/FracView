using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Algorithms;
using FracView.Gfx;
using SkiaSharp;

namespace FracViewWpf.Dto
{
    public class SessionSettings
    {
        public RunSettings RunSettings { get; set; } = new RunSettings();

        public string StableColor { get; set; }
        
        public List<ColorRampKeyframeJson> ColorRampKeyframes { get; set; }

        public SKColor GetStableColor()
        {
            return SKColor.Parse(StableColor);
        }

        public void SetStableColor(SKColor value)
        {
            StableColor = value.ToString();
        }

        public List<Keyframe<SKColor, double>> GetColorRampKeyframes()
        {
            var results = new List<Keyframe<SKColor, double>>();

            foreach (var kfstring in ColorRampKeyframes)
            {
                var kf = new Keyframe<SKColor, double>();

                kf.ValueStart = SKColor.Parse(kfstring.ValueStart);
                kf.ValueEnd = SKColor.Parse(kfstring.ValueEnd);
                kf.IntervalStart = kfstring.IntervalStart;
                kf.IntervalEnd = kfstring.IntervalEnd;

                results.Add(kf);
            }

            return results;
        }

        public void SetColorRampKeyframes(List<Keyframe<SKColor, double>> value)
        {
            ColorRampKeyframes = value
                .Select(x => new ColorRampKeyframeJson()
                {
                    ValueStart = x.ValueStart.ToString(),
                    ValueEnd = x.ValueEnd.ToString(),
                    IntervalStart = x.IntervalStart,
                    IntervalEnd = x.IntervalEnd,
                })
                .ToList();
        }
    }
}
