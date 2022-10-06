using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    public record ProgressReport(double ElapsedSeconds, int CurrentStep, int TotalSteps, ComplexPoint CurrentPoint, string? CurrentWorkName)
    {
    }
}
