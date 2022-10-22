using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Algorithms;
using FracView.Dto;
using FracView.Gfx;
using SkiaSharp;

namespace FracView.Dto
{
    /// <summary>
    /// Container to store/load entire state of the application (at least what's worth saving).
    /// To save, values should be saved into this class, then serialized into json.
    /// To load, a json string should be read, then deserialized into this object, then restored to the appropriate places in the application.
    /// </summary>
    public class SessionSettings
    {
        /// <summary>
        /// Gets or sets algorithm run settings.
        /// </summary>
        public RunSettings RunSettings { get; set; } = new RunSettings();

        /// <summary>
        /// Gets or sets the current stable color.
        /// </summary>
        public string? StableColor { get; set; }
        
        /// <summary>
        /// Gets or sets coloramp keyframes.
        /// </summary>
        public List<ColorRampKeyframeJson>? ColorRampKeyframes { get; set; }

        /// <summary>
        /// Converts the string value of the color into the type used by the application.
        /// </summary>
        /// <returns></returns>
        public SKColor GetStableColor()
        {
            return SKColor.Parse(StableColor);
        }

        /// <summary>
        /// Saves the stable color value to a format that can be serialized to json.
        /// </summary>
        /// <param name="value"></param>
        public void SetStableColor(SKColor value)
        {
            StableColor = value.ToString();
        }

        /// <summary>
        /// Converts the saved value of the keyframes into the type used by the application.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">Thrown if ColorRampKeyframes isn't set.</exception>
        public List<Keyframe<SKColor, double>> GetColorRampKeyframes()
        {
            if (object.ReferenceEquals(null, ColorRampKeyframes))
            {
                throw new NullReferenceException(nameof(ColorRampKeyframes));
            }

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

        /// <summary>
        /// Converts coloramp keyframes into a value that can be serialized to json.
        /// </summary>
        /// <param name="value"></param>
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
