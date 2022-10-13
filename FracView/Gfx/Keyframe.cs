using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Gfx
{
    public class Keyframe<TKey, TInterval>
        where TKey : struct
        where TInterval : struct
    {
        public TInterval IntervalStart { get; set; }
        public TInterval IntervalEnd { get; set; }
        public TKey ValueStart { get; set; }
        public TKey ValueEnd { get; set; }

        public override string ToString()
        {
            return $"{IntervalStart}->{IntervalEnd}; {ValueStart}->{ValueEnd}";
        }
    }
}
