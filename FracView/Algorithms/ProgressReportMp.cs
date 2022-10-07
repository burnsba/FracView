using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    public record ProgressReportMp<T>(double ElapsedSeconds, int CurrentStep, int TotalSteps, ComplexPointMp<T> CurrentPoint, string? CurrentWorkName) where T : struct, MultiPrecision.IConstant
    {
    }
}
