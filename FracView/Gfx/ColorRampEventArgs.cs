using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Gfx
{
    /// <summary>
    /// Container for event information.
    /// </summary>
    public class ColorRampEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the ColorRamp associated with the event.
        /// </summary>
        public ColorRamp? ColorRamp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorRampEventArgs"/> class.
        /// </summary>
        /// <param name="ColorRamp">ColorRamp associated with the event.</param>
        public ColorRampEventArgs(ColorRamp cr)
        {
            ColorRamp = cr;
        }
    }
}
