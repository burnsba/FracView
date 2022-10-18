using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    /// <summary>
    /// Interface to describe an escape algorithm.
    /// </summary>
    public interface IEscapeAlgorithm : IAlgorithm
    {
        /// <summary>
        /// Gets the max number of iterations to evaluate a point.
        /// </summary>
        int MaxIterations { get; }
    }
}
