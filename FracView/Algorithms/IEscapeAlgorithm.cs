﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    public interface IEscapeAlgorithm : IAlgorithm
    {
        bool UseHistogram { get; }

        int MaxIterations { get; }

        bool EvaluatePoints(CancellationToken token);
    }
}