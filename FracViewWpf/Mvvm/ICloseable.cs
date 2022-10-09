﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracViewWpf.Mvvm
{
    /// <summary>
    /// Specifies object (window) has close method.
    /// </summary>
    public interface ICloseable
    {
        /// <summary>
        /// Closes object (window).
        /// </summary>
        void Close();
    }
}
