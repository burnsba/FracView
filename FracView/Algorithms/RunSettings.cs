using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Algorithms
{
    public record RunSettings
    {
        public decimal OriginX { get; set; }
        public decimal OriginY { get; set; }
        public decimal FractalWidth { get; set; }
        public decimal FractalHeight { get; set; }
        public int StepWidth { get; set; }
        public int StepHeight { get; set; }
        public int MaxIterations { get; set; }
        public bool UseHistogram { get; set; }
    }
}
