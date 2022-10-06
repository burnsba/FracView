using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    public record ProgressReport(double elapsedSeconds, int currentStep, int totalSteps, ComplexPoint currentPoint)
    {
    }
}
