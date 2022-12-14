using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FracView.Dto
{
    /// <summary>
    /// Container for serializing keyframe data.
    /// <see cref="Gfx.ColorRamp.Keyframes"/>
    /// </summary>
    public record ColorRampKeyframeJson
    {
        public double IntervalStart { get; set; }
        public double IntervalEnd { get; set; }
        public string? ValueStart { get; set; }
        public string? ValueEnd { get; set; }
    }
}
