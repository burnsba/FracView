using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    /// <summary>
    /// Progress report container.
    /// </summary>
    /// <param name="ElapsedSeconds">Number of seconds that have elapsed since the task started.</param>
    /// <param name="CurrentStep">Current step being evaluated.</param>
    /// <param name="TotalSteps">Total number of steps to be evaluated.</param>
    /// <param name="CurrentWorkName">Description of the current task.</param>
    public record ProgressReport(double ElapsedSeconds, int CurrentStep, int TotalSteps, string? CurrentWorkName)
    {
    }
}
