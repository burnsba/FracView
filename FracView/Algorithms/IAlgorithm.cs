using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    public interface IAlgorithm
    {
        List<EvalComplexUnit> ConsideredPoints { get; }

        int StepWidth { get; }
        int StepHeight { get; }

        int ProgressCallbackIntervalSec { get; set; }
        Action<ProgressReport>? ProgressCallback { get; set; }
    }
}
