﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracViewWpf.Views
{
    public static class Constants
    {
        public static IReadOnlyList<double> ScrollValues = new List<double>()
        {
            1.0,
            1.1,
            1.21,
            1.33,
            1.46,
            1.61,
            1.8,
            2.0,
            2.2,
            2.4,
            2.6,
            2.8,
            3,
            3.33,
            3.66,
            4,
            4.25,
            4.5,
            4.75,
            5,
            5.5,
            6,
            6.5,
            7,
            8,
            9,
        }.AsReadOnly();
    }
}