using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Gfx
{
    /// <summary>
    /// Generic keyframe clas.
    /// </summary>
    /// <typeparam name="TKey">Type of value returned from range.</typeparam>
    /// <typeparam name="TInterval">Type the range is defined by.</typeparam>
    public class Keyframe<TKey, TInterval>
        where TKey : struct
        where TInterval : struct
    {
        /// <summary>
        /// Gets or sets the starting interval value.
        /// </summary>
        public TInterval IntervalStart { get; set; }

        /// <summary>
        /// Gets or sets the end interval value.
        /// </summary>
        public TInterval IntervalEnd { get; set; }

        /// <summary>
        /// Gets or sets the value used at the start of the interval.
        /// </summary>
        public TKey ValueStart { get; set; }

        /// <summary>
        /// Gets or sets the value used at the end of the interval.
        /// </summary>
        public TKey ValueEnd { get; set; }

        public override string ToString()
        {
            return $"{IntervalStart}->{IntervalEnd}; {ValueStart}->{ValueEnd}";
        }
    }
}
